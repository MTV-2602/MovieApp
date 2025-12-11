using MovieApp.DAL.Entities;
using MovieApp.DAL.Repositories;
using System.Collections.Generic;

namespace MovieApp.BLL.Services
{
    public class RatingService
    {
        private readonly RatingRepo _repo = new();

        public void AddOrUpdateRating(int userId, int movieId, int score)
        {
            if (score < 1 || score > 5)
                throw new System.ArgumentException("Rating score must be between 1 and 5");

            _repo.AddOrUpdateRating(userId, movieId, score);
        }

        public Rating? GetUserRating(int userId, int movieId)
        {
            return _repo.GetUserRatingForMovie(userId, movieId);
        }

        public double GetAverageRating(int movieId)
        {
            return _repo.GetAverageRating(movieId);
        }

        public int GetRatingCount(int movieId)
        {
            return _repo.GetRatingCount(movieId);
        }

        public List<Rating> GetAllRatingsForMovie(int movieId)
        {
            return _repo.GetAllRatingsForMovie(movieId);
        }
    }
}
