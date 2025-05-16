using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;
using TaskManagement.Utils;

namespace TaskManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> RegisterUserAsync(string username, string email, string password, string? firstName = null, string? lastName = null)
        {
            // Check if username is already taken
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                throw new InvalidOperationException("Username is already taken");
            }

            // Check if email is already registered
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                throw new InvalidOperationException("Email is already registered");
            }

            // Generate salt and hash password
            string salt = PasswordHasher.GenerateSalt();
            string hashedPassword = PasswordHasher.HashPassword(password, salt);

            // Create new user
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            // Add user to database
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> AuthenticateAsync(string usernameOrEmail, string password)
        {
            // Find user by username or email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => 
                    u.Username == usernameOrEmail || 
                    u.Email == usernameOrEmail);

            // If user not found or inactive
            if (user == null || !user.IsActive)
            {
                return null;
            }

            // Verify password
            if (PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                return user;
            }

            return null;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            return !await _context.Users.AnyAsync(u => u.Username == username);
        }
    }
} 