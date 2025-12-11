using MovieApp.DAL.Entities;
using MovieApp.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.BLL.Services
{
    public class UserAccountService
    {
        private readonly UserAccountRepo _repo = new();

        // ============================
        //  LOGIN (Authenticate)
        // ============================
        public UserAccount? Authenticate(string username, string password)
        {
            var user = _repo.GetByUsername(username);
            if (user == null) return null;

            bool isMatch = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return isMatch ? user : null;
        }

        // ============================
        //  REGISTER (Create User)
        // ============================
        public bool Register(string displayName, string username, string password)
        {
            if (_repo.GetByUsername(username) != null)
            {
                return false; // username đã tồn tại
            }

            var newUser = new UserAccount
            {
                DisplayName = displayName,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.Now,
                Status = "Active",
                Role = 2 // default User
            };

            _repo.Create(newUser);
            return true;
        }

        // ============================
        //  GET USER BY ID
        // ============================
        public UserAccount? GetUserById(int id)
        {
            return _repo.GetById(id);
        }

        // ============================
        //  UPDATE PROFILE
        // ============================
        public void UpdateProfile(int userId, string newDisplayName, string newUsername, string? newPassword)
        {
            var user = _repo.GetById(userId);
            if (user == null)
                throw new Exception("User not found");

            user.DisplayName = newDisplayName;
            user.Username = newUsername;

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            _repo.Update(user);
        }

        // ============================
        //  CHANGE STATUS (BAN / ACTIVE)
        // ============================
        public void ChangeStatus(int userId, string status)
        {
            var user = _repo.GetById(userId);
            if (user == null)
                throw new Exception("User not found");

            user.Status = status;
            _repo.Update(user);
        }

        // ============================
        //  LIST USERS
        // ============================
        public List<UserAccount> GetAllUsers()
        {
            return _repo.GetAll();
        }
    }
}