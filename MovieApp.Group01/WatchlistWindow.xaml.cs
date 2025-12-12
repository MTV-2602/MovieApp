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
            var inputWin = new InputNameWindow();
            inputWin.Owner = this; 

            if (inputWin.ShowDialog() == true)
            {
                string listName = inputWin.ResultName;

                _watchlistService.CreateWatchlist(_currentUserId, listName);

                LoadWatchlists();

                if (LbWatchlists.Items.Count > 0)
                    LbWatchlists.SelectedIndex = LbWatchlists.Items.Count - 1;
            }
        }

        private void DeleteList_Click(object sender, RoutedEventArgs e)
        {
            if (LbWatchlists.SelectedItem is not Watchlist selectedList)
            {
                MessageBox.Show("Please select a watchlist to delete.");
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete '{selectedList.Title}'?\nAll movies in this list will be removed from the watchlist.",
                                         "Confirm Delete",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _watchlistService.DeleteWatchlist(selectedList.WatchlistId);

                    MessageBox.Show("Watchlist deleted successfully.");

                    LoadWatchlists();

                    IcMovies.ItemsSource = null;
                    TxtSelectedListTitle.Text = "Select a list";
                    TxtCount.Text = "(0 items)";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting list: " + ex.Message);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}