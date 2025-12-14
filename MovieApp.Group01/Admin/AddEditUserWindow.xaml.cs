using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MovieApp.Group01
{
    public partial class AddEditUserWindow : Window
    {
        private readonly UserAccountService _userService = new();
        private readonly UserAccount? _existingUser;
        private bool IsEditMode => _existingUser != null;

        public AddEditUserWindow()
        {
            InitializeComponent();
            Title = "Add User";
        }

        public AddEditUserWindow(UserAccount user) : this()
        {
            _existingUser = user;
            Title = "Edit User";
            UsernameTextBox.IsReadOnly = true;
            LoadUserData();
        }

        private void LoadUserData()
        {
            if (_existingUser == null) return;

            DisplayNameTextBox.Text = _existingUser.DisplayName;
            UsernameTextBox.Text = _existingUser.Username;

            foreach (ComboBoxItem item in RoleComboBox.Items)
            {
                if (item.Tag?.ToString() == _existingUser.Role.ToString())
                {
                    RoleComboBox.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Content.ToString() == _existingUser.Status)
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void DisplayNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateDisplayName();
        }

        private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateUsername();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ValidatePassword();
        }

        private bool ValidateDisplayName()
        {
            bool isValid = !string.IsNullOrWhiteSpace(DisplayNameTextBox.Text);
            DisplayNameError.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
            return isValid;
        }

        private bool ValidateUsername()
        {
            bool isValid = !string.IsNullOrWhiteSpace(UsernameTextBox.Text);
            UsernameError.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
            return isValid;
        }

        private bool ValidatePassword()
        {
            bool isValid = !string.IsNullOrWhiteSpace(PasswordBox.Password) || IsEditMode;
            PasswordError.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
            return isValid;
        }

        private bool ValidateForm()
        {
            bool displayNameValid = ValidateDisplayName();
            bool usernameValid = ValidateUsername();
            bool passwordValid = ValidatePassword();

            if (!displayNameValid || !usernameValid || !passwordValid)
            {
                MessageBox.Show("Vui lòng điền đầy đủ các trường bắt buộc (*).",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionContext.CurrentRole != 1)
            {
                MessageBox.Show("Chỉ Admin mới được thêm/sửa người dùng.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateForm())
                return;

            try
            {
                if (IsEditMode && _existingUser != null)
                {
                    var existingUserFromDb = _userService.GetUserById(_existingUser.UserId);
                    if (existingUserFromDb == null)
                    {
                        MessageBox.Show("Người dùng không tồn tại trong hệ thống.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string? newPassword = string.IsNullOrWhiteSpace(PasswordBox.Password) 
                        ? null 
                        : PasswordBox.Password;

                    _userService.UpdateProfile(
                        existingUserFromDb.UserId,
                        DisplayNameTextBox.Text.Trim(),
                        UsernameTextBox.Text.Trim(),
                        newPassword);

                    int newRole = int.Parse((RoleComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "2");
                    if (existingUserFromDb.Role != newRole)
                    {
                        _userService.ChangeRole(existingUserFromDb.UserId, newRole);
                    }

                    string newStatus = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Active";
                    if (existingUserFromDb.Status != newStatus)
                    {
                        _userService.ChangeStatus(existingUserFromDb.UserId, newStatus);
                    }
                }
                else
                {
                    int role = int.Parse((RoleComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "2");
                    string status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Active";

                    bool success = _userService.CreateUser(
                        DisplayNameTextBox.Text.Trim(),
                        UsernameTextBox.Text.Trim(),
                        PasswordBox.Password,
                        role);

                    if (!success)
                    {
                        MessageBox.Show("Username đã tồn tại.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var newUser = _userService.GetAllUsers()
                        .FirstOrDefault(u => u.Username == UsernameTextBox.Text.Trim());

                    if (newUser != null && newUser.Status != status)
                    {
                        _userService.ChangeStatus(newUser.UserId, status);
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu người dùng: {ex.Message}",
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

