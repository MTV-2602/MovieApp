using Microsoft.Win32;
using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MovieApp.Group01
{
    public partial class AddMovieWindow : Window
    {
        private readonly MovieService _movieService = new();
        private readonly DirectorService _directorService = new();
        private readonly SupabaseService _supabaseService = new();
        private readonly Movie? _existingMovie;
        private bool _isEditMode => _existingMovie != null;
        private string? _selectedPosterFilePath;
        private string? _selectedTrailerFilePath;

        public AddMovieWindow()
        {
            InitializeComponent();
            LoadDirectors();
            Title = "Add Movie";
        }

        public AddMovieWindow(Movie movie) : this()
        {
            _existingMovie = movie;
            Title = "Edit Movie";
            LoadMovieData();
        }

        private void LoadDirectors()
        {
            try
            {
                var directors = _directorService.GetAllDirectors();
                DirectorComboBox.ItemsSource = directors;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách đạo diễn: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMovieData()
        {
            if (_existingMovie == null) return;

            TitleTextBox.Text = _existingMovie.Title;
            DescriptionTextBox.Text = _existingMovie.Description ?? "";

            if (!string.IsNullOrEmpty(_existingMovie.Genre))
            {
                foreach (ComboBoxItem item in GenreComboBox.Items)
                {
                    if (item.Content.ToString() == _existingMovie.Genre)
                    {
                        GenreComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            if (_existingMovie.DirectorId.HasValue)
            {
                DirectorComboBox.SelectedValue = _existingMovie.DirectorId;
            }

            if (_existingMovie.ReleaseDate.HasValue)
            {
                ReleaseDatePicker.SelectedDate = _existingMovie.ReleaseDate.Value.ToDateTime(TimeOnly.MinValue);
            }

            DurationTextBox.Text = _existingMovie.Duration?.ToString() ?? "";

            if (!string.IsNullOrEmpty(_existingMovie.PosterUrl))
            {
                PosterUrlTextBox.Text = _existingMovie.PosterUrl;
                PosterFileNameText.Text = "Current poster";
                PosterFileNameText.Foreground = System.Windows.Media.Brushes.Gray;
            }

            if (!string.IsNullOrEmpty(_existingMovie.TrailerUrl))
            {
                TrailerUrlTextBox.Text = _existingMovie.TrailerUrl;
                TrailerFileNameText.Text = "Current video";
                TrailerFileNameText.Foreground = System.Windows.Media.Brushes.Gray;
            }

            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Content.ToString() == _existingMovie.Status)
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateTitle();
        }

        private void GenreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateGenre();
        }

        private void DurationTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void SelectPosterButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif;*.webp)|*.jpg;*.jpeg;*.png;*.gif;*.webp|All files (*.*)|*.*",
                Title = "Select Poster Image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedPosterFilePath = openFileDialog.FileName;
                PosterFileNameText.Text = Path.GetFileName(_selectedPosterFilePath);
                PosterFileNameText.Foreground = System.Windows.Media.Brushes.Green;
                PosterUrlTextBox.Text = "";
            }
        }

        private void SelectTrailerButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Video files (*.mp4;*.webm;*.ogg;*.mov;*.avi)|*.mp4;*.webm;*.ogg;*.mov;*.avi|All files (*.*)|*.*",
                Title = "Select Trailer Video"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(openFileDialog.FileName);
                const long maxSizeBytes = 100 * 1024 * 1024; // 100MB

                if (fileInfo.Length > maxSizeBytes)
                {
                    var sizeMB = fileInfo.Length / (1024.0 * 1024.0);
                    MessageBox.Show(
                        $"Video file quá lớn ({sizeMB:F2} MB).\n\nGiới hạn: 100 MB\nVui lòng chọn video nhỏ hơn hoặc nén video trước khi upload.",
                        "File Too Large",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                _selectedTrailerFilePath = openFileDialog.FileName;
                var sizeText = fileInfo.Length < 1024 * 1024
                    ? $"{fileInfo.Length / 1024.0:F1} KB"
                    : $"{fileInfo.Length / (1024.0 * 1024.0):F2} MB";
                TrailerFileNameText.Text = $"{Path.GetFileName(_selectedTrailerFilePath)} ({sizeText})";
                TrailerFileNameText.Foreground = System.Windows.Media.Brushes.Green;
                TrailerUrlTextBox.Text = "";
            }
        }

        private bool ValidateTitle()
        {
            bool isValid = !string.IsNullOrWhiteSpace(TitleTextBox.Text);
            TitleError.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
            return isValid;
        }

        private bool ValidateGenre()
        {
            bool isValid = GenreComboBox.SelectedItem != null;
            GenreError.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
            return isValid;
        }

        private bool ValidateForm()
        {
            bool titleValid = ValidateTitle();
            bool genreValid = ValidateGenre();

            if (!titleValid || !genreValid)
            {
                MessageBox.Show("Vui lòng điền đầy đủ các trường bắt buộc (*).",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionContext.CurrentRole != 1)
            {
                MessageBox.Show("Chỉ Admin mới được thêm/sửa phim.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateForm())
                return;

            try
            {
                SaveButton.IsEnabled = false;
                SaveButton.Content = "Uploading...";

                string posterUrl = PosterUrlTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(_selectedPosterFilePath))
                {
                    try
                    {
                        posterUrl = await _supabaseService.UploadPosterAsync(_selectedPosterFilePath);
                        PosterUrlTextBox.Text = posterUrl;
                        PosterFileNameText.Text = "Uploaded successfully";
                        PosterFileNameText.Foreground = System.Windows.Media.Brushes.Green;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi upload poster",
                            "Upload Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        SaveButton.IsEnabled = true;
                        SaveButton.Content = "Save";
                        return;
                    }
                }
                else if (string.IsNullOrWhiteSpace(posterUrl))
                {
                    MessageBox.Show("Vui lòng chọn poster image hoặc nhập Supabase URL.",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SaveButton.IsEnabled = true;
                    SaveButton.Content = "Save";
                    return;
                }

                string trailerUrl = TrailerUrlTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(_selectedTrailerFilePath))
                {
                    try
                    {
                        var fileInfo = new FileInfo(_selectedTrailerFilePath);
                        const long maxSizeBytes = 100 * 1024 * 1024; // 100MB

                        if (fileInfo.Length > maxSizeBytes)
                        {
                            var sizeMB = fileInfo.Length / (1024.0 * 1024.0);
                            MessageBox.Show(
                                $"Video file quá lớn ({sizeMB:F2} MB).\n\nGiới hạn: 100 MB\nVui lòng chọn video nhỏ hơn hoặc nén video trước khi upload.",
                                "File Too Large",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            SaveButton.IsEnabled = true;
                            SaveButton.Content = "Save";
                            return;
                        }

                        trailerUrl = await _supabaseService.UploadVideoAsync(_selectedTrailerFilePath);
                        TrailerUrlTextBox.Text = trailerUrl;
                        TrailerFileNameText.Text = "Uploaded successfully";
                        TrailerFileNameText.Foreground = System.Windows.Media.Brushes.Green;
                    }
                    catch (ArgumentException ex)
                    {
                        MessageBox.Show(ex.Message, "Validation Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        SaveButton.IsEnabled = true;
                        SaveButton.Content = "Save";
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi upload video",
                            "Upload Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        SaveButton.IsEnabled = true;
                        SaveButton.Content = "Save";
                        return;
                    }
                }

                Movie movie;

                if (_isEditMode && _existingMovie != null)
                {
                    var existingMovieFromDb = _movieService.GetMovieDetail(_existingMovie.MovieId);
                    if (existingMovieFromDb == null)
                    {
                        MessageBox.Show("Phim không tồn tại trong hệ thống.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    existingMovieFromDb.Title = TitleTextBox.Text.Trim();
                    existingMovieFromDb.Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text)
                        ? null
                        : DescriptionTextBox.Text.Trim();
                    existingMovieFromDb.Genre = (GenreComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
                    existingMovieFromDb.DirectorId = DirectorComboBox.SelectedValue as int?;

                    if (ReleaseDatePicker.SelectedDate.HasValue)
                    {
                        existingMovieFromDb.ReleaseDate = DateOnly.FromDateTime(ReleaseDatePicker.SelectedDate.Value);
                    }
                    else
                    {
                        existingMovieFromDb.ReleaseDate = null;
                    }

                    if (int.TryParse(DurationTextBox.Text, out int duration))
                    {
                        existingMovieFromDb.Duration = duration;
                    }
                    else
                    {
                        existingMovieFromDb.Duration = null;
                    }

                    existingMovieFromDb.PosterUrl = posterUrl;
                    existingMovieFromDb.TrailerUrl = trailerUrl;
                    existingMovieFromDb.Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Available";

                    _movieService.UpdateMovie(existingMovieFromDb);
                }
                else
                {
                    movie = new Movie
                    {
                        Title = TitleTextBox.Text.Trim(),
                        Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text)
                            ? null
                            : DescriptionTextBox.Text.Trim(),
                        Genre = (GenreComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "",
                        DirectorId = DirectorComboBox.SelectedValue as int?,
                        ReleaseDate = ReleaseDatePicker.SelectedDate.HasValue
                            ? DateOnly.FromDateTime(ReleaseDatePicker.SelectedDate.Value)
                            : null,
                        Duration = int.TryParse(DurationTextBox.Text, out int duration) ? duration : null,
                        PosterUrl = posterUrl,
                        TrailerUrl = trailerUrl,
                        Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Available",
                        CreatedBy = SessionContext.CurrentUserId,
                        CreatedAt = DateTime.Now
                    };

                    _movieService.AddMovie(movie);
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
                MessageBox.Show($"Lỗi khi lưu phim: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SaveButton.IsEnabled = true;
                SaveButton.Content = "Save";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

