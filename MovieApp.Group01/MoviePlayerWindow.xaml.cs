using Microsoft.Web.WebView2.Core;
using MovieApp.BLL.Services;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;

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
                string videoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, movie.TrailerUrl);

                if (!File.Exists(videoPath))
                {
                    string projectPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\", movie.TrailerUrl));
                    if (File.Exists(projectPath)) videoPath = projectPath;
                }

                if (File.Exists(videoPath))
                {
                    PlayVideoInWebView(videoPath);
                }
                else
                {
                    MessageBox.Show($"❌ Video file not found: {movie.TrailerUrl}");
                }
            }

            // Load rating and comments
            LoadRatingAndComments();
        }

        private void PlayVideoInWebView(string fullPath)
        {
            string folder = Path.GetDirectoryName(fullPath);
            string fileName = Path.GetFileName(fullPath);

            WebPlayer.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "local.videos",
                folder,
                CoreWebView2HostResourceAccessKind.Allow);

            string html = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <style>
            body, html {{ 
                margin: 0; 
                padding: 0; 
                width: 100%; 
                height: 100%; 
                background-color: black; 
                overflow: hidden; 
            }}
            #vid {{ 
                width: 100vw;       /* Chiếm 100% chiều rộng view */ 
                height: 100vh;      /* Chiếm 100% chiều cao view */ 
                object-fit: contain; /* ⭐ QUAN TRỌNG: Giữ nguyên tỷ lệ video, không bao giờ bị cắt */ 
                pointer-events: none; /* Click xuyên qua video để WPF bắt sự kiện */ 
            }}
        </style>
    </head>
    <body>
        <video id='vid' autoplay>
            <source src='https://local.videos/{fileName}' type='video/mp4'>
        </video>

        <script>
            const v = document.getElementById('vid');
            
            // --- XỬ LÝ AUTOPLAY MẠNH MẺ ---
            // Cố gắng play ngay khi nạp
            window.onload = function() {{
                forcePlay();
            }};

            function forcePlay() {{
                v.play().then(() => {{
                    // Play thành công
                }}).catch((e) => {{
                    // Nếu lỗi (do browser chặn), thử mute rồi play lại
                    // v.muted = true; 
                    // v.play();
                    // setTimeout(() => {{ v.muted = false; }}, 1000); // Mở lại tiếng sau 1s
                }});
            }}

            // Gửi metadata về C#
            v.onloadedmetadata = () => {{
                window.chrome.webview.postMessage(JSON.stringify({{ type: 'duration', value: v.duration }}));
            }};

            v.onclick = () => {{
            window.chrome.webview.postMessage(JSON.stringify({{ type: 'toggle' }}));
            }};

            // Update tiến độ
            v.ontimeupdate = () => {{
                window.chrome.webview.postMessage(JSON.stringify({{ type: 'time', value: v.currentTime }}));
            }};

            // Kết thúc
            v.onended = () => {{
                window.chrome.webview.postMessage(JSON.stringify({{ type: 'ended' }}));
            }};

            // API cho C# gọi
            function play() {{ v.play(); }}
            function pause() {{ v.pause(); }}
            function seek(t) {{ v.currentTime = t; }}
            function vol(val) {{ v.volume = val; }}
        </script>
    </body>
    </html>";

            WebPlayer.NavigateToString(html);

            // Đặt trạng thái WPF là đang Play để nút hiển thị đúng icon Pause
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