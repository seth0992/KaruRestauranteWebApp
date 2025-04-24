using Blazored.Toast;
using KaruRestauranteWebApp.Web;
using KaruRestauranteWebApp.Web.Authentication;
using KaruRestauranteWebApp.Web.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();

builder.Services.AddOutputCache();

// Configuración de autenticación para Blazor Server
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "ServerAuth";
    options.DefaultChallengeScheme = "ServerAuth";
})
.AddCookie("ServerAuth", options =>
{
    options.Cookie.Name = "BlazorServerAuth";
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
});

// Se agregar el CustomAuthStateProvider
builder.Services.AddAuthentication();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddControllers();

builder.Services.AddLocalization(); //Agregar la localizacion 
// Add Blazored Toast
builder.Services.AddBlazoredToast();

builder.Services.AddScoped<NotificationService>();

builder.Services.AddHttpClient<ApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        //client.BaseAddress = new("https+http://apiservice");
        client.BaseAddress = new(Environment.GetEnvironmentVariable("API_BASE_URL") ?? "https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();


#region Configuración para soporte multiidioma
var supportedCultures = new[] { "en-US", "es-ES", "es-CR" }; // Añade es-CR
var localizeoptions = new RequestLocalizationOptions()
    .SetDefaultCulture("es-CR") // Cambia el valor predeterminado a es-CR
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizeoptions);
#endregion


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
