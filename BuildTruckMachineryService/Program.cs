using DotNetEnv;
using BuildTruckShared.Domain.Repositories;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Configuration;
using BuildTruckShared.Infrastructure.ExternalServices.Cloudinary.Services;
using BuildTruckShared.Infrastructure.Interfaces.ASP.Configuration;
using BuildTruckShared.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckMachineryService.Machinery.Application.ACL.Services;
using BuildTruckMachineryService.Machinery.Application.Internal.CommandServices;
using BuildTruckMachineryService.Machinery.Application.Internal.OutboundServices;
using BuildTruckMachineryService.Machinery.Domain.Services;
using BuildTruckMachineryService.Machinery.Infrastructure.ACL;
using BuildTruckMachineryService.Machinery.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckMachineryService.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckMachineryService.Shared.Infrastructure.Tokens.JWT;
using BuildTruckMachineryService.Users.Application.Internal.OutboundServices;
using BuildTruckMachineryService.Users.Infrastructure.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

using MachineryUserContextService = BuildTruckMachineryService.Machinery.Application.ACL.Services.IUserContextService;
using MachineryCloudinaryService = BuildTruckMachineryService.Machinery.Application.ACL.Services.ICloudinaryService;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// ===== ROUTING & CONTROLLERS =====
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options =>
    options.Conventions.Add(new KebabCaseRouteNamingConvention()));

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:8080",
                "http://localhost:3000",
                "https://buildtruck-99bc0.web.app",
                "https://buildtruck-99bc0.firebaseapp.com",
                "https://buildtruck.netlify.app",
                "https://a160c5e0-b357-45c1-952c-1a8cc886f536.cfargotunnel.com",
                "https://guitars-talk-seasonal-annoying.trycloudflare.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ===== DATABASE =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ProjectServiceDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    else
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Error);
});

// ===== JWT =====
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

var tokenSettings = builder.Configuration.GetSection("TokenSettings").Get<TokenSettings>();
if (tokenSettings == null || !tokenSettings.IsValid())
    throw new InvalidOperationException("JWT TokenSettings are missing or invalid");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = tokenSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.SecretKey)),
            ValidateIssuer = tokenSettings.ValidateIssuer,
            ValidIssuer = tokenSettings.Issuer,
            ValidateAudience = tokenSettings.ValidateAudience,
            ValidAudience = tokenSettings.Audience,
            ValidateLifetime = tokenSettings.ValidateLifetime,
            ClockSkew = TimeSpan.FromMinutes(tokenSettings.ClockSkewMinutes)
        };
    });

// ===== SWAGGER =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "BuildTruck Project Service API", Version = "v1" });
    options.EnableAnnotations();
});

// ===== SHARED SERVICES =====
builder.Services.AddScoped<IUnitOfWork>(provider =>
    new UnitOfWork<ProjectServiceDbContext>(provider.GetRequiredService<ProjectServiceDbContext>()));

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
if (cloudinarySettings == null || !cloudinarySettings.IsValid)
    throw new InvalidOperationException("Cloudinary settings are missing or invalid.");
builder.Services.AddScoped<ICloudinaryImageService, CloudinaryImageService>();

// ===== HTTP CLIENT — UserService =====
builder.Services.AddHttpClient("UserService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["UserService:BaseUrl"] ?? "http://buildtruck-user-service:8080");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IUserFacade, HttpUserFacade>();

// ===== PROJECTS BOUNDED CONTEXT =====
builder.Services.AddScoped<ProjectRepository>(provider =>
    new ProjectRepository(provider.GetRequiredService<ProjectServiceDbContext>()));
builder.Services.AddScoped<IProjectCommandService, ProjectCommandService>();
builder.Services.AddScoped<IProjectFacade, ProjectFacade>();

builder.Services.AddScoped<MachineryUserContextService, UserContextService>();
builder.Services.AddScoped<MachineryCloudinaryService>(provider =>
{
    var sharedCloudinary = provider.GetRequiredService<ICloudinaryImageService>();
    var logger = provider.GetRequiredService<ILogger<BuildTruckMachineryService.Machinery.Infrastructure.ACL.CloudinaryService>>();
    return new BuildTruckMachineryService.Machinery.Infrastructure.ACL.CloudinaryService(sharedCloudinary, logger);
});
builder.Services.AddScoped<BuildTruckMachineryService.Machinery.Interfaces.REST.Transform.ProjectResourceAssembler>();

builder.Services.AddHttpContextAccessor();

// ===== BUILD APP =====
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProjectServiceDbContext>();
    try
    {
        var creator = context.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
        creator.CreateTables();
    }
    catch { }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAllPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
