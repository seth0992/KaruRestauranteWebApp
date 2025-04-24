using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text.Json.Serialization;
using System.Text;
using KaruRestauranteWebApp.BL.Repositories;
using KaruRestauranteWebApp.BL.Services;
using KaruRestauranteWebApp.Database.Data;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// Add controllers to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add DbContext to the container.
//builder.Services.AddDbContext<AppDbContext>(options =>
//{
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection")

//        );
//});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions =>
        {
            // Esta opción ayuda a manejar tablas con triggers
            sqlServerOptions.UseRelationalNulls();

            // Puedes agregar otras opciones útiles
            sqlServerOptions.EnableRetryOnFailure();
            sqlServerOptions.CommandTimeout(30); // Opcional: aumentar timeout
        }
    );
});


//Add categories Services and repository
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

//Add Invetory Services and repository 
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

//Add Combos Services and repository
builder.Services.AddScoped<IComboRepository, ComboRepository>();
builder.Services.AddScoped<IComboService, ComboService>();

//Add FastFood Services and repository
builder.Services.AddScoped<IFastFoodRepository, FastFoodRepository>();
builder.Services.AddScoped<IFastFoodService, FastFoodService>();

//Add roductType Services and repository
builder.Services.AddScoped<IProductTypeRepository, ProductTypeRepository>();
builder.Services.AddScoped<IProductTypeService, ProductTypeService>();

// Añadir Customer Services y repository
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Añadir Ordens Services y repository
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Añadir Table Services y repository
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<ITableService, TableService>();

// Añadir ElectroniInvoice Services y repository
builder.Services.AddScoped<IElectronicInvoiceRepository, ElectronicInvoiceRepository>();
builder.Services.AddScoped<IElectronicInvoiceService, ElectronicInvoiceService>();

// Añadir ElectroniInvoice Services y repository
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Añadir CashRegister Services y repository
builder.Services.AddScoped<ICashRegisterSessionRepository, CashRegisterSessionRepository>();
builder.Services.AddScoped<ICashRegisterTransactionRepository, CashRegisterTransactionRepository>();
builder.Services.AddScoped<ICashRegisterSessionService, CashRegisterSessionService>();
builder.Services.AddScoped<ICashRegisterTransactionService, CashRegisterTransactionService>();

//Add User services and Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserServices>();

//Add Roles Services and repository
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();

// Añadir ProductInventory Services y repository
builder.Services.AddScoped<IProductInventoryRepository, ProductInventoryRepository>();
builder.Services.AddScoped<IProductInventoryService, ProductInventoryService>();

// Añadir Reportes Services y repository
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();

//Add authentication services
var secret = builder.Configuration.GetValue<string>("Jwt:Secret");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "doseHiue",
        ValidAudience = "doseHiue",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };

});

builder.Services.AddAuthorization();

builder.Services.AddControllers().AddJsonOptions(options =>
   options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);


// Agregar los servicios de authenticacion
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Agregar la localización para el cambio de idioma
builder.Services.AddLocalization();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// add middleware autentication
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.MapControllers(); //Agregado para el uso de controladores

app.MapDefaultEndpoints();

app.Run();

