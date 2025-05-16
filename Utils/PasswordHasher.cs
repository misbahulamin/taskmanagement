using System.Security.Cryptography;
using System.Text;

namespace TaskManagement.Utils
{
    public static class PasswordHasher
    {
        // Generate a salt for password
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }
        
        // Hash password with salt
        public static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = string.Concat(password, salt);
                var saltedPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);
                var hashedBytes = sha256.ComputeHash(saltedPasswordBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }
        
        // Verify password
        public static bool VerifyPassword(string enteredPassword, string storedHash, string salt)
        {
            var hashOfEnteredPassword = HashPassword(enteredPassword, salt);
            return storedHash.Equals(hashOfEnteredPassword);
        }
    }
} 