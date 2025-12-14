using MovieApp.BLL.Services;
using System.Windows.Controls;

namespace MovieApp.Group01
{
    public partial class AdminDashboardPage : Page
    {
        private readonly DashboardService _dashboardService = new();

        public AdminDashboardPage()
        {
            InitializeComponent();
            Loaded += AdminDashboardPage_Loaded;
        }

        private void AdminDashboardPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                TotalMoviesText.Text = _dashboardService.GetTotalMovies().ToString();
                TotalUsersText.Text = _dashboardService.GetTotalUsers().ToString();
                TotalWatchesText.Text = _dashboardService.GetTotalWatches().ToString();

                AvailableMoviesText.Text = _dashboardService.GetAvailableMovies().ToString();
                UnavailableMoviesText.Text = _dashboardService.GetUnavailableMovies().ToString();
                AdminCountText.Text = _dashboardService.GetAdminCount().ToString();
                RegularUserCountText.Text = _dashboardService.GetRegularUserCount().ToString();

                TotalDirectorsText.Text = _dashboardService.GetTotalDirectors().ToString();

                var latestMovies = _dashboardService.GetLatestMovies(5);
                LatestMoviesList.ItemsSource = latestMovies;

                var mostWatchedMovies = _dashboardService.GetMostWatchedMovies(5);
                MostWatchedMoviesList.ItemsSource = mostWatchedMovies;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải dữ liệu dashboard: {ex.Message}",
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}

