using System;
using System.Collections.Generic;

namespace MovieApp.DAL.Entities;

public partial class Comment
{
    public int CommentId { get; set; }

    public int MovieId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public int? ParentCommentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Comment> InverseParentComment { get; set; } = new List<Comment>();

    public virtual Movie Movie { get; set; } = null!;

    public virtual Comment? ParentComment { get; set; }

    public virtual UserAccount User { get; set; } = null!;
}
