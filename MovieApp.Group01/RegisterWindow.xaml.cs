using Microsoft.IdentityModel.Tokens;
using MovieApp.BLL.Services;
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
    public partial class RegisterWindow : Window
    {
        private readonly UserAccountService _service = new();

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextbox.Text;
            string password = PasswordTextbox.Password;
            string displayName = DisplayNameTextBox.Text;
            string confirmPassword = ConfirmPasswordTextbox.Password;

            // ============================
            // VALIDATION
            // ============================
            if (username.IsNullOrEmpty())
            {
                MessageBox.Show("Username cannot be empty.", "Input Error");
                return;
            }
            if (displayName.IsNullOrEmpty())
            {
                MessageBox.Show("Display Name cannot be empty.", "Input Error");
                return;
            }
            if (password.IsNullOrEmpty())
            {
                MessageBox.Show("Password cannot be empty.", "Input Error");
                return;
            }
            if (confirmPassword.IsNullOrEmpty())
            {
                MessageBox.Show("Please confirm your password.", "Input Error");
                return;
            }
            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Input Error");
                return;
            }

            // ============================
            // REGISTER
            // ============================
            bool success = _service.Register(displayName, username, password);

            if (!success)
            {
                MessageBox.Show("Username already exists.", "Registration Failed");
                return;
            }

            MessageBox.Show("Registration successful!", "Success");

            // Open login window
            LoginWindow login = new();
            login.Show();
            this.Close();
        }

        private void LogIn_Click(object sender, MouseButtonEventArgs e)
        {
            LoginWindow loginWindow = new();
            loginWindow.Show();
            this.Close();
        }
    }
}
