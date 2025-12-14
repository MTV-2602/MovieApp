using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.DAL.Repositories
{
    public class UserAccountRepo
    {
        private readonly MovieAppContext _ctx = new();

        // Lấy user theo username
        public UserAccount? GetByUsername(string username)
        {
            return _ctx.UserAccounts.FirstOrDefault(u => u.Username == username);
        }

        // Lấy user theo username + passwordHash
        public UserAccount? GetByUsernameAndPassword(string username, string passwordHash)
        {
            return _ctx.UserAccounts
                       .FirstOrDefault(u => u.Username == username && u.PasswordHash == passwordHash);
        }

        // Tạo user mới
        public void Create(UserAccount user)
        {
            using var ctx = new MovieAppContext();
            try
            {
                ctx.UserAccounts.Add(user);
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ SaveChanges failed:");
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                throw;
            }
        }

        // Lấy user theo ID
        public UserAccount? GetById(int id)
        {
            return _ctx.UserAccounts.FirstOrDefault(u => u.UserId == id);
        }

        // Update user
        public void Update(UserAccount user)
        {
            _ctx.UserAccounts.Update(user);
            _ctx.SaveChanges();
        }

        // Lấy tất cả user
        public List<UserAccount> GetAll()
        {
            return _ctx.UserAccounts.ToList();
        }

        public void Delete(int userId)
        {
            var user = _ctx.UserAccounts.Find(userId);
            if (user != null)
            {
                _ctx.UserAccounts.Remove(user);
                _ctx.SaveChanges();
            }
        }
    }
}
