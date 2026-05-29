using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace CybersecurityChatbot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Chatbot _bot = new Chatbot();
        private readonly MemoryStore _memory = new MemoryStore();

        private bool _awaitingName = true;  
        private bool _isTyping = false;

        private static readonly SolidColorBrush BrCyan = Brush("#00FFFF");
        private static readonly SolidColorBrush BrGreen = Brush("#00FF88");
        private static readonly SolidColorBrush BrYellow = Brush("#FFD700");
        private static readonly SolidColorBrush BrRed = Brush("#FF5050");
        private static readonly SolidColorBrush BrGray = Brush("#555555");
        private static readonly SolidColorBrush BrTextMain = Brush("#E0E0E0");
        private static readonly SolidColorBrush BrTextDim = Brush("#909090");
        private static readonly SolidColorBrush BrUserBg = Brush("#12123A");
        private static readonly SolidColorBrush BrBotBg = Brush("#081018");
        private static readonly SolidColorBrush BrUserBord = Brush("#2A2A6A");
        private static readonly SolidColorBrush BrBotBord = Brush("#0A3030");
        private static readonly FontFamily MonoFont = new FontFamily("Consolas");

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnWindowLoaded;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _bot.PlayVoiceGreeting();
            await Task.Delay(200); // Let window render before first message
            await AnimateBotMessage(_bot.WelcomeMessage(), isWelcome: true);
            TxtInput.Focus();
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            await ProcessInput();
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                _ = ProcessInput();
            }
        }

        private async void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            _memory.Reset();
            _awaitingName = true;

            TxtUserBadge.Text = "User: —";
            TxtStatus.Text = "Conversation cleared. Enter your name to start again.";
            TxtStatus.Foreground = BrYellow;
            TxtSentiment.Text = "";
            MemoryPanel.Visibility = Visibility.Collapsed;
            TxtMemory.Text = "";

            await AnimateBotMessage(
                "Conversation cleared. Let's start fresh!\n\nWhat is your name?",
                isWelcome: false);
            TxtInput.Focus();
        }

        private void TxtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnSend.IsEnabled = TxtInput.Text.Trim().Length > 0 && !_isTyping;
        }

        private async Task ProcessInput()
        {
            if (_isTyping) return;

            string raw = TxtInput.Text;

            // Validate
            if (!InputValidator.isValid(raw))
            {
                AppendBotBubble("I didn't catch that. Could you please rephrase?");
                TxtInput.Clear();
                return;
            }

            TxtInput.Clear();
            BtnSend.IsEnabled = false;

            // ── State: awaiting name ──────────────────────────────────────────
            if (_awaitingName)
            {
                if (!InputValidator.isNameValid(raw))
                {
                    await AnimateBotMessage(
                        "That doesn't look like a valid name.\n\n" +
                        "Please enter at least two letters — numbers only are not accepted.");
                    return;
                }

                _memory.UserName = raw.Trim();
                _awaitingName = false;

                TxtUserBadge.Text = $"User: {_memory.UserName}";
                TxtStatus.Text = $"Chatting as {_memory.UserName}  |  type 'help' for topics  |  type 'exit' to quit";
                TxtStatus.Foreground = BrGreen;

                AppendUserBubble(raw.Trim());
                await AnimateBotMessage(
                    _bot.GreetingMessage(_memory.UserName),
                    isWelcome: true);
                return;
            }

            // ── State: normal chat ────────────────────────────────────────────
            AppendUserBubble(raw.Trim());

            // Exit command
            if (InputValidator.Sanitise(raw) == "exit")
            {
                await AnimateBotMessage(
                    $"Stay safe online, {_memory.UserName}. Goodbye!\n\n" +
                    "Remember: knowledge is your best defence against cybercrime.");
                await Task.Delay(1800);
                Application.Current.Shutdown();
                return;
            }

            // Get response from ResponseHandler (all Part 2 logic lives here)
            string response = KeywordResponder.GetResponse(raw, _memory);

            // Refresh UI indicators
            UpdateSentimentDisplay(_memory.LastSentiment);
            UpdateMemoryPanel();

            // Show response with typing animation
            await AnimateBotMessage(response);
        }

        private void AppendUserBubble(string text)
        {
            TextBlock nameLabel = MakeLabel(_memory.UserName, BrTextDim);
            TextBlock msgText = MakeMessageText(text, BrTextMain);

            StackPanel inner = new StackPanel();
            inner.Children.Add(nameLabel);
            inner.Children.Add(msgText);

            Border bubble = new Border
            {
                Background = BrUserBg,
                BorderBrush = BrUserBord,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(90, 4, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Right,
                Child = inner
            };

            ChatPanel.Children.Add(bubble);
            ScrollToEnd();
        }

        private void AppendBotBubble(string text, bool isWelcome = false)
        {
            var (bubble, _) = CreateBotBubble(text, isWelcome);
            ChatPanel.Children.Add(bubble);
            ScrollToEnd();
        }

        private async Task AnimateBotMessage(string fullText, bool isWelcome = false)
        {
            _isTyping = true;
            BtnSend.IsEnabled = false;

            var (bubble, msgText) = CreateBotBubble("", isWelcome);
            ChatPanel.Children.Add(bubble);
            ScrollToEnd();

            string built = "";
            foreach (char c in fullText)
            {
                built += c;
                msgText.Text = built;

                if (c == '\n') ScrollToEnd();
                await Task.Delay(11); // Typing speed (ms per character)
            }

            ScrollToEnd();
            _isTyping = false;
            BtnSend.IsEnabled = TxtInput.Text.Trim().Length > 0;
        }

        private (Border bubble, TextBlock msgText) CreateBotBubble(string text, bool isWelcome)
        {
            TextBlock nameLabel = MakeLabel("CyberSecBot", BrCyan, bold: true);
            TextBlock msgText = MakeMessageText(text, isWelcome ? BrGreen : BrTextMain);

            StackPanel inner = new StackPanel();
            inner.Children.Add(nameLabel);
            inner.Children.Add(msgText);

            Border bubble = new Border
            {
                Background = BrBotBg,
                BorderBrush = BrBotBord,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 4, 90, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = inner
            };

            return (bubble, msgText);
        }

        private void UpdateSentimentDisplay(string sentiment)
        {
            switch (sentiment)
            {
                case "worried":
                    TxtSentiment.Text = "Mood detected: worried";
                    TxtSentiment.Foreground = BrRed;
                    break;
                case "frustrated":
                    TxtSentiment.Text = "Mood detected: frustrated";
                    TxtSentiment.Foreground = BrYellow;
                    break;
                case "curious":
                    TxtSentiment.Text = "Mood detected: curious";
                    TxtSentiment.Foreground = BrCyan;
                    break;
                case "positive":
                    TxtSentiment.Text = "Mood detected: positive";
                    TxtSentiment.Foreground = BrGreen;
                    break;
                default:
                    TxtSentiment.Text = "";
                    break;
            }
        }

        private void UpdateMemoryPanel()
        {
            if (_memory.InterestedTopics.Count == 0) return;

            MemoryPanel.Visibility = Visibility.Visible;

            string topics = string.Join("  •  ", _memory.InterestedTopics);
            TxtMemory.Text = $"Topics explored:  {topics}";

            if (!string.IsNullOrEmpty(_memory.FavouriteTopic))
                TxtMemory.Text += $"   |   First interest: {_memory.FavouriteTopic}";
        }

        private void ScrollToEnd()
        {
            ChatScroller.ScrollToEnd();
        }

        private TextBlock MakeLabel(string text, SolidColorBrush colour, bool bold = false)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = colour,
                FontFamily = MonoFont,
                FontSize = 10,
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                Margin = new Thickness(0, 0, 0, 4)
            };
        }

        private TextBlock MakeMessageText(string text, SolidColorBrush colour)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = colour,
                FontFamily = MonoFont,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };
        }

        private static SolidColorBrush Brush(string hex)
        {
            return new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(hex));
        }

    }
}