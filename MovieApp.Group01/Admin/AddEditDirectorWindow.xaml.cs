using MovieApp.BLL.Services;
using MovieApp.DAL.Entities;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MovieApp.Group01
{
    public partial class AddEditDirectorWindow : Window
    {
        private readonly DirectorService _directorService = new();
        private readonly Director? _existingDirector;
        private bool IsEditMode => _existingDirector != null;

        public AddEditDirectorWindow()
        {
            InitializeComponent();
            Title = "Add Director";
        }

        public AddEditDirectorWindow(Director director) : this()
        {
            _existingDirector = director;
            Title = "Edit Director";
            LoadDirectorData();
        }

        private void LoadDirectorData()
        {
            if (_existingDirector == null) return;

            NameTextBox.Text = _existingDirector.DirectorName;
            CountryTextBox.Text = _existingDirector.Country ?? "";
            BioTextBox.Text = _existingDirector.Bio ?? "";

            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (item.Content.ToString() == _existingDirector.Status)
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateName();
        }

        private bool ValidateName()
        {
            bool isValid = !string.IsNullOrWhiteSpace(NameTextBox.Text);
            NameError.Visibility = isValid ? Visibility.Collapsed : Visibility.Visible;
            return isValid;
        }

        private bool ValidateForm()
        {
            bool nameValid = ValidateName();

            if (!nameValid)
            {
                MessageBox.Show("Vui lòng điền đầy đủ các trường bắt buộc (*).",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionContext.CurrentRole != 1)
            {
                MessageBox.Show("Chỉ Admin mới được thêm/sửa đạo diễn.",
                    "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateForm())
                return;

            try
            {
                if (IsEditMode && _existingDirector != null)
                {
                    var dir = _directorService.GetDirectorById(_existingDirector.DirectorId);
                    if (dir == null)
                    {
                        MessageBox.Show("Đạo diễn không tồn tại.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    dir.DirectorName = NameTextBox.Text.Trim();
                    dir.Country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text.Trim();
                    dir.Bio = string.IsNullOrWhiteSpace(BioTextBox.Text) ? null : BioTextBox.Text.Trim();
                    dir.Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Active";

                    _directorService.UpdateDirector(dir);
                }
                else
                {
                    var dir = new Director
                    {
                        DirectorName = NameTextBox.Text.Trim(),
                        Country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text.Trim(),
                        Bio = string.IsNullOrWhiteSpace(BioTextBox.Text) ? null : BioTextBox.Text.Trim(),
                        Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Active"
                    };

                    _directorService.AddDirector(dir);
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
                MessageBox.Show($"Lỗi khi lưu đạo diễn: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

