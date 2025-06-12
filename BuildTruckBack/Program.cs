// Users Context
using BuildTruckBack.Users.Application.Internal.CommandServices;
using BuildTruckBack.Users.Application.Internal.QueryServices;
using BuildTruckBack.Users.Domain.Repositories;
using BuildTruckBack.Users.Domain.Services;
using BuildTruckBack.Users.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckBack.Users.Application.ACL.Services; 
using BuildTruckBack.Users.Application.ACL.Services;

// Shared Context
using BuildTruckBack.Shared.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.Interfaces.ASP.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Configuration;
using BuildTruckBack.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Email.Configuration;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Email.Services;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Cloudinary.Configuration;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Cloudinary.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

if (connectionString == null) throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    else if (builder.Environment.IsProduction())
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Error);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "BuildTruckBack.API",
            Version = "v1",
            Description = "BuildTruck Platform API",
            TermsOfService = new Uri("https://buildtruck.com/tos"),
            Contact = new OpenApiContact
            {
                Name = "BuildTruck Team",
                Email = "contact@buildtruck.com"
            },
            License = new OpenApiLicense
            {
                Name = "Apache 2.0",
                Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
            }
        });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
    options.EnableAnnotations();
});

// ===== DEPENDENCY INJECTION =====

// Shared Bounded Context - Infrastructure
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// ✅ Shared Email Services (renamed to Generic)
builder.Services.AddScoped<IGenericEmailService, GenericEmailService>();
// 📦 Configure Cloudinary Settings
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
// 📦 Register Cloudinary Image Service (Shared)
builder.Services.AddScoped<ICloudinaryImageService, CloudinaryImageService>();

// 📦 Register Users Domain Image Service (ACL)
builder.Services.AddScoped<IImageService, ImageServiceAdapter>();

// ✅ Validar configuración de Cloudinary al inicio
var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
if (cloudinarySettings == null || !cloudinarySettings.IsValid)
{
    throw new InvalidOperationException(
        "Cloudinary settings are missing or invalid. Please check your appsettings.json file.");
}


// ✅ Users Bounded Context
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();

// ✅ Users ACL Services
builder.Services.AddScoped<BuildTruckBack.Users.Application.ACL.Services.IEmailService, EmailServiceAdapter>();

var app = builder.Build();

// Verify if the database exists and create it if it doesn't
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply CORS Policy
app.UseCors("AllowAllPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();