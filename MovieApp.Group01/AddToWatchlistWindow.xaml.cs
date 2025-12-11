using MovieApp.BLL.Services;
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
    /// Interaction logic for AddToWatchlistWindow.xaml
    /// </summary>
    public partial class AddToWatchlistWindow : Window
    {
        private readonly WatchlistService _watchlistService = new();
        private readonly WatchlistMovieService _watchlistMovieService = new();

        private int _userId;
        private int _movieId;

        public AddToWatchlistWindow(int userId, int movieId)
        {
            InitializeComponent();
            _userId = userId;
            _movieId = movieId;
            LoadWatchlists();
        }

        private void LoadWatchlists()
        {
            var list = _watchlistService.GetUserWatchlists(_userId);
            CbWatchlists.ItemsSource = list;

            if (list.Count > 0)
                CbWatchlists.SelectedIndex = 0;
            else
            {
                RbNew.IsChecked = true;
                RbExisting.IsEnabled = false;
            }
        }

        private void Mode_Checked(object sender, RoutedEventArgs e)
        {
            if (CbWatchlists == null || TxtNewWatchlist == null) return;

            bool isNew = RbNew.IsChecked == true;
            CbWatchlists.IsEnabled = !isNew;
            TxtNewWatchlist.IsEnabled = isNew;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            int targetWatchlistId = 0;

            if (RbNew.IsChecked == true)
            {
                string name = TxtNewWatchlist.Text.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Please enter a watchlist name.");
                    return;
                }
                var newWl = _watchlistService.CreateWatchlist(_userId, name);
                targetWatchlistId = newWl.WatchlistId;
            }
            else
            {
                if (CbWatchlists.SelectedValue == null)
                {
                    MessageBox.Show("Please select a watchlist.");
                    return;
                }
                targetWatchlistId = (int)CbWatchlists.SelectedValue;
            }

            _watchlistMovieService.AddToWatchlist(targetWatchlistId, _movieId);

            MessageBox.Show("Movie added successfully!", "Success");
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
