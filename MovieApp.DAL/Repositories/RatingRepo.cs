using Microsoft.EntityFrameworkCore;
using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovieApp.DAL.Repositories
{
    public class RatingRepo
    {
        private readonly MovieAppContext _ctx = new();

        public void AddOrUpdateRating(int userId, int movieId, int score)
        {
            var existing = _ctx.Ratings
                .FirstOrDefault(r => r.UserId == userId && r.MovieId == movieId);

            if (existing != null)
            {
                existing.Score = score;
                existing.CreatedAt = DateTime.Now;
                _ctx.Ratings.Update(existing);
            }
            else
            {
                var newRating = new Rating
                {
                    UserId = userId,
                    MovieId = movieId,
                    Score = score,
                    CreatedAt = DateTime.Now
                };
                _ctx.Ratings.Add(newRating);
            }
            _ctx.SaveChanges();
        }

        public Rating? GetUserRatingForMovie(int userId, int movieId)
        {
            return _ctx.Ratings
                .FirstOrDefault(r => r.UserId == userId && r.MovieId == movieId);
        }

        public double GetAverageRating(int movieId)
        {
            var ratings = _ctx.Ratings
                .Where(r => r.MovieId == movieId)
                .ToList();

            if (ratings.Count == 0)
                return 0;

            return ratings.Average(r => r.Score);
        }

        public int GetRatingCount(int movieId)
        {
            return _ctx.Ratings
                .Count(r => r.MovieId == movieId);
        }

        public List<Rating> GetAllRatingsForMovie(int movieId)
        {
            return _ctx.Ratings
                .Include(r => r.User)
                .Where(r => r.MovieId == movieId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }
    }
}
