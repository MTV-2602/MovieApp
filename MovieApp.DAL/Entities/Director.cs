using System;
using System.Collections.Generic;

namespace MovieApp.DAL.Entities;

public partial class Director
{
    public int DirectorId { get; set; }

    public string DirectorName { get; set; } = null!;

    public string? Bio { get; set; }

    public string? Country { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
