using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MovieApp.Group01
{
    public partial class UserManagementWindow : Window
    {
        private readonly UserAccountService _userService = new();
        private List<UserAccount> _allUsers = new();

        public UserManagementWindow()
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
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                _allUsers = _userService.GetAllUsers();
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách người dùng: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            string keyword = SearchBox?.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(keyword))
            {
                UsersDataGrid.ItemsSource = _allUsers;
                return;
            }

            var filtered = _allUsers
                .Where(u => u.Username.ToLower().Contains(keyword) ||
                           u.DisplayName.ToLower().Contains(keyword) ||
                           u.Status.ToLower().Contains(keyword))
                .ToList();

            UsersDataGrid.ItemsSource = filtered;
        }

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = UsersDataGrid.SelectedItem != null;
            EditButton.IsEnabled = hasSelection;
            BanButton.IsEnabled = hasSelection;
            ChangeRoleButton.IsEnabled = hasSelection;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addWindow = new AddEditUserWindow();
                if (addWindow.ShowDialog() == true)
                {
                    LoadUsers();
                    MessageBox.Show("Thêm người dùng thành công!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm người dùng: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is not UserAccount selectedUser)
            {
                MessageBox.Show("Vui lòng chọn người dùng cần sửa.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var editWindow = new AddEditUserWindow(selectedUser);
                if (editWindow.ShowDialog() == true)
                {
                    LoadUsers();
                    MessageBox.Show("Cập nhật người dùng thành công!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật người dùng: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BanButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is not UserAccount selectedUser)
            {
                MessageBox.Show("Vui lòng chọn người dùng.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedUser.UserId == SessionContext.CurrentUserId)
            {
                MessageBox.Show("Bạn không thể ban chính mình.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newStatus = selectedUser.Status == "Active" ? "Banned" : "Active";
            string action = newStatus == "Banned" ? "ban" : "unban";

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn {action} người dùng '{selectedUser.Username}'?",
                $"Confirm {action}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _userService.ChangeStatus(selectedUser.UserId, newStatus);
                    LoadUsers();
                    MessageBox.Show($"{action} người dùng thành công!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi {action} người dùng: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ChangeRoleButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is not UserAccount selectedUser)
            {
                MessageBox.Show("Vui lòng chọn người dùng.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedUser.UserId == SessionContext.CurrentUserId)
            {
                MessageBox.Show("Bạn không thể thay đổi role của chính mình.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int newRole = selectedUser.Role == 1 ? 2 : 1;
            string newRoleName = newRole == 1 ? "Admin" : "User";

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn đổi role của '{selectedUser.Username}' thành {newRoleName}?",
                "Confirm Change Role",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _userService.ChangeRole(selectedUser.UserId, newRole);
                    LoadUsers();
                    MessageBox.Show("Đổi role thành công!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đổi role: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }
    }

    public class RoleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int role)
            {
                return role == 1 ? "Admin" : "User";
            }
            return "User";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

