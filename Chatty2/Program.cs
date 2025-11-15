using Chatty2.Components;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// **ACCESS AND SET STATIC FIELD HERE**
var authorityValue = builder.Configuration["Authority"];
Chatty2.Services.UTC_DB.authority = authorityValue; // Set the static field

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Radzen services (for NotificationService, etc.)
builder.Services.AddRadzenComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
