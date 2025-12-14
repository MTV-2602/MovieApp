using MovieApp.BLL.Services;
using System;
using System.Windows;

namespace MovieApp.Group01
{
    public partial class AdminReplyWindow : Window
    {
        private readonly CommentService _commentService = new();
        private readonly int _movieId;
        private readonly int _parentCommentId;

        public AdminReplyWindow(int movieId, int parentCommentId)
        {
            InitializeComponent();
            _movieId = movieId;
            _parentCommentId = parentCommentId;
        }

        private void PostButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ReplyTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập nội dung reply.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (SessionContext.CurrentUserId == 0)
                {
                    MessageBox.Show("Bạn cần đăng nhập để reply.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _commentService.AddComment(
                    SessionContext.CurrentUserId,
                    _movieId,
                    ReplyTextBox.Text.Trim(),
                    _parentCommentId);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi post reply: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

