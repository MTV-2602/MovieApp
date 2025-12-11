using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MovieApp.Group01
{
    public partial class CommentsWindow : Window
    {
        private readonly CommentService _commentService = new();
        private readonly int _movieId;
        private readonly string _movieTitle;

        // Pagination
        private int _commentsLoaded = 0;
        private const int _commentsPerPage = 10;
        private int _totalComments = 0;

        public CommentsWindow(int movieId, string movieTitle)
        {
            InitializeComponent();
            _movieId = movieId;
            _movieTitle = movieTitle;

            TxtMovieTitle.Text = _movieTitle;
            
            LoadCommentCount();
            LoadComments();
        }

        private void LoadCommentCount()
        {
            _totalComments = _commentService.GetCommentCount(_movieId);
            TxtCommentCount.Text = $"{_totalComments} comment{(_totalComments != 1 ? "s" : "")}";
        }

        private void LoadComments()
        {
            var comments = _commentService.GetCommentsForMovie(_movieId, _commentsLoaded, _commentsPerPage);

            if (comments.Count == 0 && _commentsLoaded == 0)
            {
                var noComments = new TextBlock
                {
                    Text = "No comments yet. Be the first to comment!",
                    FontStyle = FontStyles.Italic,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97E8A")),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 40, 0, 40),
                    FontSize = 14
                };
                CommentsPanel.Children.Add(noComments);
                BtnLoadMore.Visibility = Visibility.Collapsed;
                return;
            }

            foreach (var comment in comments)
            {
                var commentBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF5F5")),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 12)
                };

                var commentStack = new StackPanel();

                // User + Date header
                var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
                
                var userText = new TextBlock
                {
                    Text = comment.User?.Username ?? "Anonymous",
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#370808")),
                    FontSize = 14
                };
                headerPanel.Children.Add(userText);

                var dateText = new TextBlock
                {
                    Text = " • " + (comment.CreatedAt?.ToString("MMM dd, yyyy HH:mm") ?? ""),
                    FontSize = 12,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B3C46")),
                    FontStyle = FontStyles.Italic,
                    VerticalAlignment = VerticalAlignment.Center
                };
                headerPanel.Children.Add(dateText);

                commentStack.Children.Add(headerPanel);

                // Content
                var contentText = new TextBlock
                {
                    Text = comment.Content,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#370808")),
                    FontSize = 14,
                    LineHeight = 20
                };
                commentStack.Children.Add(contentText);

                commentBorder.Child = commentStack;
                CommentsPanel.Children.Add(commentBorder);

                // Add replies
                if (comment.InverseParentComment.Count > 0)
                {
                    foreach (var reply in comment.InverseParentComment)
                    {
                        var replyBorder = new Border
                        {
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")),
                            CornerRadius = new CornerRadius(10),
                            Padding = new Thickness(12),
                            Margin = new Thickness(30, 0, 0, 12),
                            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EC9FAB")),
                            BorderThickness = new Thickness(2, 0, 0, 0)
                        };

                        var replyStack = new StackPanel();

                        var replyHeaderPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

                        var replyUserText = new TextBlock
                        {
                            Text = "Reply: " + (reply.User?.Username ?? "Anonymous"),
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B3C46")),
                            FontSize = 13
                        };
                        replyHeaderPanel.Children.Add(replyUserText);

                        var replyDateText = new TextBlock
                        {
                            Text = " • " + (reply.CreatedAt?.ToString("MMM dd, yyyy HH:mm") ?? ""),
                            FontSize = 11,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B3C46")),
                            FontStyle = FontStyles.Italic,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        replyHeaderPanel.Children.Add(replyDateText);

                        replyStack.Children.Add(replyHeaderPanel);

                        var replyContentText = new TextBlock
                        {
                            Text = reply.Content,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#370808")),
                            FontSize = 13,
                            LineHeight = 18
                        };
                        replyStack.Children.Add(replyContentText);

                        replyBorder.Child = replyStack;
                        CommentsPanel.Children.Add(replyBorder);
                    }
                }
            }

            _commentsLoaded += comments.Count;

            // Update Load More button
            if (_commentsLoaded < _totalComments)
            {
                BtnLoadMore.Visibility = Visibility.Visible;
                BtnLoadMore.Content = $"Load More ({_commentsLoaded}/{_totalComments})";
            }
            else
            {
                BtnLoadMore.Visibility = Visibility.Collapsed;
            }
        }

        private void PostComment_Click(object sender, RoutedEventArgs e)
        {
            if (SessionContext.CurrentUserId == 0)
            {
                MessageBox.Show("Please login to post a comment.", "Login Required", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string content = TxtComment.Text.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Please enter a comment.", "Validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _commentService.AddComment(SessionContext.CurrentUserId, _movieId, content);
                TxtComment.Clear();

                // Reset and reload
                _commentsLoaded = 0;
                CommentsPanel.Children.Clear();
                LoadCommentCount();
                LoadComments();

                MessageBox.Show("Comment posted successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error posting comment: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMore_Click(object sender, RoutedEventArgs e)
        {
            LoadComments();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
