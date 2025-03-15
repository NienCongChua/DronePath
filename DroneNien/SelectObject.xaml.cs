using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace DroneNien
{
    /// <summary>
    /// Interaction logic for SelectObject.xaml
    /// </summary>
    public partial class SelectObject : Window
    {
        private List<SelectableObject> objects;
        public SelectObject()
        {
            InitializeComponent();
            objects = new List<SelectableObject>();
            LoadObjects();
            ObjectsList.ItemsSource = objects;
        }
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            var selectedObjects = objects.Where(obj => obj.IsChecked).Select(obj => TranslateToEnglish(obj.Name)).ToList();
            string filePath = "C:\\Users\\NienNguyen\\Desktop\\DronePath\\DroneNien\\source\\detect\\phathien.txt";
            File.WriteAllLines(filePath, selectedObjects);
            this.Close();
        }

        private string TranslateToEnglish(string vietnameseName)
        {
            return vietnameseName switch
            {
                "Người" => "human",
                "Xe ôtô" => "car",
                "Xe máy" => "motorbike",
                "Container" => "container",
                "Ghế dài" => "bench",
                "Nhà" => "house",
                "Cái ô" => "umbrella",
                _ => vietnameseName
            };
        }

        private void LoadObjects()
        {
            objects = new List<SelectableObject>
            {
                new SelectableObject { Name = "Người" },
                new SelectableObject { Name = "Xe ôtô" },
                new SelectableObject { Name = "Xe máy" },
                new SelectableObject { Name = "Container" },
                new SelectableObject { Name = "Ghế dài" },
                new SelectableObject { Name = "Nhà" },
                new SelectableObject { Name = "Cái ô" }
            };
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = RemoveDiacritics(SearchBox.Text.ToLower());
            ObjectsList.ItemsSource = objects.Where(obj => RemoveDiacritics(obj.Name.ToLower()).Contains(searchText)).ToList();
            // Hiển thị hoặc ẩn nút xóa
            ClearButton.Visibility = string.IsNullOrWhiteSpace(SearchBox.Text) ? Visibility.Collapsed : Visibility.Visible;
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

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            ClearButton.Visibility = Visibility.Collapsed; // Ẩn nút khi không có nội dung
        }
    }
    public class SelectableObject
    {
        public required string Name { get; set; }
        public bool IsChecked { get; set; }
    }
}
