using Microsoft.Web.WebView2.Core;
using MovieApp.BLL.Services;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MovieApp.Group01
{
    public partial class MoviePlayerWindow : Window
    {
        private readonly MovieService _movieService = new();
        private readonly WatchingHistoryService _historyService = new();
        private readonly WatchlistMovieService _watchlistService = new();
        private readonly RatingService _ratingService = new();
        private readonly CommentService _commentService = new();

        private int _movieId;
        private bool _isDragging = false;
        private bool _isPlaying = false;
        private double _totalDuration = 0;

        // Fullscreen variables
        private bool _isFullScreen = false;
        private WindowStyle _previousWindowStyle;
        private WindowState _previousWindowState;
        private ResizeMode _previousResizeMode;

        public MoviePlayerWindow(int movieId)
        {
            InitializeComponent();

            WebPlayer.NavigationCompleted += async (s, e) =>
            {
                if (_isPlaying)
                {
                    await WebPlayer.ExecuteScriptAsync("play()");
                }
            };

            _movieId = movieId;

            // Set default background color to avoid white flash
            WebPlayer.DefaultBackgroundColor = System.Drawing.Color.Black;

            // Load data when window loads
            this.Loaded += async (s, e) =>
            {
                if (_movieId > 0)
                {
                    // 1. Initialize WebView Environment
                    await WebPlayer.EnsureCoreWebView2Async();

                    // 2. Listen for messages from JavaScript (Time updates)
                    WebPlayer.WebMessageReceived += WebPlayer_WebMessageReceived;

                    // 3. Load Movie Content
                    LoadMovieData();
                }
            };

            this.KeyDown += Window_KeyDown;
        }

        private void LoadMovieData()
        {
            var movie = _movieService.GetMovieDetail(_movieId);
            if (movie == null) { MessageBox.Show("Movie not found!"); Close(); return; }

            // Set UI Text
            TxtTitle.Text = movie.Title;
            TxtDescription.Text = movie.Description;
            TxtGenre.Text = movie.Genre;
            TxtYear.Text = movie.ReleaseDate?.Year.ToString() ?? "N/A";
            TxtDirector.Text = movie.Director?.DirectorName ?? "Unknown";

            try { if (!string.IsNullOrEmpty(movie.PosterUrl)) ImgPoster.Source = new BitmapImage(new Uri(movie.PosterUrl, UriKind.RelativeOrAbsolute)); } catch { }

            if (!string.IsNullOrWhiteSpace(movie.TrailerUrl))
            {
                PlayVideoFromSupabase(movie.TrailerUrl);
            }

            // Load rating and comments
            LoadRatingAndComments();
        }

        private void PlayVideoFromSupabase(string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                MessageBox.Show("Video URL is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        html, body {{
            width: 100%;
            height: 100%;
            overflow: hidden;
            background-color: #000;
        }}
        #player {{
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            display: flex;
            align-items: center;
            justify-content: center;
        }}
        video {{
            width: 100%;
            height: 100%;
            object-fit: contain;
        }}
    </style>
</head>
<body>
    <div id='player'>
        <video id='videoPlayer' autoplay>
            <source src='{videoUrl}' type='video/mp4'>
            Your browser does not support the video tag.
        </video>
    </div>
    <script>
        var video = document.getElementById('videoPlayer');
        
        video.addEventListener('loadedmetadata', function() {{
            window.chrome.webview.postMessage(JSON.stringify({{ type: 'duration', value: video.duration }}));
        }});
        
        video.addEventListener('timeupdate', function() {{
            window.chrome.webview.postMessage(JSON.stringify({{ type: 'time', value: video.currentTime }}));
        }});
        
        video.addEventListener('play', function() {{
            window.chrome.webview.postMessage(JSON.stringify({{ type: 'play' }}));
        }});
        
        video.addEventListener('pause', function() {{
            window.chrome.webview.postMessage(JSON.stringify({{ type: 'pause' }}));
        }});
        
        video.addEventListener('ended', function() {{
            window.chrome.webview.postMessage(JSON.stringify({{ type: 'ended' }}));
        }});
        
        video.addEventListener('click', function() {{
            window.chrome.webview.postMessage(JSON.stringify({{ type: 'toggle' }}));
        }});
        
        function play() {{ video.play(); }}
        function pause() {{ video.pause(); }}
        function seek(t) {{ video.currentTime = t; }}
        function vol(val) {{ video.volume = val; }}
    </script>
</body>
</html>";

            WebPlayer.NavigateToString(html);
            _isPlaying = true;
            UpdatePlayButtonState();
        }

        // ==========================================
        // RATING & COMMENT FEATURES
        // ==========================================

        private void LoadRatingAndComments()
        {
            // Load user's rating
            if (SessionContext.CurrentUserId != 0)
            {
                var userRating = _ratingService.GetUserRating(SessionContext.CurrentUserId, _movieId);
                if (userRating != null)
                {
                    UpdateStarDisplay(userRating.Score);
                }
            }

            // Load average rating
            var avgRating = _ratingService.GetAverageRating(_movieId);
            var ratingCount = _ratingService.GetRatingCount(_movieId);

            if (ratingCount > 0)
            {
                TxtRatingInfo.Text = $"Average: {avgRating:F1}/5 ({ratingCount} ratings)";
            }
            else
            {
                TxtRatingInfo.Text = "No ratings yet";
            }

            // Load comment count
            UpdateCommentCount();
        }

        private void UpdateCommentCount()
        {
            var totalComments = _commentService.GetCommentCount(_movieId);
            TxtCommentCount.Text = $"{totalComments} comment{(totalComments != 1 ? "s" : "")}";
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            if (SessionContext.CurrentUserId == 0)
            {
                MessageBox.Show("Please login to rate this movie.", "Login Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = sender as Button;
            if (button != null && int.TryParse(button.Tag.ToString(), out int score))
            {
                try
                {
                    _ratingService.AddOrUpdateRating(SessionContext.CurrentUserId, _movieId, score);
                    UpdateStarDisplay(score);

                    // Reload rating info
                    var avgRating = _ratingService.GetAverageRating(_movieId);
                    var ratingCount = _ratingService.GetRatingCount(_movieId);
                    TxtRatingInfo.Text = $"Average: {avgRating:F1}/5 ({ratingCount} ratings)";

                    MessageBox.Show($"You rated this movie {score} stars!", "Rating Submitted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error submitting rating: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UpdateStarDisplay(int score)
        {
            var stars = new[] { Star1, Star2, Star3, Star4, Star5 };

            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].Content = i < score ? "★" : "☆";
            }
        }

        private void OpenComments_Click(object sender, RoutedEventArgs e)
        {
            var movie = _movieService.GetMovieDetail(_movieId);
            if (movie == null) return;

            var commentsWindow = new CommentsWindow(_movieId, movie.Title);
            commentsWindow.Owner = this;
            commentsWindow.ShowDialog();

            // Refresh comment count after closing popup
            UpdateCommentCount();
        }

        // ==========================================
        // COMMUNICATION: JAVASCRIPT -> C# (Update UI)
        // ==========================================
        private void WebPlayer_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var json = e.TryGetWebMessageAsString();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                string type = root.GetProperty("type").GetString();

                if (type == "duration")
                {
                    _totalDuration = root.GetProperty("value").GetDouble();
                    ProgressSlider.Maximum = _totalDuration;
                    TxtTime.Text = $"00:00 / {TimeSpan.FromSeconds(_totalDuration):mm\\:ss}";
                }
                else if (type == "time")
                {
                    // Only update slider if user is NOT dragging it
                    if (!_isDragging)
                    {
                        double current = root.GetProperty("value").GetDouble();
                        ProgressSlider.Value = current;
                        TxtTime.Text = $"{TimeSpan.FromSeconds(current):mm\\:ss} / {TimeSpan.FromSeconds(_totalDuration):mm\\:ss}";
                    }
                }
                else if (type == "ended")
                {
                    _isPlaying = false;
                    UpdatePlayButtonState();
                    ProgressSlider.Value = 0;
                }
                else if (type == "toggle")
                {
                    TogglePlay();
                }
            }
            catch { }
        }

        // ==========================================
        // COMMUNICATION: C# -> WEB (Control Video)
        // ==========================================

        private async void TogglePlay()
        {
            if (_isPlaying)
            {
                await WebPlayer.ExecuteScriptAsync("pause()");
                _isPlaying = false;
            }
            else
            {
                await WebPlayer.ExecuteScriptAsync("play()");
                _isPlaying = true;
            }
            UpdatePlayButtonState();
        }

        private void TogglePlay_Click(object sender, RoutedEventArgs e) => TogglePlay();
        private void TogglePlay_Click(object sender, MouseButtonEventArgs e) => TogglePlay();

        private void UpdatePlayButtonState()
        {
            BtnPlayPause.Content = _isPlaying ? "⏸" : "▶";
            OverlayPlay.Visibility = _isPlaying ? Visibility.Hidden : Visibility.Visible;
        }

        // Seek Video
        private void ProgressSlider_DragStarted(object sender, DragStartedEventArgs e) { _isDragging = true; }

        private async void ProgressSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isDragging = false;
            await WebPlayer.ExecuteScriptAsync($"seek({ProgressSlider.Value})");
        }

        // Change Volume
        private async void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (WebPlayer != null && WebPlayer.CoreWebView2 != null)
                await WebPlayer.ExecuteScriptAsync($"vol({VolumeSlider.Value})");
        }

        // ==========================================
        // OTHER FEATURES
        // ==========================================

        private void BackButton_Click(object sender, RoutedEventArgs e) => Close();

        private void AddToWatchlist_Click(object sender, RoutedEventArgs e)
        {
            if (SessionContext.CurrentUserId == 0)
            {
                MessageBox.Show("Please login to use this feature.", "Login Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new AddToWatchlistWindow(SessionContext.CurrentUserId, _movieId);

            dialog.Owner = this;

            dialog.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save History
            if (SessionContext.CurrentUserId != 0 && ProgressSlider.Value > 5)
            {
                _historyService.SaveHistory(SessionContext.CurrentUserId, _movieId, (int)ProgressSlider.Value);
            }
            WebPlayer.Dispose();
        }

        private void BtnFullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (!_isFullScreen)
            {
                _previousWindowStyle = WindowStyle; _previousWindowState = WindowState; _previousResizeMode = ResizeMode;
                RowHeader.Height = new GridLength(0);
                RowInfo.Height = new GridLength(0);
                WindowStyle = WindowStyle.None; ResizeMode = ResizeMode.NoResize; WindowState = WindowState.Maximized;
                BtnFullscreen.Content = "✖"; _isFullScreen = true;
            }
            else
            {
                WindowStyle = _previousWindowStyle; ResizeMode = _previousResizeMode; WindowState = _previousWindowState;
                RowHeader.Height = GridLength.Auto;
                RowInfo.Height = GridLength.Auto;
                BtnFullscreen.Content = "⛶"; _isFullScreen = false;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _isFullScreen) BtnFullscreen_Click(this, null);
        }
    }
}