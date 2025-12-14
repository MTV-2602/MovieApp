using MovieApp.DAL.Entities;
using MovieApp.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.BLL.Services
{
    public class DirectorService
    {
        private readonly DirectorRepo _directorRepo = new();

        public List<Director> GetAllDirectors()
        {
            return _directorRepo.GetAllDirectors();
        }

        public Director? GetDirectorById(int id)
        {
            return _directorRepo.GetById(id);
        }

        public List<Director> SearchDirectors(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _directorRepo.GetAllDirectors();

            return _directorRepo.SearchDirectors(keyword);
        }

        public List<Director> FilterByCountry(string country)
        {
            if (string.IsNullOrWhiteSpace(country) || country == "Country")
                return _directorRepo.GetAllDirectors();

            return _directorRepo.FilterByCountry(country);
        }

        public void AddDirector(Director director)
        {
            ValidateDirector(director);
            director.Status = string.IsNullOrWhiteSpace(director.Status) ? "Active" : director.Status;
            _directorRepo.Add(director);
        }

        public void UpdateDirector(Director director)
        {
            ValidateDirector(director);
            _directorRepo.Update(director);
        }

        public void DeleteDirector(int directorId)
        {
            var d = _directorRepo.GetById(directorId);
            if (d == null) throw new ArgumentException("Director not found");
            _directorRepo.Delete(directorId);
        }

        private void ValidateDirector(Director director)
        {
            if (director == null) throw new ArgumentNullException(nameof(director));
            if (string.IsNullOrWhiteSpace(director.DirectorName)) throw new ArgumentException("Director name is required");
        }
    }
}
