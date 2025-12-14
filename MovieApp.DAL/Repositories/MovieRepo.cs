using Microsoft.EntityFrameworkCore;
using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.DAL.Repositories
{
    public class MovieRepo
    {
        public Movie? GetLatestMovie()
        {
            using var ctx = new MovieAppContext();

            return ctx.Movies
                      .Include(x => x.Director)
                      .OrderByDescending(x => x.MovieId)
                      .FirstOrDefault();
        }

        public List<Movie> GetAllMovies()
        {
            using var ctx = new MovieAppContext();

            return ctx.Movies
                      .Include(x => x.Director)
                      .AsNoTracking()
                      .ToList();
        }

        public Movie? GetById(int id)
        {
            using var ctx = new MovieAppContext();

            return ctx.Movies
                      .Include(x => x.Director)
                      .FirstOrDefault(x => x.MovieId == id);
        }

        public List<Movie> SearchMovies(string keyword)
        {
            using var ctx = new MovieAppContext();

            return ctx.Movies
                      .Include(x => x.Director)
                      .Where(x => x.Title.Contains(keyword))
                      .ToList();
        }

        public List<Movie> FilterByGenre(string genre)
        {
            using var ctx = new MovieAppContext();

            return ctx.Movies
                      .Include(x => x.Director)
                      .Where(x => x.Genre == genre)
                      .ToList();
        }

        public List<Movie> FilterByCountry(string country)
        {
            using var ctx = new MovieAppContext();

            return ctx.Movies
                      .Include(x => x.Director)
                      .Where(x => x.Director.Country == country)
                      .ToList();
        }

        public List<Movie> GetMoviesByDirector(int directorId)
        {
            using var ctx = new MovieAppContext();

            return ctx.Movies
                      .Include(x => x.Director)
                      .Where(x => x.DirectorId == directorId)
                      .ToList();
        }

        public void AddMovie(Movie movie)
        {
            using var ctx = new MovieAppContext();
            ctx.Movies.Add(movie);
            ctx.SaveChanges();
        }

        public void UpdateMovie(Movie movie)
        {
            using var ctx = new MovieAppContext();
            var existingMovie = ctx.Movies.Find(movie.MovieId);
            if (existingMovie == null)
                throw new ArgumentException("Movie not found");

            existingMovie.Title = movie.Title;
            existingMovie.Description = movie.Description;
            existingMovie.Genre = movie.Genre;
            existingMovie.DirectorId = movie.DirectorId;
            existingMovie.ReleaseDate = movie.ReleaseDate;
            existingMovie.Duration = movie.Duration;
            existingMovie.PosterUrl = movie.PosterUrl;
            existingMovie.TrailerUrl = movie.TrailerUrl;
            existingMovie.Status = movie.Status;

            ctx.SaveChanges();
        }

        public void DeleteMovie(int movieId)
        {
            using var ctx = new MovieAppContext();
            var movie = ctx.Movies.Find(movieId);
            if (movie != null)
            {
                ctx.Movies.Remove(movie);
                ctx.SaveChanges();
            }
        }
    }
}
