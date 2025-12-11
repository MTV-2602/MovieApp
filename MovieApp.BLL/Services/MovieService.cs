using MovieApp.DAL.Entities;
using MovieApp.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.BLL.Services
{
    public class MovieService
    {
        private readonly MovieRepo _movieRepo = new();
        private readonly DirectorRepo _directorRepo = new();

        public Movie? GetMovieDetail(int id)
        {
            return _movieRepo.GetById(id);
        }

        public List<Movie> GetAllMovies() => _movieRepo.GetAllMovies();

        public Movie? GetLatestMovie() => _movieRepo.GetLatestMovie();

        public List<Director> GetAllDirectors() => _directorRepo.GetAllDirectors();

        public List<Movie> SearchMovies(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _movieRepo.GetAllMovies();

            return _movieRepo.SearchMovies(keyword);
        }

        public List<Movie> FilterMovies(string genre, string country)
        {
            var movies = _movieRepo.GetAllMovies();

            if (!string.IsNullOrWhiteSpace(genre) && genre != "Genre")
                movies = movies.Where(m => m.Genre == genre).ToList();

            if (!string.IsNullOrWhiteSpace(country) && country != "Country")
                movies = movies.Where(m => m.Director?.Country == country).ToList();

            return movies;
        }
    }
}
