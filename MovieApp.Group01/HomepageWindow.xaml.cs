using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;


namespace MovieApp.Group01
{
    public partial class HomepageWindow : Window
    {
        private readonly MovieService _movieService = new();
        private readonly DirectorService _directorService = new();

        public HomepageWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadMovies();
                LoadDirectors();
                LoadGenres();
                LoadCountries();

                SearchBox.TextChanged += SearchBox_TextChanged;
            }
            catch (Exception)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===================================
        // MOVIES LIST
        // ===================================
        private void LoadMovies(List<Movie>? customList = null)
        {
            RecommendedGrid.Children.Clear();

            var movies = customList ?? _movieService.GetAllMovies();

            foreach (var m in movies)
                RecommendedGrid.Children.Add(CreateMovieCard(m));
        }

        private Border CreateMovieCard(Movie movie)
        {
            BitmapImage posterImg;

            if (!string.IsNullOrWhiteSpace(movie.PosterUrl))
            {
                try
                {
                    var uri = new Uri(movie.PosterUrl, UriKind.Absolute);
                    posterImg = new BitmapImage(uri);
                    posterImg.DownloadFailed += (s, e) =>
                    {
                        posterImg = new BitmapImage(new Uri("pack://application:,,,/assets/default-poster.png"));
                    };
                }
                catch
                {
                    posterImg = new BitmapImage(new Uri("pack://application:,,,/assets/default-poster.png"));
                }
            }
            else
            {
                posterImg = new BitmapImage(new Uri("pack://application:,,,/assets/default-poster.png"));
            }

            // ============================================================
            //  POSTER CONTAINER (bo góc luôn đúng)
            // ============================================================
            var posterContainer = new Grid
            {
                Width = 200,
                Height = 250,
                Clip = new RectangleGeometry(new Rect(0, 0, 180, 250), 16, 16)
            };

            var scaleTransform = new ScaleTransform(1, 1);

            var poster = new Border
            {
                Background = new ImageBrush(posterImg) { Stretch = Stretch.UniformToFill },
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = scaleTransform
            };

            posterContainer.Children.Add(poster);

            // ========== DETAIL PANEL (hover) ==========
            var detailPanel = new Border
            {
                Style = (Style)FindResource("HoverMovieDetailPanel"),
                Child = new StackPanel
                {
                    Children =
            {
                new TextBlock
                {
                    Text = movie.Title,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White
                },
                new TextBlock
                {
                    Text = movie.Director?.DirectorName ?? "",
                    FontSize = 13,
                    Foreground = Brushes.LightGray,
                    Margin = new Thickness(0,3,0,10)
                },
                new WrapPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = movie.ReleaseDate?.Year.ToString() ?? "",
                            Foreground = Brushes.White,
                            FontWeight = FontWeights.Bold,
                            Margin = new Thickness(0,0,10,0)
                        },
                        new TextBlock
                        {
                            Text = movie.Duration.HasValue ? $"{movie.Duration} phút" : "",
                            Foreground = Brushes.White,
                            Margin = new Thickness(0,0,10,0)
                        }
                    }
                },
                new TextBlock
                {
                    Text = movie.Genre,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0,8,0,8)
                }
            }
                }
            };

            // NÚT XEM NGAY
            var playButton = new Button
            {
                Content = "Xem ngay",
                Style = (Style)FindResource("PlayPrimaryButton"),
                Margin = new Thickness(0, 8, 0, 0)
            };

            playButton.Click += (s, e) =>
            {
                e.Handled = true;
                OpenTrailer(movie);
            };

            ((detailPanel.Child as StackPanel)!).Children.Add(playButton);

            // GRID chồng lớp
            var grid = new Grid();
            grid.Children.Add(posterContainer);
            grid.Children.Add(detailPanel);

            // CARD
            var card = new Border
            {
                Style = (Style)FindResource("MovieCard"),
                Width = 180,
                CornerRadius = new CornerRadius(20),
                ClipToBounds = true,
                Padding = new Thickness(0),
                Child = grid
            };

            // ============================================================
            // ANIMATION ZOOM MƯỢT
            // ============================================================
            DoubleAnimation zoomIn = new DoubleAnimation
            {
                To = 1.06,
                Duration = TimeSpan.FromMilliseconds(150),
                AccelerationRatio = 0.2,
                DecelerationRatio = 0.3
            };

            DoubleAnimation zoomOut = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(150),
                AccelerationRatio = 0.2,
                DecelerationRatio = 0.3
            };

            // HOVER
            card.MouseEnter += (s, e) =>
            {
                detailPanel.Tag = "Show";
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, zoomIn);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, zoomIn);
            };

            card.MouseLeave += (s, e) =>
            {
                detailPanel.Tag = null;
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, zoomOut);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, zoomOut);
            };

            card.MouseLeftButtonUp += (s, e) => OpenTrailer(movie);

            return card;
        }

        private Border CreateDirectorCard(Director director, int movieCount)
        {
            var border = new Border
            {
                Style = (Style)FindResource("DirectorCard"),
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 5, 10, 5)
            };

            border.MouseLeftButtonUp += (s, e) =>
            {
                e.Handled = true;
                OpenDirectorMoviesWindow(director);   // 👉 gọi window mới
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var avatarBorder = new Border
            {
                Width = 48,
                Height = 48,
                CornerRadius = new CornerRadius(12),
                Background = (Brush)FindResource("SecondaryBrush"),
                Margin = new Thickness(0, 0, 15, 0)
            };

            var avatarIcon = new TextBlock
            {
                Text = "🎬",
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            avatarBorder.Child = avatarIcon;
            Grid.SetColumn(avatarBorder, 0);

            var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            stack.Children.Add(new TextBlock
            {
                Text = director.DirectorName,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            });

            stack.Children.Add(new TextBlock
            {
                Text = $"{movieCount} Movies",
                FontSize = 14,
                Foreground = Brushes.Gray
            });

            Grid.SetColumn(stack, 1);

            grid.Children.Add(avatarBorder);
            grid.Children.Add(stack);

            border.Child = grid;
            return border;
        }

        private void OpenDirectorMoviesWindow(Director director)
        {
            // Lấy toàn bộ phim của đạo diễn
            var movies = _movieService
                .GetAllMovies()
                .Where(m => m.DirectorId == director.DirectorId)
                .ToList();

            var win = new DirectorMoviesWindow(director, movies)
            {
                Owner = this
            };

            win.Show();
        }

        private void OpenTrailer(Movie movie)
        {

            if (movie.MovieId <= 0) return;

            var playerWindow = new MoviePlayerWindow(movie.MovieId);

            this.Hide();

            playerWindow.Closed += (s, args) => this.Show();
            playerWindow.Show();
        }


        // ===================================
        // SEARCH
        // ===================================
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = SearchBox.Text.Trim();

            if (keyword.Length == 0)
            {
                LoadMovies();
                return;
            }

            LoadMovies(_movieService.SearchMovies(keyword));
        }

        // ===================================
        // FILTER
        // ===================================
        private void GenreFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CountryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var movies = _movieService.GetAllMovies();

            string genre = GenreFilter.SelectedItem?.ToString() ?? "";
            string country = CountryFilter.SelectedItem?.ToString() ?? "";

            if (genre != "All Genres" && genre != "")
                movies = movies.Where(m => m.Genre == genre).ToList();

            if (country != "All Countries" && country != "")
                movies = movies.Where(m => m.Director?.Country == country).ToList();

            LoadMovies(movies);
        }

        // ===================================
        // LOAD GENRES FROM DB
        // ===================================
        private void LoadGenres()
        {
            var genres = _movieService.GetAllMovies()
                                      .Select(m => m.Genre)
                                      .Distinct()
                                      .OrderBy(g => g)
                                      .ToList();

            genres.Insert(0, "All Genres");

            GenreFilter.ItemsSource = genres;
            GenreFilter.SelectedIndex = 0;
        }

        // ===================================
        // LOAD COUNTRIES FROM DB
        // ===================================
        private void LoadCountries()
        {
            var countries = _directorService.GetAllDirectors()
                                            .Where(d => !string.IsNullOrWhiteSpace(d.Country))
                                            .Select(d => d.Country!)
                                            .Distinct()
                                            .OrderBy(c => c)
                                            .ToList();

            countries.Insert(0, "All Countries");

            CountryFilter.ItemsSource = countries;
            CountryFilter.SelectedIndex = 0;
        }

        // ===================================
        // DIRECTORS LIST
        // ===================================
        private void LoadDirectors()
        {
            ArtistPanel.Children.Clear();

            var directors = _directorService.GetAllDirectors();

            foreach (var d in directors)
            {
                int movieCount = d.Movies?.Count ?? 0;
                var card = CreateDirectorCard(d, movieCount);

                ArtistPanel.Children.Add(card);
            }
        }


        // ===================================
        // PROFILE & LOGOUT
        // ===================================
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionContext.CurrentUserId == 0)
            {
                MessageBox.Show("Please login first!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var updateProfileWindow = new UpdateProfileWindow(SessionContext.CurrentUserId);
            updateProfileWindow.ShowDialog();
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
                // Clear session
                SessionContext.Clear();

                // Show login window
                var loginWindow = new LoginWindow();
                loginWindow.Show();

                // Close current window
                this.Close();
            }
        }

        private void WatchlistButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionContext.CurrentUserId == 0)
            {
                MessageBox.Show("Please login first!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var watchlistWin = new WatchlistWindow();
            this.Hide();
            watchlistWin.Closed += (s, args) => this.Show();
            watchlistWin.Show();
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionContext.CurrentUserId == 0)
            {
                MessageBox.Show("Please login first!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var historyWin = new HistoryWindow();
            historyWin.Show();
            this.Close();
        }
    }
}
