using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DroneNien
{
    public partial class SelectObject : Page
    {
        private List<SelectableObject> objects;

        public SelectObject()
        {
            InitializeComponent();
            LoadObjects();
            ObjectsList.ItemsSource = objects;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoadObjects()
        {
            // Example list of objects
            objects = new List<SelectableObject>
                {
                    new SelectableObject { Name = "Người" },
                    new SelectableObject { Name = "Xe tăng" },
                    new SelectableObject { Name = "Máy bay" },
                    new SelectableObject { Name = "Ôtô" },
                    new SelectableObject { Name = "Nhà" },
                    new SelectableObject { Name = "Tên lửa" },
                    new SelectableObject { Name = "Nhà vệ sinh" },
                    new SelectableObject { Name = "Xe máy" },
                    new SelectableObject { Name = "Toilet" },
                    new SelectableObject { Name = "Xe tải" },
                    new SelectableObject { Name = "Cây" },
                    new SelectableObject { Name = "Người" },
                    new SelectableObject { Name = "Xe tăng" },
                    new SelectableObject { Name = "Máy bay" },
                    new SelectableObject { Name = "Ôtô" },
                    new SelectableObject { Name = "Nhà" },
                    new SelectableObject { Name = "Tên lửa" },
                    new SelectableObject { Name = "Nhà vệ sinh" },
                    new SelectableObject { Name = "Xe máy" },
                    new SelectableObject { Name = "Toilet" },
                    new SelectableObject { Name = "Xe tải" },
                    new SelectableObject { Name = "Cây" }
                };
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = RemoveDiacritics(SearchBox.Text.ToLower());
            ObjectsList.ItemsSource = objects.Where(obj => RemoveDiacritics(obj.Name.ToLower()).Contains(searchText)).ToList();
        }

        private void ConfirmSelection_Click(object sender, RoutedEventArgs e)
        {
            var selectedObjects = objects.Where(obj => obj.IsChecked).Select(obj => obj.Name).ToList();
            if (selectedObjects.Any())
            {
                MessageBox.Show("Selected objects: " + string.Join(", ", selectedObjects));
            }
            else
            {
                MessageBox.Show("No objects selected.");
            }
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }

    public class SelectableObject
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }
}

