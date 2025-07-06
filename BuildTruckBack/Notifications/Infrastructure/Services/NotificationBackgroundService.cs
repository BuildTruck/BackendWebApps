using BuildTruckBack.Notifications.Interfaces.ACL;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Notifications.Domain.Services;

namespace BuildTruckBack.Notifications.Infrastructure.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;

    public NotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<NotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔔 Notification Background Service iniciado - Verificaciones cada 24 horas");

        // Esperar 2 minutos al inicio para que todo esté listo
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var now = DateTime.Now;
                
                _logger.LogInformation("🕐 Iniciando verificaciones diarias - {Time}", now);

                await ExecuteDailyChecks(scope.ServiceProvider);
                
                _logger.LogInformation("✅ Verificaciones diarias completadas exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en verificaciones diarias");
            }

            // Esperar 24 horas hasta la próxima ejecución
            await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
        }
    }

    private async Task ExecuteDailyChecks(IServiceProvider serviceProvider)
    {
        try
        {
            // Verificar proyectos activos y sus alertas
            await CheckActiveProjects(serviceProvider);
            
            // Verificar stock bajo de materiales
            await CheckLowStockMaterials(serviceProvider);
            
            // Verificar maquinaria que necesita atención
            await CheckMachineryStatus(serviceProvider);
            
            // Verificar incidentes abiertos que necesitan seguimiento
            await CheckOpenIncidents(serviceProvider);
            
            // Verificar personal activo y asistencia
            await CheckPersonnelActivity(serviceProvider);
            
            // Reintentar notificaciones fallidas
            await RetryFailedNotifications(serviceProvider);
            
            // Enviar resúmenes diarios por email
            await SendDailyDigests(serviceProvider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando verificaciones diarias");
        }
    }

    private async Task CheckActiveProjects(IServiceProvider serviceProvider)
    {
        try
        {
            var notificationFacade = serviceProvider.GetRequiredService<INotificationContextFacade>();
            var projectService = serviceProvider.GetRequiredService<IProjectContextService>();

            _logger.LogInformation("📅 Verificando proyectos activos...");

            var activeProjects = await projectService.GetAllActiveProjectsAsync();
            var alertCount = 0;

            foreach (var projectId in activeProjects)
            {
                // Verificar si el proyecto existe y obtener información
                if (await projectService.ProjectExistsAsync(projectId))
                {
                    var projectName = await projectService.GetProjectNameAsync(projectId);
                    var managerId = await projectService.GetProjectManagerIdAsync(projectId);

                    if (managerId > 0)
                    {
                        // Ejemplo: Crear notificación de seguimiento semanal
                        if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                        {
                            await notificationFacade.CreateNotificationForUserAsync(
                                userId: managerId,
                                type: NotificationType.ProjectStatusChanged,
                                context: NotificationContext.Projects,
                                title: "📊 Revisión Semanal de Proyecto",
                                message: $"Es momento de revisar el progreso del proyecto '{projectName}'.",
                                priority: NotificationPriority.Normal,
                                actionUrl: $"/projects/{projectId}",
                                relatedProjectId: projectId
                            );
                            alertCount++;
                        }
                    }
                }
            }

            _logger.LogInformation("✅ Proyectos verificados: {Count} alertas generadas", alertCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando proyectos activos");
        }
    }

    private async Task CheckLowStockMaterials(IServiceProvider serviceProvider)
    {
        try
        {
            var notificationFacade = serviceProvider.GetRequiredService<INotificationContextFacade>();
            var materialService = serviceProvider.GetRequiredService<IMaterialContextService>();
            var projectService = serviceProvider.GetRequiredService<IProjectContextService>();

            _logger.LogInformation("📦 Verificando stock de materiales...");

            var activeProjects = await projectService.GetAllActiveProjectsAsync();
            var lowStockCount = 0;

            foreach (var projectId in activeProjects)
            {
                var lowStockMaterialIds = await materialService.GetLowStockMaterialsAsync(projectId);
                
                if (lowStockMaterialIds.Any())
                {
                    var managerId = await projectService.GetProjectManagerIdAsync(projectId);
                    var projectName = await projectService.GetProjectNameAsync(projectId);
                    
                    if (managerId > 0)
                    {
                        foreach (var materialId in lowStockMaterialIds)
                        {
                            var materialName = await materialService.GetMaterialNameAsync(materialId);
                            var currentStock = await materialService.GetMaterialStockAsync(materialId);
                            var minimumStock = await materialService.GetMaterialMinimumStockAsync(materialId);
                            
                            var isCritical = currentStock <= (minimumStock * 0.5m); // 50% del mínimo es crítico
                            
                            await notificationFacade.CreateNotificationForUserAsync(
                                userId: managerId,
                                type: isCritical ? NotificationType.CriticalStock : NotificationType.LowStock,
                                context: NotificationContext.Materials,
                                title: isCritical ? "🚨 Stock Crítico" : "📦 Stock Bajo",
                                message: $"Material '{materialName}' en proyecto '{projectName}': {currentStock} unidades (mínimo: {minimumStock}).",
                                priority: isCritical ? NotificationPriority.Critical : NotificationPriority.Normal,
                                actionUrl: $"/materials/{materialId}",
                                relatedProjectId: projectId,
                                relatedEntityId: materialId,
                                relatedEntityType: "Material"
                            );
                            lowStockCount++;
                        }
                    }
                }
            }

            _logger.LogInformation("✅ Materiales verificados: {Count} alertas de stock bajo", lowStockCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando stock de materiales");
        }
    }

    private async Task CheckMachineryStatus(IServiceProvider serviceProvider)
    {
        try
        {
            var notificationFacade = serviceProvider.GetRequiredService<INotificationContextFacade>();
            var machineryService = serviceProvider.GetRequiredService<IMachineryContextService>();
            var projectService = serviceProvider.GetRequiredService<IProjectContextService>();

            _logger.LogInformation("🔧 Verificando estado de maquinaria...");

            var activeProjects = await projectService.GetAllActiveProjectsAsync();
            var machineryAlerts = 0;

            foreach (var projectId in activeProjects)
            {
                var activeMachineryCount = await machineryService.GetActiveMachineryCountAsync(projectId);
                var managerId = await projectService.GetProjectManagerIdAsync(projectId);
                var projectName = await projectService.GetProjectNameAsync(projectId);
                
                // Si hay pocas máquinas activas, alertar
                if (activeMachineryCount < 2 && managerId > 0)
                {
                    await notificationFacade.CreateNotificationForUserAsync(
                        userId: managerId,
                        type: NotificationType.MachineryStatusChanged,
                        context: NotificationContext.Machinery,
                        title: "⚠️ Pocas Máquinas Activas",
                        message: $"El proyecto '{projectName}' solo tiene {activeMachineryCount} máquinas activas. Considere revisar disponibilidad.",
                        priority: NotificationPriority.Normal,
                        actionUrl: $"/machinery?project={projectId}",
                        relatedProjectId: projectId
                    );
                    machineryAlerts++;
                }
            }

            _logger.LogInformation("✅ Maquinaria verificada: {Count} alertas generadas", machineryAlerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando estado de maquinaria");
        }
    }

    private async Task CheckOpenIncidents(IServiceProvider serviceProvider)
    {
        try
        {
            var notificationFacade = serviceProvider.GetRequiredService<INotificationContextFacade>();
            var incidentService = serviceProvider.GetRequiredService<IIncidentContextService>();
            var projectService = serviceProvider.GetRequiredService<IProjectContextService>();

            _logger.LogInformation("🚨 Verificando incidentes abiertos...");

            var activeProjects = await projectService.GetAllActiveProjectsAsync();
            var incidentAlerts = 0;

            foreach (var projectId in activeProjects)
            {
                var openIncidentsCount = await incidentService.GetOpenIncidentsCountAsync(projectId);
                
                // Si hay muchos incidentes abiertos, alertar
                if (openIncidentsCount > 5)
                {
                    var managerId = await projectService.GetProjectManagerIdAsync(projectId);
                    var projectName = await projectService.GetProjectNameAsync(projectId);
                    
                    if (managerId > 0)
                    {
                        await notificationFacade.CreateNotificationForUserAsync(
                            userId: managerId,
                            type: NotificationType.IncidentReported,
                            context: NotificationContext.Incidents,
                            title: "📊 Muchos Incidentes Abiertos",
                            message: $"El proyecto '{projectName}' tiene {openIncidentsCount} incidentes abiertos. Requiere atención.",
                            priority: NotificationPriority.High,
                            actionUrl: $"/incidents?project={projectId}",
                            relatedProjectId: projectId
                        );
                        incidentAlerts++;
                    }
                }
            }

            _logger.LogInformation("✅ Incidentes verificados: {Count} alertas generadas", incidentAlerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando incidentes abiertos");
        }
    }

    private async Task CheckPersonnelActivity(IServiceProvider serviceProvider)
    {
        try
        {
            var notificationFacade = serviceProvider.GetRequiredService<INotificationContextFacade>();
            var personnelService = serviceProvider.GetRequiredService<IPersonnelContextService>();
            var projectService = serviceProvider.GetRequiredService<IProjectContextService>();

            _logger.LogInformation("👷 Verificando actividad del personal...");

            var activeProjects = await projectService.GetAllActiveProjectsAsync();
            var personnelAlerts = 0;

            foreach (var projectId in activeProjects)
            {
                var attendanceRate = await personnelService.GetAttendanceRateAsync(projectId);
                
                // Si la tasa de asistencia es baja, alertar
                if (attendanceRate < 0.8m) // Menos del 80%
                {
                    var managerId = await projectService.GetProjectManagerIdAsync(projectId);
                    var projectName = await projectService.GetProjectNameAsync(projectId);
                    
                    if (managerId > 0)
                    {
                        await notificationFacade.CreateNotificationForUserAsync(
                            userId: managerId,
                            type: NotificationType.AttendanceAlert,
                            context: NotificationContext.Personnel,
                            title: "📉 Baja Asistencia del Personal",
                            message: $"El proyecto '{projectName}' tiene una tasa de asistencia del {attendanceRate:P1}. Revisar asistencias.",
                            priority: NotificationPriority.Normal,
                            actionUrl: $"/personnel?project={projectId}",
                            relatedProjectId: projectId
                        );
                        personnelAlerts++;
                    }
                }
            }

            _logger.LogInformation("✅ Personal verificado: {Count} alertas de asistencia", personnelAlerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando actividad del personal");
        }
    }

    private async Task RetryFailedNotifications(IServiceProvider serviceProvider)
    {
        try
        {
            var deliveryService = serviceProvider.GetRequiredService<INotificationDeliveryService>();
            
            _logger.LogInformation("🔄 Reintentando notificaciones fallidas...");
            
            await deliveryService.RetryFailedDeliveriesAsync();
            
            _logger.LogInformation("✅ Reintentos de notificaciones completados");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reintentando notificaciones fallidas");
        }
    }

    private async Task SendDailyDigests(IServiceProvider serviceProvider)
    {
        try
        {
            var userService = serviceProvider.GetRequiredService<IUserContextService>();
            var emailService = serviceProvider.GetRequiredService<ISharedEmailService>();
            var notificationRepository = serviceProvider.GetRequiredService<
                BuildTruckBack.Notifications.Domain.Repositories.INotificationRepository>();

            _logger.LogInformation("📧 Enviando resúmenes diarios por email...");

            // Obtener usuarios activos (simplificado - en producción podrías tener un método específico)
            var adminUsers = await userService.GetAdminUsersAsync();
            var managerUsers = await userService.GetManagerUsersAsync();
            var allUsers = adminUsers.Concat(managerUsers).Distinct();

            var digestCount = 0;

            foreach (var userId in allUsers)
            {
                if (await userService.IsUserActiveAsync(userId))
                {
                    // Obtener notificaciones del día anterior
                    var yesterdayNotifications = await notificationRepository.FindByUserIdWithFiltersAsync(
                        userId, 1, 50, null, null, null, null);
                    
                    var todayNotifications = yesterdayNotifications
                        .Where(n => n.CreatedDate?.Date == DateTime.Now.AddDays(-1).Date)
                        .ToList();

                    if (todayNotifications.Any())
                    {
                        var userEmail = await userService.GetUserEmailAsync(userId);
                        var userName = await userService.GetUserNameAsync(userId);

                        if (!string.IsNullOrEmpty(userEmail))
                        {
                            await emailService.SendDigestEmailAsync(
                                userEmail, 
                                userName, 
                                todayNotifications, 
                                DateTime.Now.AddDays(-1)
                            );
                            digestCount++;
                        }
                    }
                }
            }

            _logger.LogInformation("✅ Resúmenes diarios enviados: {Count} emails", digestCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando resúmenes diarios");
        }
    }
}