using DotNetEnv;

// Configurations Context
using BuildTruckBack.Configurations.Application.Internal.CommandServices;
using BuildTruckBack.Configurations.Application.Internal.OutboundServices;
using BuildTruckBack.Configurations.Domain.Repositories;
using BuildTruckBack.Configurations.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckBack.Configurations.Domain.Model.Commands;

// Users Context
using BuildTruckBack.Users.Application.Internal.CommandServices;
using BuildTruckBack.Users.Application.Internal.QueryServices;
using BuildTruckBack.Users.Domain.Repositories;
using BuildTruckBack.Users.Domain.Services;
using BuildTruckBack.Users.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckBack.Users.Application.ACL.Services; 
using BuildTruckBack.Users.Application.Internal.OutboundServices;

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

// Auth Context (with alias to avoid conflicts)
using AuthUserContextService = BuildTruckBack.Auth.Application.ACL.Services.IUserContextService;
using BuildTruckBack.Auth.Application.Internal.CommandServices;
using BuildTruckBack.Auth.Application.Internal.QueryServices;
using BuildTruckBack.Auth.Application.Internal.OutboundServices;
using BuildTruckBack.Auth.Domain.Services;
using BuildTruckBack.Auth.Infrastructure.ACL;
using BuildTruckBack.Auth.Infrastructure.Tokens.JWT.Configuration;
using BuildTruckBack.Auth.Infrastructure.Tokens.JWT.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BuildTruckBack.Machinery.Application.ACL.Services;
using BuildTruckBack.Machinery.Application.Internal.CommandServices;
using BuildTruckBack.Machinery.Application.Internal.QueryServices;
using BuildTruckBack.Machinery.Domain.Repositories;
using BuildTruckBack.Machinery.Domain.Services;
using BuildTruckBack.Machinery.Infrastructure.ACL;
using BuildTruckBack.Machinery.Infrastructure.Persistence.EFC.Repositories;

// Projects Context (with alias to avoid conflicts)
using ProjectsUserContextService = BuildTruckBack.Projects.Application.ACL.Services.IUserContextService;
using ProjectsCloudinaryService = BuildTruckBack.Projects.Application.ACL.Services.ICloudinaryService;
using BuildTruckBack.Projects.Application.Internal.CommandServices;
using BuildTruckBack.Projects.Domain.Services;
using BuildTruckBack.Projects.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckBack.Projects.Infrastructure.ACL;
using BuildTruckBack.Projects.Interfaces.REST.Transform;

// Personnel Context
using BuildTruckBack.Personnel.Application.Internal.CommandServices;
using BuildTruckBack.Personnel.Application.Internal.QueryServices;
using BuildTruckBack.Personnel.Application.ACL.Services;
using BuildTruckBack.Personnel.Domain.Repositories;
using BuildTruckBack.Personnel.Domain.Services;
using BuildTruckBack.Personnel.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckBack.Personnel.Infrastructure.ACL;
using BuildTruckBack.Personnel.Infrastructure.Exports;
using BuildTruckBack.Projects.Application.Internal.OutboundServices;
using PersonnelCloudinaryService = BuildTruckBack.Personnel.Infrastructure.ACL.CloudinaryService;

// Materials Context
using BuildTruckBack.Materials.Application.Internal.CommandServices;
using BuildTruckBack.Materials.Application.Internal.QueryServices;
using BuildTruckBack.Materials.Domain.Repositories;
using BuildTruckBack.Materials.Domain.Services;
using BuildTruckBack.Materials.Infrastructure.Persistence.EFC.Repositories;

//Shared
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Services;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Exports.Configuration;

// Documentation Bounded Context
using BuildTruckBack.Documentation.Application.Internal.CommandServices;
using BuildTruckBack.Documentation.Application.Internal.QueryServices;
using BuildTruckBack.Documentation.Application.ACL.Services;
using BuildTruckBack.Documentation.Domain.Repositories;
using BuildTruckBack.Documentation.Domain.Services;
using BuildTruckBack.Documentation.Infrastructure.Persistence.EFC.Repositories;
using BuildTruckBack.Documentation.Infrastructure.ACL;
using BuildTruckBack.Documentation.Infrastructure.Exports;


// Incidents 
using BuildTruckBack.Incidents.Application.Internal;
using BuildTruckBack.Incidents.Domain.Commands;
using BuildTruckBack.Incidents.Domain.Model.Queries;
using Microsoft.Extensions.Options;
using DocumentationCloudinaryService = BuildTruckBack.Documentation.Infrastructure.ACL.CloudinaryService;



// ===== LOAD ENVIRONMENT VARIABLES =====
Env.Load();

var builder = WebApplication.CreateBuilder(args);

OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

// ===== CONFIGURATION =====
// Las variables de entorno se cargan automáticamente usando la sintaxis jerárquica
builder.Configuration.AddEnvironmentVariables();

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

// JWT Configuration
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

var tokenSettings = builder.Configuration.GetSection("TokenSettings").Get<TokenSettings>();
if (tokenSettings == null || !tokenSettings.IsValid())
{
    throw new InvalidOperationException("JWT TokenSettings are missing or invalid");
}

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
// Configurar Export Settings
builder.Services.Configure<ExportSettings>(
    builder.Configuration.GetSection("ExportSettings"));

// Registrar servicios de Export
builder.Services.AddScoped<IExcelGeneratorService, ExcelGeneratorService>();
builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();

// Shared Email Services (renamed to Generic)
builder.Services.AddScoped<IGenericEmailService, GenericEmailService>();

// Configure Cloudinary Settings
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

// Register Cloudinary Image Service (Shared)
builder.Services.AddScoped<ICloudinaryImageService, CloudinaryImageService>();

// Register Users Domain Image Service (ACL)
builder.Services.AddScoped<IImageService, ImageServiceAdapter>();

// Validar configuración de Cloudinary al inicio
var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
if (cloudinarySettings == null || !cloudinarySettings.IsValid)
{
    throw new InvalidOperationException(
        "Cloudinary settings are missing or invalid. Please check your appsettings.json file.");
}

builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IConfigurationCommandHandler, ConfigurationCommandService>();
builder.Services.AddScoped<IConfigurationFacade, ConfigurationFacade>();

// Users Bounded Context
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IUserFacade, UserFacade>();

// Users ACL Services
builder.Services.AddScoped<BuildTruckBack.Users.Application.ACL.Services.IEmailService, EmailServiceAdapter>();

// Auth Bounded Context
builder.Services.AddScoped<AuthUserContextService, BuildTruckBack.Auth.Infrastructure.ACL.UserContextService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IAuthCommandService, AuthCommandService>();
builder.Services.AddScoped<IAuthQueryService, AuthQueryService>();
builder.Services.AddScoped<IAuthFacade, AuthFacade>();

// Projects Bounded Context
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<IProjectCommandService, ProjectCommandService>();
builder.Services.AddScoped<ProjectResourceAssembler>();

// Personnel Bounded Context
builder.Services.AddScoped<IPersonnelRepository, PersonnelRepository>();
builder.Services.AddScoped<IPersonnelCommandService, PersonnelCommandService>();
builder.Services.AddScoped<IPersonnelQueryService, PersonnelQueryService>();
builder.Services.AddScoped<IProjectFacade, ProjectFacade>();

// Materials Bounded Context
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<IMaterialCommandService, MaterialCommandService>();
builder.Services.AddScoped<IMaterialQueryService, MaterialQueryService>();

// Material Entries
builder.Services.AddScoped<IMaterialEntryRepository, MaterialEntryRepository>();
builder.Services.AddScoped<IMaterialEntryCommandService, MaterialEntryCommandService>();
builder.Services.AddScoped<IMaterialEntryQueryService, MaterialEntryQueryService>();

// Material Usages
builder.Services.AddScoped<IMaterialUsageRepository, MaterialUsageRepository>();
builder.Services.AddScoped<IMaterialUsageCommandService, MaterialUsageCommandService>();
builder.Services.AddScoped<IMaterialUsageQueryService, MaterialUsageQueryService>();

// Materials ACL Services (para comunicación con otros bounded contexts)
builder.Services.AddScoped<BuildTruckBack.Materials.Application.ACL.Services.IProjectContextService, 
    BuildTruckBack.Materials.Infrastructure.ACL.ProjectContextService>();

builder.Services.AddScoped<BuildTruckBack.Materials.Application.ACL.Services.IUserContextService, 
    BuildTruckBack.Materials.Infrastructure.ACL.UserContextService>();

// Materials Facade (para que otros contexts puedan acceder a Materials)
builder.Services.AddScoped<BuildTruckBack.Materials.Application.Internal.OutboundServices.IMaterialFacade, 
    BuildTruckBack.Materials.Application.Internal.OutboundServices.MaterialFacade>();

// Materials Context Facade (para acceso externo)
builder.Services.AddScoped<BuildTruckBack.Materials.Interfaces.ACL.IMaterialsContextFacade, 
    BuildTruckBack.Materials.Interfaces.ACL.Services.MaterialsContextFacade>();

// ===== AGREGAR DESPUÉS DE LA LÍNEA DE HttpContextAccessor (si no existe) =====
builder.Services.AddHttpContextAccessor();


// Inventory Service
builder.Services.AddScoped<IInventoryQueryService, InventoryQueryService>();

// Projects ACL Services - Using aliases to avoid conflicts
builder.Services.AddScoped<ProjectsUserContextService, BuildTruckBack.Projects.Infrastructure.ACL.UserContextService>();

// Personnel ACL Services - Communication with other contexts
builder.Services.AddScoped<BuildTruckBack.Personnel.Application.ACL.Services.IProjectContextService, 
    BuildTruckBack.Personnel.Infrastructure.ACL.ProjectContextService>();

builder.Services.AddScoped<BuildTruckBack.Personnel.Application.ACL.Services.IUserContextService, 
    BuildTruckBack.Personnel.Infrastructure.ACL.UserContextService>();

// Projects Cloudinary Service - Create adapter that wraps shared service
builder.Services.AddScoped<ProjectsCloudinaryService>(provider =>
{
    var sharedCloudinaryService = provider.GetRequiredService<ICloudinaryImageService>();
    var logger = provider.GetRequiredService<ILogger<BuildTruckBack.Projects.Infrastructure.ACL.CloudinaryService>>();
    return new BuildTruckBack.Projects.Infrastructure.ACL.CloudinaryService(sharedCloudinaryService, logger);
});

// Personnel Cloudinary Service - Using alias to avoid conflicts
builder.Services.AddScoped<BuildTruckBack.Personnel.Application.ACL.Services.ICloudinaryService>(provider =>
{
    var sharedCloudinaryService = provider.GetRequiredService<ICloudinaryImageService>();
    var logger = provider.GetRequiredService<ILogger<PersonnelCloudinaryService>>();
    return new PersonnelCloudinaryService(sharedCloudinaryService, logger);
});

// Personnel Export Handler
builder.Services.AddScoped<PersonnelEntityExportHandler>();

builder.Services.AddScoped<IUniversalExportService>(provider =>
{
    var excelGenerator = provider.GetRequiredService<IExcelGeneratorService>();
    var pdfGenerator = provider.GetRequiredService<IPdfGeneratorService>();
    var settings = provider.GetRequiredService<IOptions<ExportSettings>>();
    var logger = provider.GetRequiredService<ILogger<UniversalExportService>>();
    
    var universalService = new UniversalExportService(excelGenerator, pdfGenerator, settings, logger);
    
    var personnelHandler = provider.GetRequiredService<PersonnelEntityExportHandler>();
    universalService.RegisterHandler(personnelHandler);
    
    return universalService;
});

// Documentation Bounded Context
builder.Services.AddScoped<IDocumentationRepository, DocumentationRepository>();
builder.Services.AddScoped<IDocumentationCommandService, DocumentationCommandService>();
builder.Services.AddScoped<IDocumentationQueryService, DocumentationQueryService>();

// Documentation ACL Services - Communication with other contexts
builder.Services.AddScoped<BuildTruckBack.Documentation.Application.ACL.Services.IProjectContextService, 
    BuildTruckBack.Documentation.Infrastructure.ACL.ProjectContextService>();

builder.Services.AddScoped<BuildTruckBack.Documentation.Application.ACL.Services.IUserContextService, 
    BuildTruckBack.Documentation.Infrastructure.ACL.UserContextService>();

// Documentation Cloudinary Service
builder.Services.AddScoped<BuildTruckBack.Documentation.Application.ACL.Services.ICloudinaryService>(provider =>
{
    var sharedCloudinaryService = provider.GetRequiredService<ICloudinaryImageService>();
    var logger = provider.GetRequiredService<ILogger<DocumentationCloudinaryService>>();
    return new DocumentationCloudinaryService(sharedCloudinaryService, logger);
});

// Documentation Export Handler
builder.Services.AddScoped<DocumentationExportHandler>();


// Incidents Bounded Context
builder.Services.AddScoped<IIncidentFacade, IncidentFacade>();
builder.Services.AddScoped<IIncidentCommandHandler, IncidentCommandHandler>();
builder.Services.AddScoped<IIncidentQueryHandler, IncidentQueryHandler>();
builder.Services.AddScoped<
    BuildTruckBack.Incidents.Application.ACL.Services.ICloudinaryService,
    BuildTruckBack.Incidents.Infrastructure.ACL.CloudinaryService
>();


// Machinery Bounded Context
builder.Services.AddScoped<IMachineryRepository, MachineryRepository>();
//builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProjectFacade, ProjectFacade>(); // Implement as needed
builder.Services.AddScoped<CreateMachineryCommandHandler>();
builder.Services.AddScoped<UpdateMachineryCommandHandler>();
builder.Services.AddScoped<DeleteMachineryCommandHandler>();

builder.Services.AddScoped<GetActiveMachineryQueryHandler>();
builder.Services.AddScoped<GetMachineryByIdQueryHandler>();
builder.Services.AddScoped<GetMachineryByProjectQueryHandler>();
// Register Command Service
builder.Services.AddScoped<IMachineryCommandService, MachineryCommandService>();
builder.Services.AddScoped<IMachineryQueryService, MachineryQueryService>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<ICloudinaryImageService, CloudinaryImageService>();
builder.Services.AddScoped<IMachineryCloudinaryService, MachineryCloudinaryService>();





var app = builder.Build();






// Verify if the database exists and create it if it doesn't
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    context.Database.EnsureCreated();
}


app.UseSwagger();
app.UseSwaggerUI();


// Apply CORS Policyy
app.UseCors("AllowAllPolicy");

app.UseAuthentication(); 
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapControllers();
app.Run();

