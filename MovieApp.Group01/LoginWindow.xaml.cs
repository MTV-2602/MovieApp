using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System;
using System.Windows;

namespace MovieApp.Group01
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly UserAccountService _service = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextbox.Text;
            string password = PasswordTextbox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            UserAccount? acc = _service.Authenticate(username, password);

            if (acc == null)
            {
                MessageBox.Show("Incorrect username or password.");
                return;
            }

            if (acc.Status == "Banned")
            {
                MessageBox.Show("Tài khoản của bạn đã bị ban. Vui lòng liên hệ admin.",
                    "Account Banned", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (acc.Status == "Inactive")
            {
                MessageBox.Show("Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ admin.",
                    "Account Inactive", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SessionContext.CurrentUserId = acc.UserId;
            SessionContext.CurrentUsername = acc.Username;
            SessionContext.CurrentRole = acc.Role;

            try
            {
                Window targetWindow = acc.Role switch
                {
                    1 => new AdminShell(),
                    2 => new HomepageWindow(),
                    _ => null
                };

                if (targetWindow == null)
                {
                    MessageBox.Show("Vai trò không hợp lệ.", "Access Denied",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                targetWindow.Loaded += (s, e) =>
                {
                    this.Close();
                };
                
                targetWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở cửa sổ: {ex.Message}\n\n{ex.StackTrace}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            var win = new RegisterWindow();
            win.Show();
            this.Close();
        }
    }
}