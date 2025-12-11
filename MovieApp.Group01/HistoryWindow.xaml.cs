using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MovieApp.Group01
{
    /// <summary>
    /// Interaction logic for HistoryWindow.xaml
    /// </summary>
    public partial class HistoryWindow : Window
    {
        private readonly WatchingHistoryService _historyService = new();

        public HistoryWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadHistory();
        }

        private void LoadHistory()
        {
            if (SessionContext.CurrentUserId == 0) return;

            var histories = _historyService.GetHistoryByUserId(SessionContext.CurrentUserId);
            HistoryListPanel.Children.Clear();

            if (histories.Count == 0)
            {
                HistoryListPanel.Children.Add(new TextBlock
                {
                    Text = "You haven't watched any movies yet.",
                    FontSize = 18,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                });
                return;
            }

            foreach (var item in histories)
            {
                if (item.Movie == null) continue;
                HistoryListPanel.Children.Add(CreateHistoryItem(item));
            }
        }

        private Border CreateHistoryItem(WatchingHistory history)
        {
            var border = new Border { Style = (Style)FindResource("HistoryItemBorder") };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Poster
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Info
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Date

            // 1. Poster
            var poster = new Image { Stretch = Stretch.UniformToFill, Height = 120 };
            try
            {
                if (!string.IsNullOrEmpty(history.Movie.PosterUrl))
                    poster.Source = new BitmapImage(new Uri(history.Movie.PosterUrl, UriKind.RelativeOrAbsolute));
            }
            catch { }

            var posterCard = new Border { CornerRadius = new CornerRadius(10), ClipToBounds = true, Child = poster };
            Grid.SetColumn(posterCard, 0);

            // 2. Info
            var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(20, 0, 0, 0) };
            stack.Children.Add(new TextBlock { Text = history.Movie.Title, FontSize = 20, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("PrimaryBrush") });
            stack.Children.Add(new TextBlock { Text = $"Genre: {history.Movie.Genre}", FontSize = 14, Foreground = Brushes.Gray, Margin = new Thickness(0, 5, 0, 0) });

            // Format thời gian đã xem
            TimeSpan t = TimeSpan.FromSeconds(history.WatchDuration ?? 0);
            stack.Children.Add(new TextBlock { Text = $"Stopped at: {t:mm\\:ss}", FontSize = 14, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("SecondaryBrush"), Margin = new Thickness(0, 10, 0, 0) });

            Grid.SetColumn(stack, 1);

            // 3. Date
            var dateTxt = new TextBlock
            {
                Text = history.WatchedAt?.ToString("dd/MM/yyyy HH:mm"),
                VerticalAlignment = VerticalAlignment.Top,
                Foreground = Brushes.Gray
            };
            Grid.SetColumn(dateTxt, 2);

            grid.Children.Add(posterCard);
            grid.Children.Add(stack);
            grid.Children.Add(dateTxt);

            border.Child = grid;

            // Click vào item để xem tiếp phim
            border.MouseLeftButtonUp += (s, e) =>
            {
                var player = new MoviePlayerWindow(history.MovieId);
                this.Hide();
                player.Closed += (sender, args) => this.Show(); // Hoặc load lại history để cập nhật thời gian mới
                player.Show();
            };

            return border;
        }

        // Navigation Events
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var home = new HomepageWindow();
            home.Show();
            this.Close();
        }
        private void WatchlistButton_Click(object sender, RoutedEventArgs e)
        {
            var watchlist = new WatchlistWindow();

            this.Hide();

            watchlist.Closed += (s, args) => this.Show();

            watchlist.Show();
        }
        private void ProfileButton_Click(object sender, RoutedEventArgs e) { new UpdateProfileWindow(SessionContext.CurrentUserId).ShowDialog(); }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear session
                SessionContext.CurrentUserId = 0;
                SessionContext.CurrentUsername = string.Empty;

                // Show login window
                var loginWindow = new LoginWindow();
                loginWindow.Show();

                // Close current window
                this.Close();
            }
        }
    }
}
