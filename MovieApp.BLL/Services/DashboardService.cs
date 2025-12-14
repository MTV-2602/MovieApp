using MovieApp.DAL.Entities;
using MovieApp.DAL.Repositories;

namespace MovieApp.BLL.Services
{
    public class DashboardService
    {
        private readonly MovieRepo _movieRepo = new();
        private readonly UserAccountRepo _userRepo = new();
        private readonly WatchingHistoryRepo _historyRepo = new();
        private readonly DirectorRepo _directorRepo = new();

        public int GetTotalMovies()
        {
            return _movieRepo.GetAllMovies().Count;
        }

        public int GetTotalUsers()
        {
            return _userRepo.GetAll().Count;
        }

        public int GetTotalWatches()
        {
            return _historyRepo.GetAll().Count;
        }

        public int GetAvailableMovies()
        {
            return _movieRepo.GetAllMovies().Count(m => m.Status == "Available");
        }

        public int GetUnavailableMovies()
        {
            return _movieRepo.GetAllMovies().Count(m => m.Status == "Unavailable");
        }

        public int GetAdminCount()
        {
            return _userRepo.GetAll().Count(u => u.Role == 1);
        }

        public int GetRegularUserCount()
        {
            return _userRepo.GetAll().Count(u => u.Role == 2);
        }

        public List<Movie> GetLatestMovies(int count = 5)
        {
            return _movieRepo.GetAllMovies()
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .ToList();
        }

        public List<Movie> GetMostWatchedMovies(int count = 5)
        {
            using var ctx = new MovieAppContext();
            var movieWatchCounts = ctx.WatchingHistories
                .GroupBy(h => h.MovieId)
                .Select(g => new { MovieId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(count)
                .ToList();

            var movieIds = movieWatchCounts.Select(x => x.MovieId).ToList();
            return _movieRepo.GetAllMovies()
                .Where(m => movieIds.Contains(m.MovieId))
                .OrderBy(m => movieIds.IndexOf(m.MovieId))
                .ToList();
        }

        public int GetTotalDirectors()
        {
            return _directorRepo.GetAllDirectors().Count;
        }
    }
}

