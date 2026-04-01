using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace Rama
{
    public partial class SettingsWindow : Window
    {
        private string SettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Rama", "settings.json");

        public RamaSettings Settings { get; private set; } = new();

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
            ApplySettingsToUI();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    Settings = JsonConvert.DeserializeObject<RamaSettings>(json) ?? new();
                }
            }
            catch
            {
                Settings = new RamaSettings();
            }
        }

        private void ApplySettingsToUI()
        {
            // Voice
            VoiceLanguageCombo.SelectedIndex = Settings.VoiceLanguage switch
            {
                "hi" => 1,
                "mr" => 2,
                _ => 0
            };
            VoiceSpeedSlider.Value = Settings.VoiceSpeed;
            VoiceVolumeSlider.Value = Settings.VoiceVolume;
            AutoSpeakCheckBox.IsChecked = Settings.AutoSpeak;
            WakeWordCheckBox.IsChecked = Settings.WakeWordEnabled;

            // Language
            InterfaceLangCombo.SelectedIndex = Settings.InterfaceLanguage switch
            {
                "hi" => 1, "mr" => 2, "es" => 3, "fr" => 4, "de" => 5,
                "zh" => 6, "ja" => 7, "ko" => 8, "ar" => 9, "ru" => 10, "pt" => 11,
                _ => 0
            };
            AutoDetectCheckBox.IsChecked = Settings.AutoDetectLanguage;
            AmbientLearnCheckBox.IsChecked = Settings.AmbientLearning;
            HinglishCheckBox.IsChecked = Settings.HinglishSupport;

            // Personality
            SassSlider.Value = Settings.SassLevel;
            HumorSlider.Value = Settings.HumorLevel;
            NaughtyCheckBox.IsChecked = Settings.NaughtyMode;
            SassyVoiceCheckBox.IsChecked = Settings.SassyVoice;

            // AI
            LocalAICheckBox.IsChecked = Settings.UseLocalAI;
            SelfLearningCheckBox.IsChecked = Settings.SelfLearning;
            CriticalThinkingCheckBox.IsChecked = Settings.CriticalThinking;
            MistakeLearningCheckBox.IsChecked = Settings.LearnFromMistakes;
            RememberContextCheckBox.IsChecked = Settings.RememberContext;

            // System
            SystemTrayCheckBox.IsChecked = Settings.MinimizeToTray;
            StartupCheckBox.IsChecked = Settings.StartWithWindows;
            NotificationsCheckBox.IsChecked = Settings.ShowNotifications;
            DarkModeCheckBox.IsChecked = Settings.DarkMode;

            DataPathText.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rama");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Read settings from UI
            Settings.VoiceLanguage = VoiceLanguageCombo.SelectedIndex switch
            {
                1 => "hi", 2 => "mr", _ => "en"
            };
            Settings.VoiceSpeed = (int)VoiceSpeedSlider.Value;
            Settings.VoiceVolume = (int)VoiceVolumeSlider.Value;
            Settings.AutoSpeak = AutoSpeakCheckBox.IsChecked ?? true;
            Settings.WakeWordEnabled = WakeWordCheckBox.IsChecked ?? true;

            Settings.InterfaceLanguage = InterfaceLangCombo.SelectedIndex switch
            {
                1 => "hi", 2 => "mr", 3 => "es", 4 => "fr", 5 => "de",
                6 => "zh", 7 => "ja", 8 => "ko", 9 => "ar", 10 => "ru", 11 => "pt",
                _ => "en"
            };
            Settings.AutoDetectLanguage = AutoDetectCheckBox.IsChecked ?? true;
            Settings.AmbientLearning = AmbientLearnCheckBox.IsChecked ?? true;
            Settings.HinglishSupport = HinglishCheckBox.IsChecked ?? true;

            Settings.SassLevel = (int)SassSlider.Value;
            Settings.HumorLevel = (int)HumorSlider.Value;
            Settings.NaughtyMode = NaughtyCheckBox.IsChecked ?? true;
            Settings.SassyVoice = SassyVoiceCheckBox.IsChecked ?? true;

            Settings.UseLocalAI = LocalAICheckBox.IsChecked ?? false;
            Settings.SelfLearning = SelfLearningCheckBox.IsChecked ?? true;
            Settings.CriticalThinking = CriticalThinkingCheckBox.IsChecked ?? true;
            Settings.LearnFromMistakes = MistakeLearningCheckBox.IsChecked ?? true;
            Settings.RememberContext = RememberContextCheckBox.IsChecked ?? true;

            Settings.MinimizeToTray = SystemTrayCheckBox.IsChecked ?? true;
            Settings.StartWithWindows = StartupCheckBox.IsChecked ?? false;
            Settings.ShowNotifications = NotificationsCheckBox.IsChecked ?? true;
            Settings.DarkMode = DarkModeCheckBox.IsChecked ?? true;

            // Save to file
            try
            {
                string dir = Path.GetDirectoryName(SettingsPath)!;
                Directory.CreateDirectory(dir);
                string json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                File.WriteAllText(SettingsPath, json);

                MessageBox.Show("✅ Settings saved! Some changes take effect on restart.",
                    "Rama Settings", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error saving: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Reset all settings to defaults?",
                "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Settings = new RamaSettings();
                ApplySettingsToUI();
            }
        }
    }

    /// <summary>
    /// Rama settings model.
    /// </summary>
    public class RamaSettings
    {
        // Voice
        public string VoiceLanguage { get; set; } = "en";
        public int VoiceSpeed { get; set; } = 1;
        public int VoiceVolume { get; set; } = 85;
        public bool AutoSpeak { get; set; } = true;
        public bool WakeWordEnabled { get; set; } = true;

        // Language
        public string InterfaceLanguage { get; set; } = "en";
        public bool AutoDetectLanguage { get; set; } = true;
        public bool AmbientLearning { get; set; } = true;
        public bool HinglishSupport { get; set; } = true;

        // Personality
        public int SassLevel { get; set; } = 3;
        public int HumorLevel { get; set; } = 3;
        public bool NaughtyMode { get; set; } = true;
        public bool SassyVoice { get; set; } = true;

        // AI
        public bool UseLocalAI { get; set; } = false;
        public bool SelfLearning { get; set; } = true;
        public bool CriticalThinking { get; set; } = true;
        public bool LearnFromMistakes { get; set; } = true;
        public bool RememberContext { get; set; } = true;

        // System
        public bool MinimizeToTray { get; set; } = true;
        public bool StartWithWindows { get; set; } = false;
        public bool ShowNotifications { get; set; } = true;
        public bool DarkMode { get; set; } = true;
    }
}
