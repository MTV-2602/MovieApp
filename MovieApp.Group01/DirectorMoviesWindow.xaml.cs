using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MovieApp.Group01
{
    public partial class DirectorMoviesWindow : Window
    {
        private readonly Director _director;
        private readonly List<Movie> _movies;

        public DirectorMoviesWindow(Director director, List<Movie> movies)
        {
            InitializeComponent();
            _director = director;
            _movies = movies ?? new List<Movie>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DirectorNameText.Text = _director.DirectorName;
            MovieCountText.Text = $"{_movies.Count} Movies";

            LoadMovies();
        }

        private void LoadMovies()
        {
            MoviesPanel.Children.Clear();

            if (_movies.Count == 0)
            {
                MoviesPanel.Children.Add(new TextBlock
                {
                    Text = "This director has no movies.",
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(10)
                });
                return;
            }

            foreach (var m in _movies)
            {
                MoviesPanel.Children.Add(CreateMovieCard(m));
            }
        }

        // ==== COPY STYLE TỪ HOMEPAGE ====
        private Border CreateMovieCard(Movie movie)
        {
            // Load poster
            string fullPoster = System.IO.Path.GetFullPath(movie.PosterUrl ?? "");
            BitmapImage posterImg;

            try
            {
                posterImg = new BitmapImage(new Uri(fullPoster, UriKind.Absolute));
            }
            catch
            {
                posterImg = new BitmapImage(new Uri("pack://application:,,,/assets/default-poster.png"));
            }

            // Poster container (bo góc)
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

            // Detail panel
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

            // Nút xem ngay
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

            // Grid chồng lớp
            var grid = new Grid();
            grid.Children.Add(posterContainer);
            grid.Children.Add(detailPanel);

            // Card ngoài
            var card = new Border
            {
                Style = (Style)FindResource("MovieCard"),
                Width = 180,
                CornerRadius = new CornerRadius(20),
                ClipToBounds = true,
                Padding = new Thickness(0),
                Child = grid
            };

            // Animation zoom
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

        private void OpenTrailer(Movie movie)
        {

            if (movie.MovieId <= 0) return;

            var playerWindow = new MoviePlayerWindow(movie.MovieId);

            this.Hide();

            playerWindow.Closed += (s, args) => this.Show();
            playerWindow.Show();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
