using System.Windows;

namespace Laboratorium4.Zadanie1.NET;

public partial class FileSelectionWindow
{
    public List<string> SelectedFiles { get; private set; }

    public FileSelectionWindow(List<string> availableFiles)
    {
        InitializeComponent();
        LoadFileList(availableFiles);
    }

    private void LoadFileList(List<string> availableFiles)
    {
        FilesListBox.Items.Clear();
        foreach (var file in availableFiles) FilesListBox.Items.Add(file);
        Console.WriteLine("Files loaded into selection window: " + string.Join(", ", availableFiles));
    }

    private void SelectFileButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedFiles = FilesListBox.SelectedItems.Cast<string>().ToList();
        Console.WriteLine("Selected files: " + string.Join(", ", SelectedFiles));
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}