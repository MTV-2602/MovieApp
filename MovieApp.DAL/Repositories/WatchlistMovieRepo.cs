using Microsoft.EntityFrameworkCore;
using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.DAL.Repositories
{
    public class WatchlistMovieRepo
    {
        private readonly MovieAppContext _ctx = new();

        public void AddToWatchlist(int watchlistId, int movieId)
        {
            try
            {
                var existing = _ctx.WatchlistMovies.Find(watchlistId, movieId);
                if (existing == null)
                {
                    var item = new WatchlistMovie
                    {
                        WatchlistId = watchlistId,
                        MovieId = movieId,
                        AddedAt = DateTime.Now
                    };
                    _ctx.WatchlistMovies.Add(item);
                    _ctx.SaveChanges();
                }
            }
            catch { /* Handle duplicate key or error silently */ }
        }

        public List<Movie> GetMoviesByWatchlistId(int watchlistId)
        {
            return _ctx.WatchlistMovies
                       .Where(wm => wm.WatchlistId == watchlistId)
                       .Include(wm => wm.Movie)             
                       .ThenInclude(m => m.Director)        
                       .Select(wm => wm.Movie)          
                       .ToList();
        }

        public void RemoveMovie(int watchlistId, int movieId)
        {
            var item = _ctx.WatchlistMovies.FirstOrDefault(wm => wm.WatchlistId == watchlistId && wm.MovieId == movieId);
            if (item != null)
            {
                _ctx.WatchlistMovies.Remove(item);
                _ctx.SaveChanges();
            }
        }

        public Watchlist? GetFirstWatchlistByUser(int userId)
        {
            return _ctx.Watchlists.FirstOrDefault(w => w.UserId == userId);
        }

        public List<WatchlistMovie> GetAll()
        {
            return _ctx.WatchlistMovies
                           .Include(wm => wm.Movie)
                           .Include(wm => wm.Watchlist)
                           .ToList();
        }

        public void Remove(WatchlistMovie item)
        {
            _ctx.WatchlistMovies.Remove(item);
            _ctx.SaveChanges();
        }

        public void Add(WatchlistMovie item)
        {
            _ctx.WatchlistMovies.Add(item);
            _ctx.SaveChanges();
        }
    }
}
