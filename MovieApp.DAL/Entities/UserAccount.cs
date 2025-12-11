using System;
using System.Collections.Generic;

namespace MovieApp.DAL.Entities;

public partial class UserAccount
{
    public int UserId { get; set; }

    public string DisplayName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<WatchingHistory> WatchingHistories { get; set; } = new List<WatchingHistory>();

    public virtual ICollection<Watchlist> Watchlists { get; set; } = new List<Watchlist>();
}
