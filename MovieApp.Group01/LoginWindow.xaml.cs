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

            // ✔ GỌI ĐÚNG SERVICE
            UserAccount? acc = _service.Authenticate(username, password);

            if (acc == null)
            {
                MessageBox.Show("Incorrect username or password.");
                return;
            }

            // Login success
            if (acc.Role == 2)
            {
                SessionContext.CurrentUserId = acc.UserId;
                SessionContext.CurrentUsername = acc.Username;
                var home = new HomepageWindow();
                this.Hide();
                home.ShowDialog();
            }
            else if (acc.Role == 1)
            {
                var admin = new MovieAdminWindow();
                this.Hide();
                admin.ShowDialog();
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