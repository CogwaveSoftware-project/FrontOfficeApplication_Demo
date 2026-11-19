
using FrontOfficeApplication;
using FrontOfficeApplication.Helper.Security;
using FrontOfficeApplication.ProjectAPI.Middlewares;
using FrontOfficeApplication.ProjectInfrastructure.CheckInAndOut;
using FrontOfficeApplication.ProjectInfrastructure.DataBase;
using FrontOfficeApplication.ProjectInfrastructure.FileStorage;
using FrontOfficeApplication.ProjectInfrastructure.Master;
using FrontOfficeApplication.ProjectInfrastructure.ProductLicence;
using FrontOfficeApplication.ProjectInfrastructure.Reservation;
using FrontOfficeApplication.ProjectInfrastructure.UserAccessMaster;
using FrontOfficeApplication.ProjectServiceLayer;
using FrontOfficeApplication.ProjectServiceLayer.CheckInAndOut.Interfaces;
using FrontOfficeApplication.ProjectServiceLayer.CheckInAndOut.Services;
using FrontOfficeApplication.ProjectServiceLayer.FileStorage.Interfaces;
using FrontOfficeApplication.ProjectServiceLayer.FileStorage.Services;
using FrontOfficeApplication.ProjectServiceLayer.GST;
using FrontOfficeApplication.ProjectServiceLayer.Master.Interfaces;
using FrontOfficeApplication.ProjectServiceLayer.Master.Services;
using FrontOfficeApplication.ProjectServiceLayer.ProductLicence.Interfaces;
using FrontOfficeApplication.ProjectServiceLayer.ProductLicence.Services;
using FrontOfficeApplication.ProjectServiceLayer.Reservation.Interfaces;
using FrontOfficeApplication.ProjectServiceLayer.Reservation.Services;
using FrontOfficeApplication.ProjectServiceLayer.TaxCalculation;
using FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.Interfaces;
using FrontOfficeApplication.ProjectServiceLayer.UserAccessMaster.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//-----------------------------------------------------//

// ==============================
// 🔥 SERILOG CONFIGURATION
// ==============================

//Log.Logger = new LoggerConfiguration()
//    .ReadFrom.Configuration(builder.Configuration)
//    .Enrich.FromLogContext()
//    .WriteTo.Async(a => a.File("Logs/log-.txt",
//        rollingInterval: RollingInterval.Day))
//    .CreateLogger();

//builder.Host.UseSerilog(

//-----------------------------------------------------//

// Swagger (Development only)
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

//-----------------------------------------------------//

// =========================
// 🔐 JWT CONFIGURATION
// =========================

var jwtKey = builder.Configuration["Jwt:Key"]
?? throw new InvalidOperationException("JWT Key is not configured.");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    //options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

//-----------------------------------------------------//

// ========================
// 📦 Dependency Injection
// Add services to the container.
// =========================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSingleton<DBConnectionFactory>();
//builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IDbConnectionFactory, DBConnectionFactory>();
builder.Services.AddScoped<JwtTokenService>();

//------------Repository------------//
builder.Services.AddScoped<IUserAccessMaster_Repository, UserAccessMaster_DAL>();
builder.Services.AddScoped<IProductLicence_Repository, ProductLicence_DAL>();
builder.Services.AddScoped<IMaster_Repository, Master_DAL>();
builder.Services.AddScoped<IFileStorage_Repository, FileStorage_DAL>();
builder.Services.AddScoped<IReservation_Repository, Reservation_DAL>();
builder.Services.AddScoped<ICheckIn_Repository, CheckIn_DAL>();

//------------DALs------------//
//builder.Services.AddScoped<UserAccessMaster_DAL>();
//builder.Services.AddScoped<ProductLicence_DAL>();

//------------Services------------//
builder.Services.AddHttpClient();

builder.Services.AddScoped<IUserAccessMaster_Service, UserAccessMaster_Service>();
builder.Services.AddScoped<IProductLicence_Service, ProductLicence_Service>();
builder.Services.AddScoped<IMaster_Service, Master_Service>();
builder.Services.AddScoped<IFileStorage_Service, FileStorage_Service>();
builder.Services.AddScoped<IReservation_Service, Reservation_Service>();
builder.Services.AddScoped<ICheckIn_Service, CheckIn_Service>();
builder.Services.AddScoped<ProductLicence_Service>();
builder.Services.AddScoped<CommonMethod>();
builder.Services.AddScoped<EInvoiceGST>();
builder.Services.AddScoped<TaxCalculation_Service>();


//------------Appcongifuration------------//
//builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
//builder.Services.Configure<PhonePeSettings>(
//    builder.Configuration.GetSection("PhonePe"));


//builder.Services.ConfigureHttpJsonOptions(options =>
//{
//    //options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
//    options.SerializerOptions.PropertyNamingPolicy = null;
//});

//-----------------------------------------------------//


// =========================
// 🌍 CORS (For React)
// =========================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy =>
        {
            //policy.WithOrigins("http://localhost:5173") // React URL
            ////policy.WithOrigins("https://poswebsite.cogwave.in") // React URL
            //      .AllowAnyHeader()
            //      .AllowAnyMethod()
            //      .AllowCredentials();

            policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

//-----------------------------------------------------//

// =========================
// 🌍 ENVIRONMENT BASED Middleware Pipeline
// =========================

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("AllowReact");

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapControllers();

app.Run();

//-----------------------------------------------------//
