using Chatty2.Components.Pages;
using Chatty2.Models;
using Npgsql;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Chatty2.Services
{
    public class UTC_DB
    {
        public static string authority = "Leeds";

        public static short commandTimeout = 5;   // DB Query timeout
        public static string dbConnectionString = "Host=10.61.142.7;Database=UTC_Dev;Username=postgres;Password=Spring27fox;Timeout=5;";
        public static List<Message> MessageList = new List<Message>();  //

        public static event Action? MessagesChanged; // Raised when messages list or pin state changes
        private static void NotifyMessagesChanged() => MessagesChanged?.Invoke();

        public static async Task InsertMessageAsync(Message message)
        {
            try
            {
                await using var dataSource = NpgsqlDataSource.Create(dbConnectionString);

                //Insert some data
                await using (var cmd = dataSource.CreateCommand("INSERT INTO user_messages(username,usermessage,input_time,ispinned) VALUES((@p1),(@p2),(@p3),(@p4));"))
                {
                    cmd.CommandTimeout = commandTimeout;
                    cmd.Parameters.AddWithValue("p1", message.Username);
                    cmd.Parameters.AddWithValue("p2", message.Content);
                    cmd.Parameters.AddWithValue("p3", message.Timestamp);
                    cmd.Parameters.AddWithValue("p4", message.IsPinned);
                    await cmd.ExecuteNonQueryAsync();
                }
                dataSource.Dispose();
                Console.WriteLine("Function InsertMessage() - ENDED");

                // Notify all subscribers (other connected clients) that messages changed
                NotifyMessagesChanged();
            }
            catch (NpgsqlException MyNpgsqlException)// Catch Npsql Exception
            {
                Console.WriteLine("Npsql Exception in InsertMessageAsync() - " + MyNpgsqlException.ToString());
            }
            catch (Exception MyException)// Catch program exception
            {
                Console.WriteLine("Program Exception in InsertMessageAsync() - " + MyException.ToString());
            }
        }

        public static async Task UpdatePinnedStatus(int messageId)
        {
            try
            {
                await using var dataSource = NpgsqlDataSource.Create(dbConnectionString);

                await using (var cmd = dataSource.CreateCommand("UPDATE user_messages SET ispinned = NOT ispinned WHERE _id = @p1;"))
                {
                    cmd.CommandTimeout = commandTimeout;

                    // 3. Add the parameter for the message ID
                    cmd.Parameters.AddWithValue("p1", messageId);

                    // 4. Execute the command
                    await cmd.ExecuteNonQueryAsync();
                }
                dataSource.Dispose();

                // Notify after pin status change
                NotifyMessagesChanged();
            }
            catch (NpgsqlException MyNpgsqlException)// Catch Npsql Exception
            {
                Console.WriteLine("Npsql Exception in UpdatePinnedStatus() - " + MyNpgsqlException.ToString());
            }
            catch (Exception MyException)// Catch program exception
            {
                Console.WriteLine("Program Exception in UpdatePinnedStatus() - " + MyException.ToString());
            }
        }

        public static async Task<List<Message>> FetchMessagesAsync()
        {
            //Console.WriteLine("Function FetchMessagesAsync() - STARTED");

            try
            {
                await using var dataSource = NpgsqlDataSource.Create(dbConnectionString);

                // fetch all the data
                await using (var command = dataSource.CreateCommand("SELECT _id, username, usermessage, input_time, ispinned FROM user_messages ORDER BY _id DESC"))
                {
                    command.CommandTimeout = commandTimeout;

                    await using var reader = await command.ExecuteReaderAsync();

                    if (reader.HasRows == false)
                    {
                        Console.WriteLine("WARNING in FetchMessagesAsync() - No Data");
                    }
                    else
                    {
                        MessageList.Clear();

                        while (await reader.ReadAsync())
                        {
                            Message newMessage = new()
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Content = reader.GetString(2),
                                Timestamp = reader.GetDateTime(3),
                                IsPinned = reader.GetBoolean(4)
                            };
                            MessageList.Add(newMessage);
                        }
                        reader.Close();
                        //Console.WriteLine("Function GetMapScnList() - ENDED");
                        MessageList.Sort((x, y) => x.Id.CompareTo(y.Id));
                        //MessageList.ForEach(msg => Console.WriteLine($"msg id:{msg.Id} - user:{msg.Username} - msg:{msg.Content} - time:{msg.Timestamp} - pinned:{msg.IsPinned}"));
                    }
                }
                dataSource.Dispose();
                //Console.WriteLine("Function InsertMessage() - ENDED");
            }
            catch (NpgsqlException MyNpgsqlException)// Catch Npsql Exception
            {
                Console.WriteLine("Npsql Exception in FetchMessagesAsync() - " + MyNpgsqlException.ToString());
            }
            catch (Exception MyException)// Catch program exception
            {
                Console.WriteLine("Program Exception in FetchMessagesAsync() - " + MyException.ToString());
            }

            //Console.WriteLine("Function FetchMessagesAsync() - ENDED");
            return MessageList;
        }
    }
}
