using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace DroneNien
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _objectNames;
        private ObservableCollection<int> _objectCount;
        public DroneViewModel DroneVM { get; set; } 
        private FileSystemWatcher _fileWatcher;
        private string _filePath = "D:\\NCKH\\GitHubNien\\DronePath-main\\DronePath-main\\DroneNien\\source\\detect\\detection_counts.txt";

        public ObservableCollection<string> ObjectNames
        {
            get { return _objectNames; }
            set
            {
                _objectNames = value;
                OnPropertyChanged(nameof(ObjectNames));
            }
        }

        public ObservableCollection<int> ObjectCount
        {
            get { return _objectCount; }
            set
            {
                _objectCount = value;
                OnPropertyChanged(nameof(ObjectCount));
            }
        }

        public MainViewModel()
        {
            DroneVM = new DroneViewModel();
            ObjectNames = new ObservableCollection<string>();
            ObjectCount = new ObservableCollection<int>();
            UpdateObjectData();
            InitializeFileWatcher();
        }

        private void InitializeFileWatcher()
        {
            _fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(_filePath))
            {
                Filter = Path.GetFileName(_filePath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.EnableRaisingEvents = true;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Delay to ensure the file is fully written
            System.Threading.Thread.Sleep(100);
            UpdateObjectData();
        }

        private void UpdateObjectData()
        {
            try
            {
                if (!File.Exists(_filePath)) return;

                var lines = File.ReadAllLines(_filePath);
                var names = new ObservableCollection<string>();
                var counts = new ObservableCollection<int>();

                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        names.Add(parts[0].Trim());
                        if (int.TryParse(parts[1].Trim(), out int count))
                        {
                            counts.Add(count);
                        }
                    }
                }

                ObjectNames = names;
                ObjectCount = counts;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during file reading
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }



        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}