using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MovieApp.Group01
{
    public partial class DirectorManagementPage : Page
    {
        private readonly DirectorService _directorService = new();
        private List<Director> _allDirectors = new();

        public DirectorManagementPage()
        {
            InitializeComponent();

            if (SessionContext.CurrentRole != 1)
            {
                MessageBox.Show("Bạn không có quyền truy cập trang quản trị.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDirectors();
        }

        private void LoadDirectors()
        {
            try
            {
                _allDirectors = _directorService.GetAllDirectors();
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách đạo diễn: {ex.Message}",
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
                DirectorsDataGrid.ItemsSource = _allDirectors;
                return;
            }

            var filtered = _allDirectors
                .Where(d => d.DirectorName.ToLower().Contains(keyword) ||
                           (d.Country?.ToLower().Contains(keyword) ?? false) ||
                           (d.Bio?.ToLower().Contains(keyword) ?? false) ||
                           d.Status.ToLower().Contains(keyword))
                .ToList();

            DirectorsDataGrid.ItemsSource = filtered;
        }

        private void DirectorsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = DirectorsDataGrid.SelectedItem != null;
            EditButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addWindow = new AddEditDirectorWindow();
                if (addWindow.ShowDialog() == true)
                {
                    LoadDirectors();
                    MessageBox.Show("Thêm đạo diễn thành công!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm đạo diễn: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DirectorsDataGrid.SelectedItem is not Director selected)
            {
                MessageBox.Show("Vui lòng chọn đạo diễn cần sửa.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var editWindow = new AddEditDirectorWindow(selected);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDirectors();
                    MessageBox.Show("Cập nhật đạo diễn thành công!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật đạo diễn: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DirectorsDataGrid.SelectedItem is not Director selected)
            {
                MessageBox.Show("Vui lòng chọn đạo diễn cần xóa.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa đạo diễn '{selected.DirectorName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _directorService.DeleteDirector(selected.DirectorId);
                    LoadDirectors();
                    MessageBox.Show("Xóa đạo diễn thành công!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa đạo diễn: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDirectors();
        }
    }
}

