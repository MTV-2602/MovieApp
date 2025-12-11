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

        // 🔍 Search by title
        public List<Movie> SearchMovies(string keyword)
        {
            using var ctx = new MovieAppContext();

            return ctx.Movies
                      .Include(x => x.Director)
                      .Where(x => x.Title.Contains(keyword))
                      .ToList();
        }

        // 🔽 Filter by Genre
        public List<Movie> FilterByGenre(string genre)
        {
            using var ctx = new MovieAppContext();

            return ctx.Movies
                      .Include(x => x.Director)
                      .Where(x => x.Genre == genre)
                      .ToList();
        }

        // 🌍 Filter by Country (lấy từ Director)
        public List<Movie> FilterByCountry(string country)
        {
            using var ctx = new MovieAppContext();

            return ctx.Movies
                      .Include(x => x.Director)
                      .Where(x => x.Director.Country == country)
                      .ToList();
        }

        // 🎬 Filter by Director
        public List<Movie> GetMoviesByDirector(int directorId)
        {
            using var ctx = new MovieAppContext();

            return ctx.Movies
                      .Include(x => x.Director)
                      .Where(x => x.DirectorId == directorId)
                      .ToList();
        }
    }
}
