using MovieApp.DAL.Entities;
using MovieApp.DAL.Repositories;
using System;
using System.Collections.Generic;

namespace MovieApp.BLL.Services
{
    public class CommentService
    {
        private readonly CommentRepo _repo = new();

        public void AddComment(int userId, int movieId, string content, int? parentCommentId = null)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Comment content cannot be empty");

            _repo.AddComment(userId, movieId, content, parentCommentId);
        }

        public List<Comment> GetCommentsForMovie(int movieId, int skip = 0, int take = 10)
        {
            return _repo.GetCommentsForMovie(movieId, skip, take);
        }

        public List<Comment> GetRepliesForComment(int commentId)
        {
            return _repo.GetRepliesForComment(commentId);
        }

        public int GetCommentCount(int movieId)
        {
            return _repo.GetCommentCount(movieId);
        }

        public void DeleteComment(int commentId)
        {
            _repo.DeleteComment(commentId);
        }

        public void UpdateComment(int commentId, string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent))
                throw new ArgumentException("Comment content cannot be empty");

            _repo.UpdateComment(commentId, newContent);
        }
    }
}
