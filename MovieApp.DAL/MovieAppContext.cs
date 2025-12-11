using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MovieApp.DAL.Entities;

public partial class MovieAppContext : DbContext
{
    public MovieAppContext()
    {
    }

    public MovieAppContext(DbContextOptions<MovieAppContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Director> Directors { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    public virtual DbSet<WatchingHistory> WatchingHistories { get; set; }

    public virtual DbSet<Watchlist> Watchlists { get; set; }

    public virtual DbSet<WatchlistMovie> WatchlistMovies { get; set; }

    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
        var strConn = config["ConnectionStrings:DefaultConnectionStringDB"];

        return strConn;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString());
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__comment__E7957687ED57A600");

            entity.ToTable("comment");

            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Movie).WithMany(p => p.Comments)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("FK__comment__movie_i__46E78A0C");

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment)
                .HasForeignKey(d => d.ParentCommentId)
                .HasConstraintName("FK__comment__parent___48CFD27E");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__comment__user_id__47DBAE45");
        });

        modelBuilder.Entity<Director>(entity =>
        {
            entity.HasKey(e => e.DirectorId).HasName("PK__director__F5205E497E7BF231");

            entity.ToTable("director");

            entity.Property(e => e.DirectorId).HasColumnName("director_id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.Country)
                .HasMaxLength(80)
                .HasColumnName("country");
            entity.Property(e => e.DirectorName)
                .HasMaxLength(150)
                .HasColumnName("director_name");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasDefaultValue("Active")
                .HasColumnName("status");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.MovieId).HasName("PK__movie__83CDF7492A8CA1FD");

            entity.ToTable("movie");

            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DirectorId).HasColumnName("director_id");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Genre)
                .HasMaxLength(50)
                .HasColumnName("genre");
            entity.Property(e => e.PosterUrl)
                .HasMaxLength(500)
                .HasColumnName("poster_url");
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
            entity.Property(e => e.Status)
                .HasMaxLength(12)
                .HasDefaultValue("Available")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.TrailerUrl)
                .HasMaxLength(500)
                .HasColumnName("trailer_url");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Movies)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__movie__created_b__34C8D9D1");

            entity.HasOne(d => d.Director).WithMany(p => p.Movies)
                .HasForeignKey(d => d.DirectorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__movie__director___33D4B598");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__rating__D35B278B891D268C");

            entity.ToTable("rating");

            entity.HasIndex(e => new { e.MovieId, e.UserId }, "UQ_rating_movie_user").IsUnique();

            entity.Property(e => e.RatingId).HasColumnName("rating_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Movie).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("FK__rating__movie_id__4E88ABD4");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__rating__user_id__4F7CD00D");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__user_acc__B9BE370F64539FD7");

            entity.ToTable("user_account");

            entity.HasIndex(e => e.Username, "UQ__user_acc__F3DBC57280C1DAB2").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(100)
                .HasColumnName("display_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasDefaultValue(2)
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasDefaultValue("Active")
                .HasColumnName("status");
            entity.Property(e => e.Username)
                .HasMaxLength(120)
                .HasColumnName("username");
        });

        modelBuilder.Entity<WatchingHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__watching__096AA2E98A782146");

            entity.ToTable("watching_history");

            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WatchDuration).HasColumnName("watch_duration");
            entity.Property(e => e.WatchedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("watched_at");

            entity.HasOne(d => d.Movie).WithMany(p => p.WatchingHistories)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("FK__watching___movie__4316F928");

            entity.HasOne(d => d.User).WithMany(p => p.WatchingHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__watching___user___4222D4EF");
        });

        modelBuilder.Entity<Watchlist>(entity =>
        {
            entity.HasKey(e => e.WatchlistId).HasName("PK__watchlis__36A90B31A089A5F9");

            entity.ToTable("watchlist");

            entity.Property(e => e.WatchlistId).HasColumnName("watchlist_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsPublic).HasColumnName("is_public");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Watchlists)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__watchlist__user___398D8EEE");
        });

        modelBuilder.Entity<WatchlistMovie>(entity =>
        {
            entity.HasKey(e => new { e.WatchlistId, e.MovieId }).HasName("PK__watchlis__BE95D445FAD55EB3");

            entity.ToTable("watchlist_movie");

            entity.Property(e => e.WatchlistId).HasColumnName("watchlist_id");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("added_at");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");

            entity.HasOne(d => d.Movie).WithMany(p => p.WatchlistMovies)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("FK__watchlist__movie__3E52440B");

            entity.HasOne(d => d.Watchlist).WithMany(p => p.WatchlistMovies)
                .HasForeignKey(d => d.WatchlistId)
                .HasConstraintName("FK__watchlist__watch__3D5E1FD2");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
