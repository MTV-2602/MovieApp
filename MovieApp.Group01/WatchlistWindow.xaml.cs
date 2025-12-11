using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MovieApp.Group01
{
    /// <summary>
    /// Interaction logic for WatchlistWindow.xaml
    /// </summary>
    public partial class WatchlistWindow : Window
    {
        private readonly WatchlistService _watchlistService = new();
        private readonly WatchlistMovieService _watchlistMovieService = new();
        private int _currentUserId;

        public WatchlistWindow()
        {
            InitializeComponent();
            _currentUserId = SessionContext.CurrentUserId;

            if (_currentUserId == 0)
            {
                MessageBox.Show("Please login first.");
                this.Close();
                return;
            }

            LoadWatchlists();
        }

        private void LoadWatchlists()
        {
            var lists = _watchlistService.GetUserWatchlists(_currentUserId);
            LbWatchlists.ItemsSource = lists;

            if (lists.Count > 0)
                LbWatchlists.SelectedIndex = 0;
        }

        private void LbWatchlists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LbWatchlists.SelectedItem is Watchlist selectedList)
            {
                TxtSelectedListTitle.Text = selectedList.Title;
                LoadMovies(selectedList.WatchlistId);
            }
        }

        private void LoadMovies(int watchlistId)
        {
            var movies = _watchlistMovieService.GetMoviesInWatchlist(watchlistId);
            IcMovies.ItemsSource = movies;
            TxtCount.Text = $"({movies.Count} items)";
        }

        private void PlayMovie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int movieId)
            {
                var player = new MoviePlayerWindow(movieId);
                player.ShowDialog();
            }
        }

        private void RemoveMovie_Click(object sender, RoutedEventArgs e)
        {
            if (LbWatchlists.SelectedItem is Watchlist selectedList && sender is Button btn && btn.Tag is int movieId)
            {
                var result = MessageBox.Show("Remove this movie?", "Confirm", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _watchlistMovieService.RemoveFromWatchlist(selectedList.WatchlistId, movieId);
                    LoadMovies(selectedList.WatchlistId);
                }
            }
        }

        private void CreateList_Click(object sender, RoutedEventArgs e)
        {
            // Mở lại cái Dialog AddToWatchlist nhưng chỉ dùng để tạo mới
            // Hoặc đơn giản dùng InputDialog (nhưng WPF k có sẵn).
            // Ở đây tôi tạo nhanh 1 watchlist mặc định để demo
            _watchlistService.CreateWatchlist(_currentUserId, "New List " + System.DateTime.Now.Ticks);
            LoadWatchlists();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}