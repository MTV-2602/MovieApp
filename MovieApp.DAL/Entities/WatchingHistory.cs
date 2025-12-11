using System;
using System.Collections.Generic;

namespace MovieApp.DAL.Entities;

public partial class WatchingHistory
{
    public int HistoryId { get; set; }

    public int UserId { get; set; }

    public int MovieId { get; set; }

    public DateTime? WatchedAt { get; set; }

    public int? WatchDuration { get; set; }

    public virtual Movie Movie { get; set; } = null!;

    public virtual UserAccount User { get; set; } = null!;
}
