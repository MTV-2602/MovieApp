using Microsoft.EntityFrameworkCore;
using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.DAL.Repositories
{
    public class DirectorRepo
    {
        // Lấy tất cả đạo diễn
        public List<Director> GetAllDirectors()
        {
            using var ctx = new MovieAppContext();

            return ctx.Directors
              .Include(d => d.Movies)
              .AsNoTracking()
              .OrderBy(d => d.DirectorName)
              .ToList();

        }

        // Lấy đạo diễn theo Id kèm phim
        public Director? GetById(int id)
        {
            using var ctx = new MovieAppContext();

            return ctx.Directors
                      .Include(d => d.Movies)
                      .FirstOrDefault(d => d.DirectorId == id);
        }

        // Search đạo diễn theo tên
        public List<Director> SearchDirectors(string keyword)
        {
            using var ctx = new MovieAppContext();

            return ctx.Directors
                      .Where(d => d.DirectorName.Contains(keyword))
                      .Include(d => d.Movies)
                      .ToList();
        }

        // Lọc theo quốc gia
        public List<Director> FilterByCountry(string country)
        {
            using var ctx = new MovieAppContext();

            return ctx.Directors
                      .Where(d => d.Country == country)
                      .Include(d => d.Movies)
                      .ToList();
        }

        public void Add(Director director)
        {
            using var ctx = new MovieAppContext();
            ctx.Directors.Add(director);
            ctx.SaveChanges();
        }

        public void Update(Director director)
        {
            using var ctx = new MovieAppContext();
            var existing = ctx.Directors.Find(director.DirectorId);
            if (existing == null) throw new ArgumentException("Director not found");

            existing.DirectorName = director.DirectorName;
            existing.Bio = director.Bio;
            existing.Country = director.Country;
            existing.Status = director.Status;

            ctx.SaveChanges();
        }

        public void Delete(int directorId)
        {
            using var ctx = new MovieAppContext();
            var dir = ctx.Directors.Find(directorId);
            if (dir != null)
            {
                ctx.Directors.Remove(dir);
                ctx.SaveChanges();
            }
        }
    }
}

