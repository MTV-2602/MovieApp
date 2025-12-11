using System;
using System.Collections.Generic;

namespace MovieApp.DAL.Entities;

public partial class Movie
{
    public int MovieId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Genre { get; set; } = null!;

    public DateOnly? ReleaseDate { get; set; }

    public int? Duration { get; set; }

    public string PosterUrl { get; set; } = null!;

    public string TrailerUrl { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? DirectorId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual UserAccount CreatedByNavigation { get; set; } = null!;

    public virtual Director? Director { get; set; }

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<WatchingHistory> WatchingHistories { get; set; } = new List<WatchingHistory>();

    public virtual ICollection<WatchlistMovie> WatchlistMovies { get; set; } = new List<WatchlistMovie>();
}
