using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.Configuration;
using magic_portal_class_library; 

namespace magic_portal
{
    public partial class MainWindow : Window
    {
        private readonly BlobStorageService _blobs;
        private readonly string _downloadDir;
        private readonly string _containerName;

        private readonly ObservableCollection<string> _blobItems = new();

        // UI controls
        private ListBox _listBox = null!;
        private TextBlock _statusText = null!;

        public MainWindow()
        {
            InitializeComponent();

            // ---- Load config ----
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.local.json", optional: true) // ignores if missing
                .Build();

            var accountUrl = config["AzureBlob:AccountUrl"]!;
            var sasToken = config["AzureBlob:SasToken"]!;
            _containerName = config["AzureBlob:ContainerName"]!;

            _blobs = new BlobStorageService(accountUrl, sasToken, _containerName);

            // ---- Download dir ----
            _downloadDir = OperatingSystem.IsWindows()
                ? @"C:\Magic Portal"
                : System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "magic-portal"
                );

            System.IO.Directory.CreateDirectory(_downloadDir);

            // ---- Build UI in code ----
            BuildUi();

            // Load blobs on startup
            _ = RefreshListAsync();
        }

        private void BuildUi()
        {
            Title = "Magic Portal";
            Width = 720;
            Height = 520;
            Background = Brushes.White;

            var root = new Grid
            {
                Margin = new Thickness(16)
            };

            // rows: header, subheader, list, buttons, status
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // --- Header ---
            var header = new TextBlock
            {
                Text = "Magic Portal — Azure Blob Manager",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 6)
            };
            Grid.SetRow(header, 0);
            root.Children.Add(header);

            // --- Subheader ---
            var subheader = new TextBlock
            {
                Text = $"Available files in '{_containerName}' container:",
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(subheader, 1);
            root.Children.Add(subheader);

            // --- ListBox ---
            _listBox = new ListBox
            {
                ItemsSource = _blobItems,
                FontSize = 12,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 12)
            };
            Grid.SetRow(_listBox, 2);
            root.Children.Add(_listBox);

            // --- Buttons row ---
            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var refreshBtn = new Button
            {
                Content = "Refresh List",
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 8, 0)
            };
            refreshBtn.Click += async (_, __) => await RefreshListAsync();

            var downloadBtn = new Button
            {
                Content = "Download Selected",
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 8, 0)
            };
            downloadBtn.Click += async (_, __) => await DownloadSelectedAsync();

            var uploadBtn = new Button
            {
                Content = "Upload File…",
                Padding = new Thickness(12, 6, 12, 6)
            };
            uploadBtn.Click += async (_, __) => await UploadFileAsync();

            buttonRow.Children.Add(refreshBtn);
            buttonRow.Children.Add(downloadBtn);
            buttonRow.Children.Add(uploadBtn);

            Grid.SetRow(buttonRow, 3);
            root.Children.Add(buttonRow);

            // --- Status text ---
            _statusText = new TextBlock
            {
                FontSize = 12,
                Foreground = Brushes.DimGray
            };
            Grid.SetRow(_statusText, 4);
            root.Children.Add(_statusText);

            Content = root;
        }

        private async Task RefreshListAsync()
        {
            try
            {
                SetStatus("Loading blob list...");
                _blobItems.Clear();

                var blobs = await _blobs.ListBlobsAsync();
                foreach (var b in blobs)
                    _blobItems.Add(b);

                SetStatus($"Loaded blob list from '{_containerName}'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load blob list:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                SetStatus("");
            }
        }

        private async Task UploadFileAsync()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select file to upload"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var filePath = dialog.FileName;
                SetStatus($"Uploading '{System.IO.Path.GetFileName(filePath)}'...");
                await _blobs.UploadFileAsync(filePath);
                SetStatus("Upload successful.");
                await RefreshListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to upload:\n{ex.Message}", "Upload Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                SetStatus("");
            }
        }

        private async Task DownloadSelectedAsync()
        {
            if (_listBox.SelectedItem is not string blobName)
            {
                MessageBox.Show("Please select a file to download.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var downloadPath = System.IO.Path.Combine(_downloadDir, blobName);

            try
            {
                SetStatus($"Downloading '{blobName}'...");
                await _blobs.DownloadFileAsync(blobName, downloadPath);
                SetStatus($"Downloaded to {downloadPath}");

                OpenFolder(_downloadDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download:\n{ex.Message}", "Download Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                SetStatus("");
            }
        }

        private void SetStatus(string msg) => _statusText.Text = msg;

        private static void OpenFolder(string path)
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", path)
                    {
                        UseShellExecute = true
                    });
                }
                else if (OperatingSystem.IsLinux())
                {
                    Process.Start("xdg-open", path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Downloaded successfully, but couldn’t open the folder automatically.\n\nPath: {path}\n\n{ex.Message}",
                    "Open Folder Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
    }
}
