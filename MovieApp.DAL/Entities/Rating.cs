using System;
using System.Collections.Generic;

namespace MovieApp.DAL.Entities;

public partial class Rating
{
    public int RatingId { get; set; }

    public int MovieId { get; set; }

    public int UserId { get; set; }

    public int Score { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Movie Movie { get; set; } = null!;

    public virtual UserAccount User { get; set; } = null!;
}
