using BuildTruckBack.Notifications.Application.ACL.Services;
using BuildTruckBack.Notifications.Domain.Model.Aggregates;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Email.Services;

namespace BuildTruckBack.Notifications.Infrastructure.ACL;

public class SharedEmailService : ISharedEmailService
{
    private readonly IGenericEmailService _genericEmailService;

    public SharedEmailService(IGenericEmailService genericEmailService)
    {
        _genericEmailService = genericEmailService;
    }

    public async Task SendNotificationEmailAsync(string email, string fullName, NotificationType type,
        string title, string message, string? actionUrl = null)
    {
        var subject = $"🔔 BuildTruck - {title}";
        var htmlBody = CreateNotificationEmailTemplate(fullName, type, title, message, actionUrl);

        await _genericEmailService.SendEmailAsync(email, subject, htmlBody);
    }

    public async Task SendDigestEmailAsync(string email, string fullName, List<Notification> notifications,
        DateTime date)
    {
        var subject = $"📋 BuildTruck - Resumen diario {date:dd/MM/yyyy}";
        var htmlBody = CreateDigestEmailTemplate(fullName, notifications, date);

        await _genericEmailService.SendEmailAsync(email, subject, htmlBody);
    }

    public async Task SendCriticalNotificationEmailAsync(string email, string fullName, string title,
        string message, string projectName, string? actionUrl = null)
    {
        var subject = $"🚨 BuildTruck - Notificación Crítica: {title}";
        var htmlBody = CreateCriticalNotificationTemplate(fullName, title, message, projectName, actionUrl);

        await _genericEmailService.SendEmailAsync(email, subject, htmlBody);
    }

    private static string CreateNotificationEmailTemplate(string fullName, NotificationType type,
        string title, string message, string? actionUrl)
    {
        var actionButton = !string.IsNullOrWhiteSpace(actionUrl)
            ? $@"<div style='text-align: center; margin: 20px 0;'>
                    <a href='{actionUrl}' style='background: #f97316; color: white; padding: 12px 24px; 
                       text-decoration: none; border-radius: 6px; font-weight: bold;'>Ver detalles</a>
                 </div>"
            : string.Empty;

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>{title} - BuildTruck</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #f97316, #ea580c); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>🔔 BuildTruck</h1>
        <p style='color: white; margin: 10px 0 0 0; font-size: 16px;'>Notificación del Sistema</p>
    </div>
    
    <div style='background: white; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #f97316; margin-top: 0;'>{title}</h2>
        
        <p>Hola <strong>{fullName}</strong>,</p>
        
        <div style='background: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0;'>
            <p style='margin: 0; font-size: 16px;'>{message}</p>
        </div>
        
        {actionButton}
        
        <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
        
        <p style='color: #6b7280; font-size: 14px; text-align: center;'>
            BuildTruck - Sistema de Gestión de Construcción<br>
            Este email fue enviado automáticamente, no responder.
        </p>
    </div>
</body>
</html>";
    }

    private static string CreateDigestEmailTemplate(string fullName, List<Notification> notifications, DateTime date)
    {
        var notificationsList = string.Join("", notifications.Take(10).Select(n =>
            $@"<li style='margin-bottom: 15px; padding: 10px; background: #f9fafb; border-radius: 6px;'>
                  <strong style='color: #374151;'>{n.Content.Title}</strong><br>
                  <span style='color: #6b7280; font-size: 14px;'>{n.Content.Message}</span><br>
                  <span style='color: #9ca3af; font-size: 12px;'>{n.Context.Value} - {n.CreatedDate:HH:mm}</span>
               </li>"));

        var moreCount = notifications.Count > 10 ? notifications.Count - 10 : 0;
        var moreText = moreCount > 0 ? $"<p>Y {moreCount} notificaciones más...</p>" : "";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Resumen Diario - BuildTruck</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #3b82f6, #1d4ed8); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0; font-size: 28px;'>📋 BuildTruck</h1>
        <p style='color: white; margin: 10px 0 0 0; font-size: 16px;'>Resumen Diario</p>
    </div>
    
    <div style='background: white; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #3b82f6; margin-top: 0;'>Resumen del {date:dd/MM/yyyy}</h2>
        
        <p>Hola <strong>{fullName}</strong>,</p>
        
        <p>Aquí tienes el resumen de tus notificaciones del día:</p>
        
        <ul style='list-style: none; padding: 0;'>
            {notificationsList}
        </ul>
        
        {moreText}
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='http://localhost:3000/notifications' style='background: #3b82f6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;'>Ver todas las notificaciones</a>
        </div>
        
        <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
        
        <p style='color: #6b7280; font-size: 14px; text-align: center;'>
            BuildTruck - Sistema de Gestión de Construcción<br>
            Este email fue enviado automáticamente, no responder.
        </p>
    </div>
</body>
</html>";
    }

    private static string CreateCriticalNotificationTemplate(string fullName, string title,
        string message, string projectName, string? actionUrl)
    {
        var actionButton = !string.IsNullOrWhiteSpace(actionUrl)
            ? $@"<div style='text-align: center; margin: 20px 0;'>
               <a href='{actionUrl}' style='background: #dc2626; color: white; padding: 12px 24px; 
                  text-decoration: none; border-radius: 6px; font-weight: bold;'>🚨 ACCIÓN REQUERIDA</a>
            </div>"
            : string.Empty;

        return $@"
<!DOCTYPE html>
<html>
<head>
   <meta charset='UTF-8'>
   <title>🚨 CRÍTICO: {title} - BuildTruck</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
   <div style='background: linear-gradient(135deg, #dc2626, #b91c1c); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
       <h1 style='color: white; margin: 0; font-size: 28px;'>🚨 BuildTruck</h1>
       <p style='color: white; margin: 10px 0 0 0; font-size: 16px;'>NOTIFICACIÓN CRÍTICA</p>
   </div>
   
   <div style='background: white; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
       <h2 style='color: #dc2626; margin-top: 0;'>🚨 {title}</h2>
       
       <p>Hola <strong>{fullName}</strong>,</p>
       
       <div style='background: #fef2f2; border-left: 4px solid #dc2626; padding: 15px; margin: 20px 0;'>
           <h4 style='margin-top: 0; color: #991b1b;'>ATENCIÓN REQUERIDA INMEDIATAMENTE</h4>
           <p style='margin: 10px 0; font-size: 16px; font-weight: bold;'>{message}</p>
           <p style='margin: 5px 0;'><strong>Proyecto:</strong> {projectName}</p>
           <p style='margin: 5px 0;'><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
       </div>
       
       {actionButton}
       
       <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
       
       <p style='color: #6b7280; font-size: 14px; text-align: center;'>
           BuildTruck - Sistema de Gestión de Construcción<br>
           Este email fue enviado automáticamente, no responder.
       </p>
   </div>
</body>
</html>";
    }
}