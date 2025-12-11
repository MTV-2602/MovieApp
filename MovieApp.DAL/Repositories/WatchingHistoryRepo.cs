using Microsoft.EntityFrameworkCore;
using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.DAL.Repositories
{
    public class WatchingHistoryRepo
    {
        private readonly MovieAppContext _ctx = new();

        public void UpsertHistory(int userId, int movieId, int durationWatched)
        {
            var history = _ctx.WatchingHistories
                              .FirstOrDefault(h => h.UserId == userId && h.MovieId == movieId);

            if (history != null)
            {
                history.WatchedAt = DateTime.Now;
                history.WatchDuration = durationWatched; 
                _ctx.WatchingHistories.Update(history);
            }
            else
            {
                var newHistory = new WatchingHistory
                {
                    UserId = userId,
                    MovieId = movieId,
                    WatchedAt = DateTime.Now,
                    WatchDuration = durationWatched
                };
                _ctx.WatchingHistories.Add(newHistory);
            }
            _ctx.SaveChanges();
        }

        public List<WatchingHistory> GetAll()
        {
            // Cần Include Movie để lấy Tên phim, Poster...
            return _ctx.WatchingHistories
                           .Include(x => x.Movie)
                           .ToList();
        }

        // Hàm thêm/sửa lịch sử (Dùng cho Player)
        public void AddOrUpdate(WatchingHistory history)
        {
            var existing = _ctx.WatchingHistories
                .FirstOrDefault(x => x.UserId == history.UserId && x.MovieId == history.MovieId);

            if (existing != null)
            {
                existing.WatchedAt = history.WatchedAt;
                existing.WatchDuration = history.WatchDuration;
                _ctx.WatchingHistories.Update(existing);
            }
            else
            {
                _ctx.WatchingHistories.Add(history);
            }
            _ctx.SaveChanges();
        }
    }
}
