using MovieApp.DAL.Entities;
using MovieApp.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.BLL.Services
{
    public class WatchingHistoryService
    {
        private readonly WatchingHistoryRepo _repo = new();

        public void SaveHistory(int userId, int movieId, int duration)
        {
            var history = new WatchingHistory
            {
                UserId = userId,
                MovieId = movieId,
                WatchDuration = duration,
                WatchedAt = DateTime.Now
            };
            _repo.AddOrUpdate(history);
        }

        public List<WatchingHistory> GetHistoryByUserId(int userId)
        {
            return _repo.GetAll()
                        .Where(h => h.UserId == userId)
                        .OrderByDescending(h => h.WatchedAt) 
                        .ToList();
        }
    }
}
