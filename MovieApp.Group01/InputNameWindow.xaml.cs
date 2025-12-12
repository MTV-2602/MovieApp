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
    /// Interaction logic for InputNameWindow.xaml
    /// </summary>
    public partial class InputNameWindow : Window
    {
        public string ResultName { get; private set; } = string.Empty;

        public InputNameWindow()
        {
            InitializeComponent();
            TxtInput.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtInput.Text))
            {
                MessageBox.Show("Name cannot be empty.");
                return;
            }
            ResultName = TxtInput.Text.Trim();
            this.DialogResult = true; 
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
