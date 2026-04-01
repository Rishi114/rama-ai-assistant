using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Rama.Core;
using Rama.Skills;

namespace Rama
{
    public partial class MainWindow : Window
    {
        private Brain _brain = null!;
        private Learner _learner = null!;
        private Memory _memory = null!;
        private SkillManager _skillManager = null!;
        private VoiceEngine _voice = null!;
        private bool _isProcessing = false;
        private bool _voiceMode = false;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            InputBox.KeyDown += InputBox_KeyDown;
            InputBox.TextChanged += InputBox_TextChanged;
            SendButton.Click += SendButton_Click;
        }

        #region Window Controls

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                Maximize_Click(sender, e);
            else
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            _voice?.Dispose();
            _learner?.Dispose();
            Application.Current.Shutdown();
        }

        #endregion

        #region Initialization

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeRama();
            InitializeVoice();
            InputBox.Focus();
        }

        private Task InitializeRama()
        {
            try
            {
                string appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rama");
                Directory.CreateDirectory(appData);
                string dbPath = Path.Combine(appData, "rama_memory.db");

                _learner = new Learner(dbPath);
                _memory = new Memory();
                _skillManager = new SkillManager();
                _skillManager.LoadBuiltInSkills();
                _brain = new Brain(_skillManager, _learner, _memory);

                string skillsDir = Path.Combine(appData, "Skills");
                Directory.CreateDirectory(skillsDir);
                _skillManager.LoadSkillsFromDirectory(skillsDir);

                UpdateSkillsList();
                UpdateStats();

                StatusText.Text = "Ready — All systems online";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Init error: {ex.Message}";
                MessageBox.Show($"Failed to initialize Rama:\n\n{ex.Message}", "Startup Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return Task.CompletedTask;
        }

        private void InitializeVoice()
        {
            try
            {
                _voice = new VoiceEngine();
                bool ok = _voice.Initialize();

                if (ok)
                {
                    _voice.OnSpeechRecognized += OnVoiceRecognized;
                    _voice.OnSpeechPartial += OnVoicePartial;
                    _voice.OnListeningStarted += () =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            VoiceStatusBadge.Visibility = Visibility.Visible;
                            MicButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F38BA8"));
                            StatusDot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F38BA8"));
                            StatusText.Text = "Listening...";
                        });
                    };
                    _voice.OnListeningStopped += () =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            VoiceStatusBadge.Visibility = Visibility.Collapsed;
                            MicButton.Background = FindResource("CardBrush") as Brush;
                            StatusDot.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A6E3A1"));
                            StatusText.Text = _voiceMode ? "Voice mode on — say 'hey rama'" : "Ready";
                            VoicePartialText.Text = "";
                        });
                    };
                    _voice.OnError += (err) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            StatusText.Text = $"Voice: {err}";
                        });
                    };

                    // Greet with voice on first load
                    _voice.Speak("Hey! I'm Rama. Your sassy AI assistant. Click the mic to start talking to me!");
                }
                else
                {
                    MicButton.IsEnabled = false;
                    MicButton.Opacity = 0.3;
                }
            }
            catch (Exception ex)
            {
                // Voice not available — disable button
                MicButton.IsEnabled = false;
                MicButton.Opacity = 0.3;
                StatusText.Text = "Voice unavailable — " + ex.Message;
            }
        }

        #endregion

        #region Voice Handling

        private void VoiceToggle_Click(object sender, MouseButtonEventArgs e)
        {
            if (_voice == null || !_voice.IsInitialized) return;

            _voiceMode = !_voiceMode;

            if (_voiceMode)
            {
                VoiceIcon.Text = "🎙️";
                VoiceLabel.Text = " Voice On";
                VoiceToggle.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F38BA8"));
                _voice.StartListening();
                AddBotMessage("🎙️ **Voice mode ON!** I'm listening. Say something like:\n\n" +
                    "• \"Open notepad\"\n" +
                    "• \"What's the weather in London?\"\n" +
                    "• \"Remind me in 5 minutes to stretch\"\n" +
                    "• \"Take note buy groceries\"\n\n" +
                    "Say **\"stop listening\"** to pause.");
            }
            else
            {
                VoiceIcon.Text = "🎤";
                VoiceLabel.Text = " Voice Off";
                VoiceToggle.Background = FindResource("CardBrush") as Brush;
                _voice.StopListening();
                AddBotMessage("🔇 Voice mode off. Back to typing!");
            }
        }

        private void MicButton_Click(object sender, RoutedEventArgs e)
        {
            if (_voice == null || !_voice.IsInitialized) return;

            if (_voice.IsSpeaking)
            {
                _voice.StopSpeaking();
                return;
            }

            _voice.ToggleListening();
        }

        private void OnVoiceRecognized(string text)
        {
            Dispatcher.Invoke(async () =>
            {
                VoicePartialText.Text = "";

                // Check for voice commands
                string lower = text.ToLowerInvariant();
                if (lower.Contains("stop listening"))
                {
                    _voiceMode = false;
                    VoiceIcon.Text = "🎤";
                    VoiceLabel.Text = " Voice Off";
                    VoiceToggle.Background = FindResource("CardBrush") as Brush;
                    _voice.StopListening();
                    _voice.Speak("Alright, shutting up. I mean... stopping listening.");
                    return;
                }

                if (lower.Contains("be quiet") || lower.Contains("shut up"))
                {
                    _voice.StopSpeaking();
                    AddBotMessage("*Fine.* 😤");
                    return;
                }

                // Process as regular input
                InputBox.Text = text;
                await ProcessInput();

                // Speak the response if in voice mode
                if (_voiceMode)
                {
                    var lastBotMsg = GetLastBotMessage();
                    if (!string.IsNullOrEmpty(lastBotMsg))
                    {
                        _voice.Speak(lastBotMsg);
                    }
                }
            });
        }

        private void OnVoicePartial(string text)
        {
            Dispatcher.Invoke(() =>
            {
                VoicePartialText.Text = text;
            });
        }

        private string GetLastBotMessage()
        {
            // Find last bot bubble
            for (int i = ChatPanel.Children.Count - 1; i >= 0; i--)
            {
                if (ChatPanel.Children[i] is Border border &&
                    border.Style == FindResource("ChatBubbleBot") &&
                    border.Child is StackPanel sp)
                {
                    var texts = new List<string>();
                    foreach (var child in sp.Children)
                    {
                        if (child is TextBlock tb)
                            texts.Add(tb.Text);
                    }
                    return string.Join(" ", texts);
                }
            }
            return "";
        }

        #endregion

        #region UI Updates

        private void UpdateSkillsList()
        {
            SkillsPanel.Children.Clear();

            var title = new TextBlock
            {
                Text = "⚡ SKILLS",
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Foreground = FindResource("SubTextBrush") as Brush,
                Margin = new Thickness(12, 8, 0, 6)
            };
            SkillsPanel.Children.Add(title);

            var skillEmojis = new Dictionary<string, string>
            {
                { "App Launcher", "🚀" }, { "File Manager", "📁" },
                { "Web Search", "🔍" }, { "Notes", "📝" },
                { "Reminders", "⏰" }, { "Calculator", "🧮" },
                { "System Info", "💻" }, { "Weather", "🌤️" },
                { "Greeting", "💬" }
            };

            foreach (var skill in _skillManager.GetAllSkills())
            {
                var card = new Border { Style = FindResource("SkillCard") as Style };
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                string emoji = skillEmojis.TryGetValue(skill.Name, out var e) ? e : "⚡";
                var emojiBlock = new TextBlock { Text = emoji, FontSize = 16, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(emojiBlock, 0);
                grid.Children.Add(emojiBlock);

                var infoPanel = new StackPanel { Margin = new Thickness(8, 0, 0, 0) };
                infoPanel.Children.Add(new TextBlock
                {
                    Text = skill.Name,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = FindResource("TextBrush") as Brush
                });
                infoPanel.Children.Add(new TextBlock
                {
                    Text = skill.Description,
                    FontSize = 10,
                    Foreground = FindResource("SubTextBrush") as Brush,
                    TextWrapping = TextWrapping.Wrap
                });
                Grid.SetColumn(infoPanel, 1);
                grid.Children.Add(infoPanel);

                card.Child = grid;
                string skillName = skill.Name;
                card.MouseLeftButtonDown += (s, e) =>
                {
                    InputBox.Text = $"help with {skillName.ToLower()}";
                    InputBox.Focus();
                    InputBox.CaretIndex = InputBox.Text.Length;
                    PlaceholderText.Visibility = Visibility.Collapsed;
                };
                SkillsPanel.Children.Add(card);
            }
        }

        private void UpdateStats()
        {
            if (_brain == null) return;
            var stats = _brain.GetStats();
            StatInteractions.Text = stats.TotalInteractions.ToString();
            StatPatterns.Text = stats.UniquePatterns.ToString();
            StatTopSkill.Text = stats.TopSkill != null ? $"Top: {stats.TopSkill}" : "Start chatting!";
        }

        #endregion

        #region Input Handling

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrEmpty(InputBox.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                _ = ProcessInput();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) => _ = ProcessInput();

        private void Suggestion_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Child is TextBlock tb)
            {
                string text = tb.Text switch
                {
                    "🚀 Launch apps" => "open notepad",
                    "📁 Manage files" => "list files",
                    "🔍 Web search" => "search latest news",
                    "📝 Take notes" => "take note hello world",
                    "⏰ Set reminders" => "remind me in 5 minutes to take a break",
                    "🧮 Calculator" => "calculate 42 * 17",
                    "🌤️ Weather" => "weather in London",
                    "🎤 Voice mode" => "set sass max",
                    _ => ""
                };
                InputBox.Text = text;
                InputBox.Focus();
                PlaceholderText.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Processing

        private async Task ProcessInput()
        {
            if (_isProcessing) return;

            string input = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            _isProcessing = true;
            InputBox.Text = "";
            InputBox.IsEnabled = false;

            AddUserMessage(input);
            _memory.Remember("user", input);

            var typingBorder = ShowTypingIndicator();

            StatusText.Text = "Rama is thinking...";
            SkillIndicator.Text = "";

            try
            {
                string response = await _brain.ThinkAsync(input);

                RemoveTypingIndicator(typingBorder);

                var recent = _learner.GetRecentInteractions(1);
                if (recent.Any() && recent[0].Skill != "none" && recent[0].Skill != "conversation")
                    SkillIndicator.Text = $"⚡ {recent[0].Skill}";

                AddBotMessage(response);
                _memory.Remember("assistant", response);

                StatusText.Text = _voiceMode ? "Voice mode on — say 'hey rama'" : "Ready";
                UpdateStats();
            }
            catch (Exception ex)
            {
                RemoveTypingIndicator(typingBorder);
                AddBotMessage($"Well THAT didn't work: {ex.Message}. Try again? 😅");
                StatusText.Text = "Error — ready to try again";
            }
            finally
            {
                _isProcessing = false;
                InputBox.IsEnabled = true;
                InputBox.Focus();
            }
        }

        #endregion

        #region Chat Messages

        private void AddUserMessage(string content)
        {
            var border = new Border { Style = FindResource("ChatBubbleUser") as Style };
            border.Child = new TextBlock
            {
                Text = content,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = Brushes.White
            };
            border.Opacity = 0;
            ChatPanel.Children.Add(border);

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            border.BeginAnimation(OpacityProperty, fadeIn);
            ScrollToBottom();
        }

        private void AddBotMessage(string content)
        {
            var border = new Border { Style = FindResource("ChatBubbleBot") as Style };
            var stackPanel = new StackPanel { MaxWidth = 480 };

            // Header
            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            header.Children.Add(new TextBlock { Text = "🤖", FontSize = 14, VerticalAlignment = VerticalAlignment.Center });
            header.Children.Add(new TextBlock
            {
                Text = " Rama",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = FindResource("AccentBrush") as Brush,
                VerticalAlignment = VerticalAlignment.Center
            });
            stackPanel.Children.Add(header);

            // Content
            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                Foreground = FindResource("TextBrush") as Brush,
                LineHeight = 20
            };
            ParseFormattedText(textBlock, content);
            stackPanel.Children.Add(textBlock);

            border.Child = stackPanel;
            border.Opacity = 0;
            ChatPanel.Children.Add(border);

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            border.BeginAnimation(OpacityProperty, fadeIn);
            ScrollToBottom();
        }

        private Border ShowTypingIndicator()
        {
            var border = new Border { Style = FindResource("TypingIndicator") as Style };
            var stack = new StackPanel { Orientation = Orientation.Horizontal };

            for (int i = 0; i < 3; i++)
            {
                var dot = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = FindResource("SubTextBrush") as Brush,
                    Margin = new Thickness(0, 0, 4, 0),
                    Opacity = 0.4
                };
                var anim = new DoubleAnimation(0.4, 1.0, TimeSpan.FromMilliseconds(400))
                {
                    BeginTime = TimeSpan.FromMilliseconds(i * 150),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                dot.BeginAnimation(OpacityProperty, anim);
                stack.Children.Add(dot);
            }

            stack.Children.Add(new TextBlock
            {
                Text = " Thinking...",
                FontSize = 12,
                Foreground = FindResource("SubTextBrush") as Brush,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 0, 0)
            });

            border.Child = stack;
            ChatPanel.Children.Add(border);
            ScrollToBottom();
            return border;
        }

        private void RemoveTypingIndicator(Border border)
        {
            if (border != null && ChatPanel.Children.Contains(border))
                ChatPanel.Children.Remove(border);
        }

        private void ScrollToBottom()
        {
            Dispatcher.BeginInvoke(new Action(() => ChatScrollViewer.ScrollToEnd()),
                DispatcherPriority.Background);
        }

        #endregion

        #region Text Formatting

        private void ParseFormattedText(TextBlock textBlock, string content)
        {
            var lines = content.Split('\n');
            bool first = true;
            foreach (var line in lines)
            {
                if (!first) textBlock.Inlines.Add(new LineBreak());
                first = false;
                if (string.IsNullOrWhiteSpace(line)) continue;
                ProcessInlineFormatting(textBlock, line);
            }
        }

        private void ProcessInlineFormatting(TextBlock textBlock, string text)
        {
            int pos = 0;
            while (pos < text.Length)
            {
                int boldStart = text.IndexOf("**", pos);
                if (boldStart < 0)
                {
                    textBlock.Inlines.Add(new Run(text.Substring(pos)));
                    break;
                }
                if (boldStart > pos)
                    textBlock.Inlines.Add(new Run(text.Substring(pos, boldStart - pos)));

                int boldEnd = text.IndexOf("**", boldStart + 2);
                if (boldEnd < 0)
                {
                    textBlock.Inlines.Add(new Run(text.Substring(boldStart)));
                    break;
                }
                textBlock.Inlines.Add(new Run(text.Substring(boldStart + 2, boldEnd - boldStart - 2))
                { FontWeight = FontWeights.SemiBold });
                pos = boldEnd + 2;
            }
        }

        #endregion
    }
}
