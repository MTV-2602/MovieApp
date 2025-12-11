using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System;
using System.Windows;

namespace MovieApp.Group01
{
    public partial class UpdateProfileWindow : Window
    {
        private readonly UserAccountService _service = new();
        private readonly int _userId;
        private UserAccount? _currentUser;

        public UpdateProfileWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadUserData();
        }

        private void LoadUserData()
        {
            _currentUser = _service.GetUserById(_userId);
            
            if (_currentUser == null)
            {
                MessageBox.Show("User not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            
            DisplayNameTextBox.Text = _currentUser.DisplayName;
            UsernameTextBox.Text = _currentUser.Username;
            
            CreatedAtText.Text = _currentUser.CreatedAt?.ToString("MMM dd, yyyy") ?? "N/A";
            StatusText.Text = _currentUser.Status ?? "Active";
            CurrentUserText.Text = $"Hello, {_currentUser.DisplayName}!";
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string displayName = DisplayNameTextBox.Text.Trim();
            string username = UsernameTextBox.Text.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(displayName))
            {
                MessageBox.Show("Display Name cannot be empty!", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Username cannot be empty!", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // G?i service ?? update profile (không thay ??i password)
                _service.UpdateProfile(_userId, displayName, username, null);

                // Update SessionContext n?u username thay ??i
                if (SessionContext.CurrentUserId == _userId)
                {
                    SessionContext.CurrentUsername = username;
                }

                MessageBox.Show("Profile updated successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Reload d? li?u
                LoadUserData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating profile: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Validation
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                MessageBox.Show("Please enter your current password!", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CurrentPasswordBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Please enter a new password!", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NewPasswordBox.Focus();
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("New password must be at least 6 characters!", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NewPasswordBox.Focus();
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("New passwords do not match!", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfirmPasswordBox.Focus();
                return;
            }

            if (currentPassword == newPassword)
            {
                MessageBox.Show("New password must be different from current password!", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Verify current password
                var authenticatedUser = _service.Authenticate(_currentUser!.Username, currentPassword);
                
                if (authenticatedUser == null)
                {
                    MessageBox.Show("Current password is incorrect!", "Authentication Failed", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    CurrentPasswordBox.Clear();
                    CurrentPasswordBox.Focus();
                    return;
                }

                // Update password
                _service.UpdateProfile(_userId, _currentUser.DisplayName, _currentUser.Username, newPassword);

                MessageBox.Show("Password changed successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear password fields
                CurrentPasswordBox.Clear();
                NewPasswordBox.Clear();
                ConfirmPasswordBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing password: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
