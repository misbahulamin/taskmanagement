using TaskManagement.Models;

namespace TaskManagement.Services
{
    public interface IAuthService
    {
        Task<User> RegisterUserAsync(string username, string email, string password, string? firstName = null, string? lastName = null);
        Task<User?> AuthenticateAsync(string usernameOrEmail, string password);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<bool> IsUsernameUniqueAsync(string username);
    }
} 