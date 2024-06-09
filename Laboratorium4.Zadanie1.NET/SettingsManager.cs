using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Laboratorium4.Zadanie1.NET;

public static class SettingsManager
{
    private static readonly string SettingsFilePath = AppConfig.EncryptionSettingsFilePath;

    public static void LoadSettings(TextBox keyTextBox, TextBox iVTextBox, ComboBox encryptionMethodComboBox)
    {
        try
        {
            if (!File.Exists(SettingsFilePath)) throw new FileNotFoundException("The settings file does not exist.");

            var json = File.ReadAllText(SettingsFilePath);
            var encryptionSettings = JsonSerializer.Deserialize<EncryptionSettings>(json);

            keyTextBox.Text = Convert.ToBase64String(encryptionSettings.Key);
            iVTextBox.Text = Convert.ToBase64String(encryptionSettings.IV);
            encryptionMethodComboBox.SelectedItem = encryptionMethodComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == encryptionSettings.EncryptionMethod);
            MessageBox.Show("Encryption settings loaded successfully.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while loading the settings: {ex.Message}");
        }
    }

    public static void SaveSettings(TextBox keyTextBox, TextBox iVTextBox, ComboBox encryptionMethodComboBox)
    {
        try
        {
            var encryptionSettings = new EncryptionSettings
            {
                Key = Convert.FromBase64String(keyTextBox.Text),
                IV = Convert.FromBase64String(iVTextBox.Text),
                EncryptionMethod = ((ComboBoxItem)encryptionMethodComboBox.SelectedItem).Content.ToString()
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(encryptionSettings, options);
            File.WriteAllText(SettingsFilePath, json);

            MessageBox.Show("Encryption settings saved successfully.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while saving the settings: {ex.Message}");
        }
    }
}