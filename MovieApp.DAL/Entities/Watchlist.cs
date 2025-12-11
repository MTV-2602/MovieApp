using System;
using System.Collections.Generic;

namespace MovieApp.DAL.Entities;

public partial class Watchlist
{
    public int WatchlistId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsPublic { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual UserAccount User { get; set; } = null!;

    public virtual ICollection<WatchlistMovie> WatchlistMovies { get; set; } = new List<WatchlistMovie>();
}
