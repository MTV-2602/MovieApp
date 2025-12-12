using MovieApp.DAL.Entities;
using MovieApp.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.BLL.Services
{
    public class WatchlistService
    {
        private readonly WatchlistRepo _repo = new();

        public List<Watchlist> GetUserWatchlists(int userId)
        {
            return _repo.GetByUserId(userId);
        }

        public Watchlist CreateWatchlist(int userId, string title)
        {
            var newWatchlist = new Watchlist
            {
                UserId = userId,
                Title = title,
                Description = "Created from Movie Player",
                IsPublic = false,
                CreatedAt = DateTime.Now
            };
            return _repo.Create(newWatchlist);
        }

        public void DeleteWatchlist(int watchlistId)
        {
            _repo.Delete(watchlistId);
        }
    }
}
