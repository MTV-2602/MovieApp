using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.DAL.Repositories
{
    public class WatchlistRepo
    {
        private readonly MovieAppContext _ctx = new();

        public List<Watchlist> GetByUserId(int userId)
        {
            return _ctx.Watchlists.Where(w => w.UserId == userId).ToList();
        }


        public Watchlist Create(Watchlist watchlist)
        {
            _ctx.Watchlists.Add(watchlist);
            _ctx.SaveChanges();
            return watchlist;
        }

        public void Delete(int watchlistId)
        {
            var item = _ctx.Watchlists.Find(watchlistId);
            if (item != null)
            {
                _ctx.Watchlists.Remove(item);
                _ctx.SaveChanges();
            }
        }
    }
}
