using Microsoft.EntityFrameworkCore;
using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovieApp.DAL.Repositories
{
    public class CommentRepo
    {
        private readonly MovieAppContext _ctx = new();

        public void AddComment(int userId, int movieId, string content, int? parentCommentId = null)
        {
            var comment = new Comment
            {
                UserId = userId,
                MovieId = movieId,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };
            _ctx.Comments.Add(comment);
            _ctx.SaveChanges();
        }

        public List<Comment> GetCommentsForMovie(int movieId, int skip = 0, int take = 10)
        {
            return _ctx.Comments
                .Include(c => c.User)
                .Include(c => c.InverseParentComment)
                    .ThenInclude(reply => reply.User)
                .Where(c => c.MovieId == movieId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        public List<Comment> GetRepliesForComment(int commentId)
        {
            return _ctx.Comments
                .Include(c => c.User)
                .Where(c => c.ParentCommentId == commentId)
                .OrderBy(c => c.CreatedAt)
                .ToList();
        }

        public int GetCommentCount(int movieId)
        {
            return _ctx.Comments
                .Count(c => c.MovieId == movieId);
        }

        public void DeleteComment(int commentId)
        {
            var comment = _ctx.Comments.Find(commentId);
            if (comment != null)
            {
                _ctx.Comments.Remove(comment);
                _ctx.SaveChanges();
            }
        }

        public void UpdateComment(int commentId, string newContent)
        {
            var comment = _ctx.Comments.Find(commentId);
            if (comment != null)
            {
                comment.Content = newContent;
                _ctx.Comments.Update(comment);
                _ctx.SaveChanges();
            }
        }
    }
}
