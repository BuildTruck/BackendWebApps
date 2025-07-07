using BuildTruckBack.Users.Domain.Model.Aggregates;
using BuildTruckBack.Users.Domain.Model.Commands;
// using BuildTruckBack.Users.Domain.Model.ValueObjects;  ← ELIMINAR ESTA LÍNEA
using UsersUserRole = BuildTruckBack.Users.Domain.Model.ValueObjects.UserRole;  // ← AGREGAR
using PersonName = BuildTruckBack.Users.Domain.Model.ValueObjects.PersonName;   // ← AGREGAR
using EmailAddress = BuildTruckBack.Users.Domain.Model.ValueObjects.EmailAddress; // ← AGREGAR
using BuildTruckBack.Users.Domain.Repositories;
using BuildTruckBack.Users.Domain.Services;
using BuildTruckBack.Shared.Domain.Repositories;
using BuildTruckBack.Users.Application.ACL.Services;
using BuildTruckBack.Shared.Infrastructure.ExternalServices.Email.Services;
using BuildTruckBack.Notifications.Interfaces.ACL;
using BuildTruckBack.Notifications.Domain.Model.ValueObjects;
using NotificationUserRole = BuildTruckBack.Notifications.Domain.Model.ValueObjects.UserRole;

namespace BuildTruckBack.Users.Application.Internal.CommandServices;

/**
 * <summary>
 *     The user command service implementation
 * </summary>
 * <remarks>
 *     This service handles user commands for the BuildTruck platform using Value Objects
 *     Uses ACL pattern for email operations to maintain clean architecture
 * </remarks>
 */
public class UserCommandService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IEmailService userEmailService,
    IGenericEmailService genericEmailService,
    IImageService imageService,
    INotificationContextFacade notificationFacade) 
    : IUserCommandService
{
    private readonly INotificationContextFacade _notificationFacade = notificationFacade;
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
        var userRole = new UsersUserRole(command.Role);
        
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
            try
            {
                Console.WriteLine($"🔍 DEBUG - Enviando notificación para rol: '{user.Role.Role}'");
                Console.WriteLine($"🔍 DEBUG - NotificationUserRole.Admin.Value: '{NotificationUserRole.Admin.Value}'");
    
                await _notificationFacade.CreateNotificationForRoleAsync(
                    role: NotificationUserRole.Admin,
                    type: NotificationType.UserRegistered,
                    context: NotificationContext.System,
                    title: "Nuevo Usuario Registrado",
                    message: $"Se registró el usuario {user.FullName} con rol {user.Role.Role}.",
                    priority: NotificationPriority.Normal,
                    actionUrl: $"/users/{user.Id}",
                    relatedEntityId: user.Id,
                    relatedEntityType: "User"
                );
                await _notificationFacade.CreateNotificationForUserAsync(
                    user.Id,
                    NotificationType.SystemNotification,
                    NotificationContext.System,
                    "Bienvenido a BuildTruck",
                    "Tu cuenta ha sido creada exitosamente. ¡Bienvenido!",
                    NotificationPriority.Normal
                );
            }
            catch (Exception notificationEx)
            {
                Console.WriteLine($"🔔 ❌ Error enviando notificación de nuevo usuario: {notificationEx.Message}");
            }
            // ✅ Enviar email de bienvenida usando ACL - más limpio y orientado al dominio
            try 
            {
                await userEmailService.SendUserCredentialsAsync(user, temporalPassword);
                Console.WriteLine($"📧 ✅ Email de credenciales enviado via ACL para usuario {user.FullName}");
            }
            catch (Exception emailEx)
            {
                // Log error pero no fallar la creación del usuario
                Console.WriteLine($"📧 ❌ Error enviando email via ACL: {emailEx.Message}");
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
        // TODO: Refactorizar esto también al ACL en el futuro
        try 
        {
            await genericEmailService.SendPasswordChangedNotificationAsync(
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

    /**
     * <summary>
     *     Handle delete user command (physical deletion)
     * </summary>
     * <param name="command">The delete user command</param>
     * <returns>Task</returns>
     */
    public async Task Handle(DeleteUserCommand command)
    {
        // ✅ Find user
        var user = await userRepository.FindByIdAsync(command.UserId);
        if (user == null)
            throw new ArgumentException($"User with ID {command.UserId} not found");

        try
        {
            // ✅ Remove user from database
            userRepository.Remove(user);
            await unitOfWork.CompleteAsync();

            Console.WriteLine($"🗑️ ✅ User {user.FullName} (ID: {user.Id}) deleted successfully");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error deleting user: {ex.Message}");
        }
    }
    
    /**
     * <summary>
     *     Handle upload profile image command (creates new or replaces existing)
     * </summary>
     * <param name="command">The upload profile image command</param>
     * <returns>Updated user with new profile image URL</returns>
     */
    public async Task<User> Handle(UploadProfileImageCommand command)
    {
        // ✅ Find user
        var user = await userRepository.FindByIdAsync(command.UserId);
        if (user == null)
            throw new ArgumentException($"User with ID {command.UserId} not found");

        try
        {
            // ✅ Validate image through ACL
            var validation = imageService.ValidateUserProfileImage(command.ImageBytes, command.FileName);
            if (!validation.IsValid)
                throw new ArgumentException(validation.ErrorMessage);

            // ✅ Upload image through ACL (handles deletion of previous image)
            var imageUrl = await imageService.UploadUserProfileImageAsync(user, command.ImageBytes, command.FileName);

            // ✅ Update user domain entity
            user.UpdateProfileImage(imageUrl);

            // ✅ Save changes
            userRepository.Update(user);
            await unitOfWork.CompleteAsync();

            Console.WriteLine($"📸 ✅ Profile image uploaded for user {user.FullName} (ID: {user.Id})");
            return user;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error uploading profile image: {ex.Message}");
        }
    }

    /**
     * <summary>
     *     Handle delete profile image command
     * </summary>
     * <param name="command">The delete profile image command</param>
     * <returns>Updated user without profile image</returns>
     */
    public async Task<User> Handle(DeleteProfileImageCommand command)
    {
        // ✅ Find user
        var user = await userRepository.FindByIdAsync(command.UserId);
        if (user == null)
            throw new ArgumentException($"User with ID {command.UserId} not found");

        try
        {
            // ✅ Delete image through ACL
            var deleted = await imageService.DeleteUserProfileImageAsync(user);
            
            if (deleted)
            {
                // ✅ Update user domain entity (remove image URL)
                user.UpdateProfileImage(null);

                // ✅ Save changes
                userRepository.Update(user);
                await unitOfWork.CompleteAsync();

                Console.WriteLine($"🗑️ ✅ Profile image deleted for user {user.FullName} (ID: {user.Id})");
            }
            else
            {
                Console.WriteLine($"⚠️ Could not delete profile image for user {user.FullName} (ID: {user.Id})");
            }

            return user;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error deleting profile image: {ex.Message}");
        }
    }
    
    /**
     * <summary>
     *     Handle update user command
     * </summary>
     * <param name="command">The update user command</param>
     * <returns>Updated user</returns>
     */
    public async Task<User> Handle(UpdateUserCommand command)
    {
        // ✅ Find user
        var user = await userRepository.FindByIdAsync(command.UserId);
        if (user == null)
            throw new ArgumentException($"User with ID {command.UserId} not found");

        // ✅ Validate that there are changes to make
        if (!command.HasChanges)
            throw new ArgumentException("No changes provided");

        try
        {
            // ✅ Check if new corporate email would conflict (if name changes)
            if (command.WillUpdateName)
            {
                var tempName = new PersonName(
                    command.Name ?? user.Name.FirstName,
                    command.LastName ?? user.Name.LastName
                );
                var newCorporateEmail = EmailAddress.GenerateCorporateEmail(tempName);
                
                // ✅ Only check conflict if email actually changes
                if (newCorporateEmail.Address != user.Email)
                {
                    if (await userRepository.ExistsByEmailAsync(newCorporateEmail))
                    {
                        throw new InvalidOperationException($"Email {newCorporateEmail.Address} already exists");
                    }
                }
            }

            // ✅ Update user using Value Objects
            user.UpdateInfo(
                firstName: command.Name,
                lastName: command.LastName,
                personalEmail: command.PersonalEmail,
                phone: null, // No incluido en command, mantener actual
                role: command.Role
            );

            // ✅ Save changes
            userRepository.Update(user);
            await unitOfWork.CompleteAsync();
            if (command.Role != null && command.Role != user.Role.Role)
            {
                try
                {
                    await _notificationFacade.CreateNotificationForRoleAsync(
                        role: NotificationUserRole.Admin,  // ← CAMBIAR AQUÍ
                        type: NotificationType.UserRegistered,
                        context: NotificationContext.System,
                        title: "🔄 Cambio de Rol de Usuario",
                        message: $"El usuario {user.FullName} cambió de rol a {user.Role.Role}.",
                        priority: NotificationPriority.Normal,
                        actionUrl: $"/users/{user.Id}",
                        relatedEntityId: user.Id,
                        relatedEntityType: "User"
                    );
                }
                catch (Exception notificationEx)
                {
                    Console.WriteLine($"🔔 ❌ Error enviando notificación de cambio de rol: {notificationEx.Message}");
                }
            }
            Console.WriteLine($"✏️ ✅ User {user.FullName} (ID: {user.Id}) updated successfully");
            return user;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error updating user: {ex.Message}");
        }
    }
}