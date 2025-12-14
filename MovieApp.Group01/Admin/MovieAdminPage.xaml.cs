using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MovieApp.Group01
{
    public partial class MovieAdminPage : Page
    {
        private readonly MovieService _movieService = new();
        private List<Movie> _allMovies = new();

        public MovieAdminPage()
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
            LoadMovies();
        }

        private void LoadMovies()
        {
            try
            {
                _allMovies = _movieService.GetAllMovies();
                MoviesDataGrid.ItemsSource = _allMovies;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách phim: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = SearchBox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                MoviesDataGrid.ItemsSource = _allMovies;
                return;
            }

            var filtered = _allMovies
                .Where(m => m.Title.ToLower().Contains(keyword) ||
                           (m.Description?.ToLower().Contains(keyword) ?? false) ||
                           m.Genre.ToLower().Contains(keyword) ||
                           (m.Director?.DirectorName.ToLower().Contains(keyword) ?? false))
                .ToList();

            MoviesDataGrid.ItemsSource = filtered;
        }

        private void MoviesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = MoviesDataGrid.SelectedItem != null;
            EditButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
            ViewCommentsButton.IsEnabled = hasSelection;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addWindow = new AddMovieWindow();
                if (addWindow.ShowDialog() == true)
                {
                    LoadMovies();
                    MessageBox.Show("Thêm phim thành công!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm phim: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (MoviesDataGrid.SelectedItem is not Movie selectedMovie)
            {
                MessageBox.Show("Vui lòng chọn phim cần sửa.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var editWindow = new AddMovieWindow(selectedMovie);
                if (editWindow.ShowDialog() == true)
                {
                    LoadMovies();
                    MessageBox.Show("Cập nhật phim thành công!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật phim: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MoviesDataGrid.SelectedItem is not Movie selectedMovie)
            {
                MessageBox.Show("Vui lòng chọn phim cần xóa.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa phim '{selectedMovie.Title}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _movieService.DeleteMovie(selectedMovie.MovieId);
                    LoadMovies();
                    MessageBox.Show("Xóa phim thành công!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa phim: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMovies();
        }

        private void ViewCommentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (MoviesDataGrid.SelectedItem is not Movie selectedMovie)
            {
                MessageBox.Show("Vui lòng chọn phim cần xem.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var adminCommentsWindow = new AdminCommentsWindow(selectedMovie);
                adminCommentsWindow.Owner = Application.Current.MainWindow;
                adminCommentsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở cửa sổ comments: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

