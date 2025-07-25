using EnergyLegacyApp.Data;

using EnergyLegacyApp.Data.Models;

using System;

using System.Security.Cryptography;

using System.Text;

namespace EnergyLegacyApp.Business

{

    public class AuthService

    {

        private readonly UserRepository _userRepository;

        public AuthService(UserRepository userRepository)

        {

            _userRepository = userRepository;

            _userRepository.EnsureUsersTableExists();

        }

        public User? ValidateUser(string username, string password)

        {

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))

                return null;

            var user = _userRepository.GetUserByUsername(username);

            if (user != null && user.IsActive)

            {

                string hashedInput = HashPassword(password);

                if (user.Password == hashedInput)

                {

                    _userRepository.UpdateLastLogin(username);

                    return user;

                }

            }

            return null;

        }

        public bool RegisterUser(User user)

        {

            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))

                return false;

            user.Password = HashPassword(user.Password);

            user.LastLogin = DateTime.Now;

            user.Role ??= "User";

            user.IsActive = true;

            return _userRepository.InsertUser(user);

        }

        private string HashPassword(string password)

        {

            using (var sha = SHA256.Create())

            {

                var bytes = Encoding.UTF8.GetBytes(password);

                var hashBytes = sha.ComputeHash(bytes);

                return Convert.ToBase64String(hashBytes);

            }

        }

    }

}

