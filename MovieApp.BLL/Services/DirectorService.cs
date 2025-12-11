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
    }
}
