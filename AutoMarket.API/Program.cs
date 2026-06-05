using System.Text;
using System.Text.Json.Serialization;
using AutoMarket.API.Middleware;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using AutoMarket.Infrastructure.Repositories;
using AutoMarket.Services.Auth;
using AutoMarket.Services.Enquiries;
using AutoMarket.Services.Listings;
using AutoMarket.Services.Mappings;
using AutoMarket.Services.Nhtsa;
using AutoMarket.Services.Reviews;
using AutoMarket.Services.ServiceHistory;
using AutoMarket.Services.Vehicles;
using AutoMarket.Services.Watchlist;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 2. DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/automarket-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30));

// 4. JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer   = true,
            ValidIssuer      = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience    = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew        = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// 5. AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMarketMappingProfile));

// 6. HttpClient for NHTSA
builder.Services.AddHttpClient<INhtsaService, NhtsaService>();

// 7. Repositories (Scoped)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IServiceRecordRepository, ServiceRecordRepository>();
builder.Services.AddScoped<IWatchlistRepository, WatchlistRepository>();
builder.Services.AddScoped<IEnquiryRepository, EnquiryRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// 8. Services (Scoped)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IServiceHistoryService, ServiceHistoryService>();
builder.Services.AddScoped<IWatchlistService, WatchlistService>();
builder.Services.AddScoped<IEnquiryService, EnquiryService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// 9. Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AutoMarket API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 10. Middleware pipeline (ORDER MATTERS)
app.UseMiddleware<ExceptionMiddleware>();   // must be first
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();                   // before UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();
