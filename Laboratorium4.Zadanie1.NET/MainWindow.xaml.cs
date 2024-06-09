using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Laboratorium4.Zadanie1.NET;

public sealed partial class MainWindow : INotifyPropertyChanged
{
    private const string FilePathInputText = "Select file(s) or folder...";
    private const string KeyInputText = "Enter Key...";
    private const string IVInputText = "Enter IV...";
    private const string DownloadedFilePathInputText = "Downloaded files...";

    private readonly EncryptionManager _encryptionManager;
    private readonly FileTransferManager _fileTransferManager;

    private ObservableCollection<string> EncryptedFiles { get; }
    private ObservableCollection<string> DownloadedFiles { get; }

    public bool CanUploadFile => EncryptedFiles.Any();

    public bool CanEncryptFile =>
        !string.IsNullOrWhiteSpace(FilePathTextBox.Text) && FilePathTextBox.Text != FilePathInputText;

    public bool CanDecryptFile => DownloadedFiles.Any();
    public bool CanSaveDecryptedFile => DownloadedFiles.Any();

    public event PropertyChangedEventHandler PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        InitializeTextBoxes();

        var asyncClient = new AsyncClient();
        var tempDirectory = Path.Combine(AppConfig.GetAppRootDirectory(), "TempFiles");
        Directory.CreateDirectory(tempDirectory);

        var encryptionSettings = new EncryptionSettings();
        _encryptionManager = new EncryptionManager(new FileEncryptor(encryptionSettings));
        _fileTransferManager = new FileTransferManager(asyncClient);

        EncryptedFiles = new ObservableCollection<string>();
        DownloadedFiles = new ObservableCollection<string>();

        DataContext = this;
        EncryptedFiles.CollectionChanged += EncryptedFiles_CollectionChanged;
        DownloadedFiles.CollectionChanged += DownloadedFiles_CollectionChanged;
        EncryptionMethodComboBox.SelectionChanged += EncryptionMethodComboBox_SelectionChanged;

        InitializeDefaultKeyAndIV();
    }

    private void InitializeTextBoxes()
    {
        FilePathTextBox.Text = FilePathInputText;
        DownloadedFilePathTextBox.Text = DownloadedFilePathInputText;
    }

    private void InitializeDefaultKeyAndIV()
    {
        var defaultAlgorithm = "AES";
        _encryptionManager.UpdateKeyAndIVForAlgorithm(defaultAlgorithm, out var keyText, out var ivText);
        KeyTextBox.Text = keyText;
        IVTextBox.Text = ivText;
        EncryptionMethodComboBox.SelectedItem = EncryptionMethodComboBox.Items.Cast<ComboBoxItem>()
            .FirstOrDefault(item => item.Content.ToString() == defaultAlgorithm);
    }

    private void EncryptionMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EncryptionMethodComboBox.SelectedItem is not ComboBoxItem selectedItem) return;
        var encryptionMethod = selectedItem.Content.ToString();
        _encryptionManager.UpdateKeyAndIVForAlgorithm(encryptionMethod, out var keyText, out var ivText);
        KeyTextBox.Text = keyText;
        IVTextBox.Text = ivText;
    }

    private void EncryptedFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanUploadFile));
    }

    private void DownloadedFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanDecryptFile));
        OnPropertyChanged(nameof(CanSaveDecryptedFile));
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void FilePathTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (FilePathTextBox.Text == FilePathInputText) FilePathTextBox.Text = "";
        FilePathTextBox.BorderBrush = Brushes.Gray;
    }

    private void KeyTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (KeyTextBox.Text == KeyInputText) KeyTextBox.Text = "";
        KeyTextBox.BorderBrush = Brushes.Gray;
    }

    private void IVTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (IVTextBox.Text == IVInputText) IVTextBox.Text = "";
        IVTextBox.BorderBrush = Brushes.Gray;
    }

    private void DownloadedFilePathTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (DownloadedFilePathTextBox.Text == DownloadedFilePathInputText) DownloadedFilePathTextBox.Text = "";
        DownloadedFilePathTextBox.BorderBrush = Brushes.Gray;
    }

    private void FilePathTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(FilePathTextBox.Text)) return;
        FilePathTextBox.Text = FilePathInputText;
        FilePathTextBox.BorderBrush = Brushes.Red;
        OnPropertyChanged(nameof(CanEncryptFile));
    }

    private void KeyTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(KeyTextBox.Text)) return;
        KeyTextBox.Text = KeyInputText;
        KeyTextBox.BorderBrush = Brushes.Red;
    }

    private void IVTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(IVTextBox.Text)) return;
        IVTextBox.Text = IVInputText;
        IVTextBox.BorderBrush = Brushes.Red;
    }

    private void DownloadedFilePathTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(DownloadedFilePathTextBox.Text)) return;
        DownloadedFilePathTextBox.Text = DownloadedFilePathInputText;
        DownloadedFilePathTextBox.BorderBrush = Brushes.Red;
    }

    private void FilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilePathTextBox.BorderBrush = string.IsNullOrWhiteSpace(FilePathTextBox.Text) ? Brushes.Red : Brushes.Gray;
        OnPropertyChanged(nameof(CanEncryptFile));
    }

    private void KeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        KeyTextBox.BorderBrush = string.IsNullOrWhiteSpace(KeyTextBox.Text) ? Brushes.Red : Brushes.Gray;
    }

    private void IVTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        IVTextBox.BorderBrush = string.IsNullOrWhiteSpace(IVTextBox.Text) ? Brushes.Red : Brushes.Gray;
    }

    private void DownloadedFilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        DownloadedFilePathTextBox.BorderBrush =
            string.IsNullOrWhiteSpace(DownloadedFilePathTextBox.Text) ? Brushes.Red : Brushes.Gray;
    }

    private void LoadSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsManager.LoadSettings(KeyTextBox, IVTextBox, EncryptionMethodComboBox);
    }

    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsManager.SaveSettings(KeyTextBox, IVTextBox, EncryptionMethodComboBox);
    }

    private void BrowseFilesButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() != true) return;
        FilePathTextBox.Text = string.Join(", ", openFileDialog.FileNames);
        FilePathTextBox.BorderBrush = Brushes.Gray;
        OnPropertyChanged(nameof(CanEncryptFile));
    }

    private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        var dlg = new CommonOpenFileDialog
        {
            Title = "Select Folder",
            IsFolderPicker = true,
            InitialDirectory = initialDirectory,
            AddToMostRecentlyUsedList = false,
            AllowNonFileSystemItems = false,
            DefaultDirectory = initialDirectory,
            EnsureFileExists = true,
            EnsurePathExists = true,
            EnsureReadOnly = false,
            EnsureValidNames = true,
            Multiselect = false,
            ShowPlacesList = true
        };

        if (dlg.ShowDialog() != CommonFileDialogResult.Ok) return;
        var selectedFolder = dlg.FileName;
        var txtFiles = Directory.EnumerateFiles(selectedFolder, "*.txt");
        FilePathTextBox.Text = string.Join(", ", txtFiles);
        FilePathTextBox.BorderBrush = Brushes.Gray;
        OnPropertyChanged(nameof(CanEncryptFile));
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private async void EncryptButton_Click(object sender, RoutedEventArgs e)
    {
        var filePaths = FilePathTextBox.Text.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        var encryptedFiles = await _encryptionManager.EncryptFilesAsync(filePaths, KeyTextBox.Text, IVTextBox.Text,
            ((ComboBoxItem)EncryptionMethodComboBox.SelectedItem).Content.ToString());
        foreach (var file in encryptedFiles) EncryptedFiles.Add(file);
        OnPropertyChanged(nameof(CanUploadFile));
    }

    private async void DecryptButton_Click(object sender, RoutedEventArgs e)
    {
        var filePaths = DownloadedFiles.ToList();
        var decryptedFiles = await _encryptionManager.DecryptFilesAsync(filePaths, KeyTextBox.Text, IVTextBox.Text,
            ((ComboBoxItem)EncryptionMethodComboBox.SelectedItem).Content.ToString());

        DownloadedFiles.Clear();
        foreach (var file in decryptedFiles) DownloadedFiles.Add(file);
        DownloadedFilePathTextBox.Text = string.Join(", ", DownloadedFiles);
    }

    private async void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var file in EncryptedFiles)
        {
            if (!file.EndsWith(".enc"))
            {
                MessageBox.Show($"The file {file} does not have the correct .enc extension.");
                continue;
            }

            var originalFileName = Path.GetFileNameWithoutExtension(FileHelper.GetOriginalFileName(file)) + ".enc";
            var tempFileName = FileHelper.GetUniqueFilePath(Path.GetDirectoryName(file), originalFileName);

            File.Move(file, tempFileName, true);

            if (!await _fileTransferManager.UploadFileAsync(tempFileName, originalFileName))
                MessageBox.Show($"Failed to upload file: {tempFileName}");

            File.Move(tempFileName, file, true);
        }

        MessageBox.Show("Files uploaded successfully.");

        EncryptionManager.ClearTempDirectory();

        EncryptedFiles.Clear();
        OnPropertyChanged(nameof(CanUploadFile));
    }


    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var availableFiles = await _fileTransferManager.GetAvailableFilesAsync();
            if (availableFiles == null || !availableFiles.Any())
            {
                MessageBox.Show("No files available for download.");
                Console.WriteLine("No files available for download.");
                return;
            }

            Console.WriteLine("Available files retrieved: " + string.Join(", ", availableFiles));

            var fileSelectionWindow = new FileSelectionWindow(availableFiles.ToList());
            var dialogResult = fileSelectionWindow.ShowDialog();

            if (dialogResult != true)
            {
                Console.WriteLine("File selection window was closed or no files were selected.");
                return;
            }

            var selectedFiles = fileSelectionWindow.SelectedFiles;
            Console.WriteLine("Selected files: " + string.Join(", ", selectedFiles));

            foreach (var selectedFile in selectedFiles)
            {
                var downloadedFilePath = await _fileTransferManager.RetrieveFileAsync(selectedFile);
                if (!string.IsNullOrWhiteSpace(downloadedFilePath))
                {
                    DownloadedFiles.Add(downloadedFilePath);
                    DownloadedFilePathTextBox.Text = string.Join(", ", DownloadedFiles);
                    MessageBox.Show($"File '{selectedFile}' has been downloaded to temporary directory.");
                }
                else
                {
                    MessageBox.Show($"Failed to download file: {selectedFile}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during the download process: {ex.Message}");
            MessageBox.Show($"An error occurred during the download process: {ex.Message}");
        }
    }

    private void SaveDecryptedFilesButton_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "All files (*.*)|*.*",
            Title = "Save Decrypted File"
        };

        foreach (var downloadedFile in DownloadedFiles)
        {
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(FileHelper.GetOriginalFileName(downloadedFile));
            if (saveFileDialog.ShowDialog() != true) continue;
            var destinationFilePath = saveFileDialog.FileName;
            File.Copy(downloadedFile, destinationFilePath, true);
            MessageBox.Show(
                $"File '{Path.GetFileName(downloadedFile)}' saved successfully as '{destinationFilePath}'.");
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        e.Handled = true;
    }
}