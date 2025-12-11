using System;
using System.Collections.Generic;

namespace MovieApp.DAL.Entities;

public partial class WatchlistMovie
{
    public int WatchlistId { get; set; }

    public int MovieId { get; set; }

    public int? SortOrder { get; set; }

    public DateTime? AddedAt { get; set; }

    public virtual Movie Movie { get; set; } = null!;

    public virtual Watchlist Watchlist { get; set; } = null!;
}
