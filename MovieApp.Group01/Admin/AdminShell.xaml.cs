using System.Windows;
using System.Windows.Controls;

namespace MovieApp.Group01
{
    public partial class AdminShell : Window
    {
        public AdminShell()
        {
            InitializeComponent();

            if (SessionContext.CurrentRole != 1)
            {
                MessageBox.Show("Bạn không có quyền truy cập trang quản trị.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NavigateToDashboard();
            SetSelectedButton(DashboardButton);
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToDashboard();
            SetSelectedButton(DashboardButton);
        }

        private void MoviesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToMovies();
            SetSelectedButton(MoviesButton);
        }

        private void DirectorsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToDirectors();
            SetSelectedButton(DirectorsButton);
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToUsers();
            SetSelectedButton(UsersButton);
        }

        private void NavigateToDashboard()
        {
            ContentFrame.Navigate(new AdminDashboardPage());
        }

        private void NavigateToMovies()
        {
            ContentFrame.Navigate(new MovieAdminPage());
        }

        private void NavigateToDirectors()
        {
            ContentFrame.Navigate(new DirectorManagementPage());
        }

        private void NavigateToUsers()
        {
            ContentFrame.Navigate(new UserManagementPage());
        }

        private void SetSelectedButton(Button selected)
        {
            DashboardButton.Style = (Style)FindResource("NavMenuButton");
            MoviesButton.Style = (Style)FindResource("NavMenuButton");
            DirectorsButton.Style = (Style)FindResource("NavMenuButton");
            UsersButton.Style = (Style)FindResource("NavMenuButton");

            selected.Style = (Style)FindResource("NavMenuButtonSelected");
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SessionContext.Clear();

                var loginWindow = new LoginWindow();
                loginWindow.Show();

                this.Close();
            }
        }
    }
}

