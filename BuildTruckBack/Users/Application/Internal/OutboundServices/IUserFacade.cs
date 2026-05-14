namespace BuildTruckBack.Users.Application.Internal.OutboundServices;

public interface IUserFacade
{
    Task<UserDto?> VerifyCredentialsAsync(string email, string password);
    Task<UserDto?> FindByEmailAsync(string email);
    Task<UserDto?> FindByIdAsync(int userId);
    Task<bool> UpdateLastLoginAsync(int userId);
    Task<bool> IsActiveUserAsync(string email);
    Task SendPasswordResetEmailAsync(int userId, string email, string fullName, string resetToken);
    Task<string> GetUserProfileImageUrlAsync(int userId, int size = 200);
    Task<bool> ResetUserPasswordAsync(int userId, string newPassword);
}
