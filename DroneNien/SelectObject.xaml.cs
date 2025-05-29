using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.IO;
namespace DroneNien
{
    /// <summary>
    /// Interaction logic for SelectObject.xaml
    /// </summary>
    public partial class SelectObject : Window
    {
        private List<SelectableObject> objects;
        private LoadPython loadPython;

        public SelectObject()
        {
            InitializeComponent();

            loadPython = new LoadPython();

            objects = new List<SelectableObject>();
            LoadObjects();
            ObjectsList.ItemsSource = objects;
        }
        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            var selectedObjects = objects.Where(obj => obj.IsChecked).Select(obj => TranslateToEnglish(obj.Name)).ToList();
            string filePath = "D:/NCKH/GitHubNien/AirsimYolo/AirsimYolo/phathien.txt";
            File.WriteAllLines(filePath, selectedObjects);
            updateFile();
            this.Close();
        }

        private void updateFile()
        {
            loadPython.ProcessPythonFile(@"D:\NCKH\GitHubNien\AirsimYolo\AirsimYolo\updateFile.py");
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
                "Vạch kẻ đường" => "crosswalk",
                "Đèn giao thông" => "traffic lights",
                _ => vietnameseName
            };
        }

        private void LoadObjects()
        {
            objects = new List<SelectableObject>
            {
                new SelectableObject { Name = "Human" },
                new SelectableObject { Name = "Car" },
                new SelectableObject { Name = "Motorbike" },
                new SelectableObject { Name = "Container" },
                new SelectableObject { Name = "Bench" },
                new SelectableObject { Name = "House" },
                new SelectableObject { Name = "Umbrella" },
                new SelectableObject { Name = "Crosswalk" }
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

