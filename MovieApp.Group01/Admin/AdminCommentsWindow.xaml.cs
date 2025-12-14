using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MovieApp.Group01
{
    public partial class AdminCommentsWindow : Window
    {
        private readonly Movie _movie;
        private readonly CommentService _commentService = new();
        private readonly RatingService _ratingService = new();
        private readonly UserAccountService _userService = new();
        private int _commentsLoaded = 0;
        private const int _commentsPerPage = 20;

        public AdminCommentsWindow(Movie movie)
        {
            InitializeComponent();
            _movie = movie;
            MovieTitleText.Text = $"Comments & Ratings: {movie.Title}";
            Loaded += AdminCommentsWindow_Loaded;
        }

        private void AdminCommentsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRatingStatistics();
            LoadComments();
        }

        private void LoadRatingStatistics()
        {
            try
            {
                var avgRating = _ratingService.GetAverageRating(_movie.MovieId);
                var ratingCount = _ratingService.GetRatingCount(_movie.MovieId);

                AverageRatingText.Text = avgRating > 0 ? $"{avgRating:F1}/5" : "N/A";
                RatingCountText.Text = $"{ratingCount} rating{(ratingCount != 1 ? "s" : "")}";
            }
            catch (Exception ex)
            {
                AverageRatingText.Text = "Error";
                RatingCountText.Text = ex.Message;
            }
        }

        private void LoadComments()
        {
            try
            {
                var totalComments = _commentService.GetCommentCount(_movie.MovieId);
                TotalCommentsText.Text = totalComments.ToString();

                var comments = _commentService.GetCommentsForMovie(_movie.MovieId, _commentsLoaded, _commentsPerPage);

                if (comments.Count == 0 && _commentsLoaded == 0)
                {
                    var noComments = new TextBlock
                    {
                        Text = "No comments yet.",
                        FontStyle = FontStyles.Italic,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")),
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 20),
                        FontSize = 14
                    };
                    CommentsPanel.Children.Add(noComments);
                    return;
                }

                foreach (var comment in comments)
                {
                    var commentCard = CreateCommentCard(comment);
                    CommentsPanel.Children.Add(commentCard);
                }

                _commentsLoaded += comments.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải comments: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CreateCommentCard(Comment comment)
        {
            var card = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9F9F9")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var stackPanel = new StackPanel();

            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            headerPanel.Children.Add(new TextBlock
            {
                Text = comment.User?.DisplayName ?? "Unknown",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B3C46")),
                Margin = new Thickness(0, 0, 10, 0)
            });
            headerPanel.Children.Add(new TextBlock
            {
                Text = comment.CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666"))
            });

            stackPanel.Children.Add(headerPanel);

            stackPanel.Children.Add(new TextBlock
            {
                Text = comment.Content,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#370808")),
                Margin = new Thickness(0, 0, 0, 8)
            });

            var actionPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var replyButton = new Button
            {
                Content = "Reply",
                Style = (Style)FindResource("ButtonStyle"),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3")),
                Tag = comment.CommentId,
                Margin = new Thickness(0, 0, 5, 0)
            };
            replyButton.Click += ReplyButton_Click;
            actionPanel.Children.Add(replyButton);

            if (comment.User != null && comment.User.Role != 1)
            {
                var banButton = new Button
                {
                    Content = comment.User.Status == "Banned" ? "Unban" : "Ban User",
                    Style = (Style)FindResource("ButtonStyle"),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")),
                    Tag = comment.User.UserId,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                banButton.Click += BanUserButton_Click;
                actionPanel.Children.Add(banButton);
            }

            var deleteButton = new Button
            {
                Content = "Delete",
                Style = (Style)FindResource("ButtonStyle"),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")),
                Tag = comment.CommentId,
                Margin = new Thickness(0, 0, 5, 0)
            };
            deleteButton.Click += DeleteButton_Click;
            actionPanel.Children.Add(deleteButton);

            stackPanel.Children.Add(actionPanel);

            if (comment.InverseParentComment != null && comment.InverseParentComment.Any())
            {
                var repliesPanel = new StackPanel
                {
                    Margin = new Thickness(20, 10, 0, 0),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"))
                };

                foreach (var reply in comment.InverseParentComment)
                {
                    var replyCard = CreateReplyCard(reply);
                    repliesPanel.Children.Add(replyCard);
                }

                stackPanel.Children.Add(repliesPanel);
            }

            card.Child = stackPanel;
            return card;
        }

        private Border CreateReplyCard(Comment reply)
        {
            var replyCard = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var stackPanel = new StackPanel();

            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5) };
            headerPanel.Children.Add(new TextBlock
            {
                Text = reply.User?.DisplayName ?? "Unknown",
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B3C46")),
                Margin = new Thickness(0, 0, 10, 0)
            });
            headerPanel.Children.Add(new TextBlock
            {
                Text = reply.CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "",
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666"))
            });

            stackPanel.Children.Add(headerPanel);
            stackPanel.Children.Add(new TextBlock
            {
                Text = reply.Content,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#370808"))
            });

            var deleteButton = new Button
            {
                Content = "Delete",
                Style = (Style)FindResource("ButtonStyle"),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336")),
                Tag = reply.CommentId,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 0, 0)
            };
            deleteButton.Click += DeleteButton_Click;
            stackPanel.Children.Add(deleteButton);

            replyCard.Child = stackPanel;
            return replyCard;
        }

        private void ReplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int parentCommentId)
            {
                var replyWindow = new AdminReplyWindow(_movie.MovieId, parentCommentId);
                if (replyWindow.ShowDialog() == true)
                {
                    CommentsPanel.Children.Clear();
                    _commentsLoaded = 0;
                    LoadComments();
                }
            }
        }

        private void BanUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                try
                {
                    var user = _userService.GetUserById(userId);
                    if (user == null)
                    {
                        MessageBox.Show("Không tìm thấy user.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (user.Role == 1)
                    {
                        MessageBox.Show("Không thể ban tài khoản Admin.", "Warning",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string newStatus = user.Status == "Active" ? "Banned" : "Active";
                    string action = newStatus == "Banned" ? "ban" : "unban";

                    var result = MessageBox.Show(
                        $"Bạn có chắc chắn muốn {action} người dùng '{user.Username}'?\n\n",
                        $"Confirm {action}",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _userService.ChangeStatus(userId, newStatus);
                        CommentsPanel.Children.Clear();
                        _commentsLoaded = 0;
                        LoadComments();
                        MessageBox.Show($"{action} người dùng thành công!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi thao tác người dùng: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int commentId)
            {
                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa comment này?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _commentService.DeleteComment(commentId);
                        CommentsPanel.Children.Clear();
                        _commentsLoaded = 0;
                        LoadComments();
                        LoadRatingStatistics();
                        MessageBox.Show("Xóa comment thành công!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa comment: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}

