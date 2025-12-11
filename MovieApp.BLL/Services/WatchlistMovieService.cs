using MovieApp.DAL.Entities;
using MovieApp.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.BLL.Services
{
    public class WatchlistMovieService
    {
        private readonly WatchlistMovieRepo _repo = new();

        public void AddToWatchlist(int watchlistId, int movieId)
        {
            _repo.AddToWatchlist(watchlistId, movieId);
        }

        public List<Movie> GetMoviesInWatchlist(int watchlistId)
        {
            return _repo.GetMoviesByWatchlistId(watchlistId);
        }

        public void RemoveFromWatchlist(int watchlistId, int movieId)
        {
            _repo.RemoveMovie(watchlistId, movieId);
        }

        //public bool IsMovieInWatchlist(int userId, int movieId)
        //{
        //    return _repo.GetAll()
        //                .Any(w => w.Watchlist != null &&
        //                          w.Watchlist.UserId == userId &&
        //                          w.MovieId == movieId);
        //}

        public List<WatchlistMovie> GetWatchlistByUserId(int userId)
        {
            // Cần Include Movie để lấy ảnh và tên
            return _repo.GetAll()
                        .Where(w => w.Watchlist.UserId == userId)
                        .OrderByDescending(w => w.AddedAt)
                        .ToList();
        }

        public void RemoveFromWatchlist(int id)
        {
            // Bạn cần implement hàm Delete trong Repo
            // _repo.Delete(id);
            // Code mẫu giả định nếu chưa có hàm Delete:
            var item = _repo.GetAll().FirstOrDefault(x => x.WatchlistId == id);
            if (item != null) _repo.Remove(item); // Hoặc hàm xóa tương ứng của bạn
        }
    }
}
