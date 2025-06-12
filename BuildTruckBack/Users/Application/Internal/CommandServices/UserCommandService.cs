using BuildTruckBack.Users.Domain.Model.Aggregates;
using BuildTruckBack.Users.Domain.Model.Commands;
using BuildTruckBack.Users.Domain.Model.ValueObjects;
using BuildTruckBack.Users.Domain.Repositories;
using BuildTruckBack.Users.Domain.Services;
using BuildTruckBack.Shared.Domain.Repositories;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Email.Services;

namespace BuildTruckBack.Users.Application.Internal.CommandServices;

/**
 * <summary>
 *     The user command service implementation
 * </summary>
 * <remarks>
 *     This service handles user commands for the BuildTruck platform using Value Objects
 * </remarks>
 */
public class UserCommandService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IEmailService emailService)
    : IUserCommandService
{
    /**
     * <summary>
     *     Handle create user command
     * </summary>
     * <param name="command">The create user command</param>
     * <returns>The created user</returns>
     */
    public async Task<User> Handle(CreateUserCommand command)
    {
        // ✅ Crear Value Objects para validación
        var personName = new PersonName(command.Name, command.LastName);
        var userRole = new UserRole(command.Role);
        
        // ✅ Generar email corporativo usando Value Object
        var corporateEmail = EmailAddress.GenerateCorporateEmail(personName);
        
        // ✅ Verificar que el email no exista
        if (await userRepository.ExistsByEmailAsync(corporateEmail))
        {
            throw new InvalidOperationException($"Email {corporateEmail.Address} already exists");
        }

        // ✅ Generar password temporal
        var temporalPassword = GenerateTemporalPassword();
        Console.WriteLine($"🔑🔑🔑 PASSWORD TEMPORAL PARA {personName.FullName}: {temporalPassword} 🔑🔑🔑");
        var passwordHash = HashPassword(temporalPassword);
        
        // ✅ Crear usuario usando constructor con Value Objects
        var user = new User(command, passwordHash);

        try
        {
            await userRepository.AddAsync(user);
            await unitOfWork.CompleteAsync();
            
            // ✅ Enviar email de bienvenida al email PERSONAL
            try 
            {
                var emailDestination = user.PersonalEmail ?? user.Email;
                await emailService.SendWelcomeEmailAsync(
                    emailDestination,
                    user.FullName,
                    temporalPassword
                );
                Console.WriteLine($"📧 ✅ Email de bienvenida enviado a {emailDestination} (email personal)");
            }
            catch (Exception emailEx)
            {
                // Log error pero no fallar la creación del usuario
                Console.WriteLine($"📧 ❌ Error enviando email de bienvenida: {emailEx.Message}");
                // El usuario se crea exitosamente aunque falle el email
            }
            
            return user;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating user: {ex.Message}");
        }
    }

    /**
     * <summary>
     *     Generate temporal password
     * </summary>
     */
    private string GenerateTemporalPassword()
    {
        return $"Temp{Random.Shared.Next(1000, 9999)}!";
    }

    /**
     * <summary>
     *     Hash password using BCrypt
     * </summary>
     */
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    
     /**
     * <summary>
     *     Handle change password command
     * </summary>
     * <param name="command">The change password command</param>
     * <returns>The updated user</returns>
     */
    public async Task<User> Handle(ChangePasswordCommand command)
    {
        // ✅ Find user
        var user = await userRepository.FindByIdAsync(command.UserId);
        if (user == null)
            throw new ArgumentException($"User with ID {command.UserId} not found");

        // ✅ Verify current password
        if (!VerifyPassword(command.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect");

        // ✅ Validate new password
        ValidatePassword(command.NewPassword);

        // ✅ Hash new password and update
        var newPasswordHash = HashPassword(command.NewPassword);
        user.UpdatePasswordHash(newPasswordHash);

        // ✅ Save changes
        userRepository.Update(user);
        await unitOfWork.CompleteAsync();

        // ✅ Enviar notificación de cambio de contraseña al email CORPORATIVO
        try 
        {
            await emailService.SendPasswordChangedNotificationAsync(
                user.Email, 
                user.FullName
            );
            Console.WriteLine($"📧 ✅ Notificación de cambio de contraseña enviada a {user.Email} (email corporativo)");
        }
        catch (Exception emailEx)
        {
            // Log error pero no fallar el cambio de contraseña
            Console.WriteLine($"📧 ❌ Error enviando notificación de cambio de contraseña: {emailEx.Message}");
        }

        return user;
    }

    /**
     * <summary>
     *     Verify password against hash using BCrypt
     * </summary>
     */
    private static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    /**
     * <summary>
     *     Validate password requirements
     * </summary>
     */
    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty");

        if (password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long");

        if (!password.Any(char.IsUpper))
            throw new ArgumentException("Password must contain at least one uppercase letter");

        if (!password.Any(char.IsLower))
            throw new ArgumentException("Password must contain at least one lowercase letter");

        if (!password.Any(char.IsDigit))
            throw new ArgumentException("Password must contain at least one number");

        if (!password.Any(ch => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(ch)))
            throw new ArgumentException("Password must contain at least one special character");
    }
}