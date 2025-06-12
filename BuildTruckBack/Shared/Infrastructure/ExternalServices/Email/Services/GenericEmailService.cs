using System.Net;
using System.Net.Mail;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Email.Configuration;
using Microsoft.Extensions.Options;

namespace BuildTruckBack.Shared.Infrastructure.ExternalServices.Email.Services;

/// <summary>
/// Generic email service implementation using System.Net.Mail
/// Serves all bounded contexts with generic email operations
/// </summary>
public class GenericEmailService : IGenericEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<GenericEmailService> _logger;

    public GenericEmailService(IOptions<EmailSettings> emailSettings, ILogger<GenericEmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Send welcome email with temporal credentials
    /// </summary>
    public async Task SendWelcomeEmailAsync(string email, string fullName, string temporalPassword)
    {
        try
        {
            var subject = "🚛 Bienvenido a BuildTruck - Credenciales de acceso";
            var htmlBody = CreateWelcomeEmailTemplate(fullName, email, temporalPassword);

            await SendEmailInternalAsync(email, subject, htmlBody);
            
            _logger.LogInformation("✅ Welcome email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send welcome email to {Email}", email);
            // No lanzar excepción para no afectar el registro del usuario
        }
    }

    /// <summary>
    /// Send email with custom subject and body (for ACL usage)
    /// </summary>
    public async Task SendEmailAsync(string email, string subject, string htmlBody)
    {
        try
        {
            await SendEmailInternalAsync(email, subject, htmlBody);
            _logger.LogInformation("✅ Custom email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send custom email to {Email}", email);
            throw; // Re-throw for ACL error handling
        }
    }

    /// <summary>
    /// Send password changed notification
    /// </summary>
    public async Task SendPasswordChangedNotificationAsync(string email, string fullName)
    {
        try
        {
            var subject = "🔐 BuildTruck - Contraseña cambiada exitosamente";
            var htmlBody = CreatePasswordChangedTemplate(fullName);

            await SendEmailAsync(email, subject, htmlBody);
            
            _logger.LogInformation("✅ Password changed notification sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send password changed notification to {Email}", email);
        }
    }

    /// <summary>
    /// Core method to send emails using SMTP (internal)
    /// </summary>
    private async Task SendEmailInternalAsync(string toEmail, string subject, string htmlBody)
    {
        using var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
        mailMessage.To.Add(toEmail);
        mailMessage.Subject = subject;
        mailMessage.Body = htmlBody;
        mailMessage.IsBodyHtml = true;

        using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
        smtpClient.Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);
        smtpClient.EnableSsl = _emailSettings.EnableSsl;

        await smtpClient.SendMailAsync(mailMessage);
    }

    /// <summary>
    /// Create welcome email HTML template
    /// </summary>
    private static string CreateWelcomeEmailTemplate(string fullName, string email, string temporalPassword)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <title>Bienvenido a BuildTruck</title>
        </head>
        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #f97316, #ea580c); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                <h1 style='color: white; margin: 0; font-size: 28px;'>🚛 BuildTruck</h1>
                <p style='color: white; margin: 10px 0 0 0; font-size: 16px;'>Plataforma de Gestión de Construcción</p>
            </div>
            
            <div style='background: white; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
                <h2 style='color: #f97316; margin-top: 0;'>¡Bienvenido a BuildTruck, {fullName}! 👋</h2>
                
                <p>Tu cuenta ha sido creada exitosamente. A continuación encontrarás tus credenciales de acceso:</p>
                
                <div style='background: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #374151;'>📧 Credenciales de Acceso:</h3>
                    <p style='margin: 5px 0;'><strong>Email:</strong> {email}</p>
                    <p style='margin: 5px 0;'><strong>Contraseña temporal:</strong> <code style='background: #e5e7eb; padding: 4px 8px; border-radius: 4px; font-family: monospace;'>{temporalPassword}</code></p>
                </div>
                
                <div style='background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0;'>
                    <h4 style='margin-top: 0; color: #92400e;'>⚠️ Importante:</h4>
                    <ul style='margin: 10px 0; padding-left: 20px;'>
                        <li>Esta es una contraseña temporal generada automáticamente</li>
                        <li>Por seguridad, <strong>debes cambiarla</strong> en tu primer inicio de sesión</li>
                        <li>Usa una contraseña segura con al menos 8 caracteres, mayúsculas, minúsculas, números y símbolos</li>
                    </ul>
                </div>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='http://localhost:3000/login' style='background: #f97316; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold;'>🚀 Acceder a BuildTruck</a>
                </div>
                
                <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
                
                <p style='color: #6b7280; font-size: 14px; text-align: center;'>
                    Si tienes alguna pregunta, contacta al administrador del sistema.<br>
                    Este email fue enviado automáticamente, no responder.
                </p>
            </div>
        </body>
        </html>";
    }

    /// <summary>
    /// Create password changed notification template
    /// </summary>
    private static string CreatePasswordChangedTemplate(string fullName)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <title>Contraseña Cambiada - BuildTruck</title>
        </head>
        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
            <div style='background: linear-gradient(135deg, #10b981, #059669); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                <h1 style='color: white; margin: 0; font-size: 28px;'>🔐 BuildTruck</h1>
                <p style='color: white; margin: 10px 0 0 0; font-size: 16px;'>Notificación de Seguridad</p>
            </div>
            
            <div style='background: white; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 10px 10px;'>
                <h2 style='color: #10b981; margin-top: 0;'>¡Contraseña cambiada exitosamente! ✅</h2>
                
                <p>Hola <strong>{fullName}</strong>,</p>
                
                <p>Te confirmamos que tu contraseña en BuildTruck ha sido cambiada exitosamente.</p>
                
                <div style='background: #d1fae5; border-left: 4px solid #10b981; padding: 15px; margin: 20px 0;'>
                    <h4 style='margin-top: 0; color: #065f46;'>📅 Detalles del cambio:</h4>
                    <p style='margin: 5px 0;'><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                    <p style='margin: 5px 0;'><strong>Estado:</strong> Cambio exitoso</p>
                </div>
                
                <div style='background: #fef2f2; border-left: 4px solid #ef4444; padding: 15px; margin: 20px 0;'>
                    <h4 style='margin-top: 0; color: #991b1b;'>🚨 ¿No fuiste tú?</h4>
                    <p>Si no realizaste este cambio, contacta inmediatamente al administrador del sistema para proteger tu cuenta.</p>
                </div>
                
                <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
                
                <p style='color: #6b7280; font-size: 14px; text-align: center;'>
                    Equipo de Seguridad BuildTruck<br>
                    Este email fue enviado automáticamente, no responder.
                </p>
            </div>
        </body>
        </html>";
    }
}