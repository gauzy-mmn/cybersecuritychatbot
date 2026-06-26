using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace CybersecurityChatbot
{


    /// <summary>
    /// Code-behind for MainWindow.xaml (Part 3).
    /// Manages four tabs: Chat, Tasks, Quiz, Activity Log.
    /// Integrates: ChatBot, ResponseHandler, NLPProcessor,
    ///             DatabaseManager, QuizManager, ActivityLog, Memory.
    /// </summary>
    public partial class MainWindow : Window
    {
        // ── Session objects ───────────────────────────────────────────────────
        private readonly ChatBot _bot = new ChatBot();
        private readonly Memory _memory = new Memory();
        private readonly QuizManager _quiz = new QuizManager();

        // ── UI state ──────────────────────────────────────────────────────────
        private bool _awaitingName = true;
        private bool _isTyping = false;
        private bool _dbConnected = false;
        private bool _awaitingAnswer = false;

        // ── Brushes ───────────────────────────────────────────────────────────
        private static readonly SolidColorBrush BrCyan = B("#00FFFF");
        private static readonly SolidColorBrush BrGreen = B("#00FF88");
        private static readonly SolidColorBrush BrYellow = B("#FFD700");
        private static readonly SolidColorBrush BrRed = B("#FF5050");
        private static readonly SolidColorBrush BrText = B("#E0E0E0");
        private static readonly SolidColorBrush BrMuted = B("#888888");
        private static readonly SolidColorBrush BrUserBg = B("#12123A");
        private static readonly SolidColorBrush BrBotBg = B("#081018");
        private static readonly SolidColorBrush BrBotBord = B("#0A3030");
        private static readonly SolidColorBrush BrUserBord = B("#2A2A6A");
        private static readonly FontFamily Mono = new FontFamily("Consolas");

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        // ── Startup ───────────────────────────────────────────────────────────
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Test DB connection
            _dbConnected = DatabaseManager.TestConnection();
            if (_dbConnected)
            {
                DatabaseManager.EnsureTableExists();
                TxtDbStatus.Text = "DB: connected ✓";
                TxtDbStatus.Foreground = BrGreen;
                ActivityLog.Log("Database connection established");
            }
            else
            {
                TxtDbStatus.Text = "DB: offline ✗";
                TxtDbStatus.Foreground = BrRed;
                ActivityLog.Log("Database connection failed — task storage unavailable");
            }

            _bot.PlayVoiceGreeting();
            await Task.Delay(200);
            await AnimateBotMessage(_bot.GetWelcomeMessage(), isWelcome: true);
            TxtInput.Focus();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  TAB: CHAT
        // =════════════════════════════════════════════════════════════════════

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
            => await ProcessChatInput();

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { e.Handled = true; _ = ProcessChatInput(); }
        }

        private void TxtInput_TextChanged(object sender, TextChangedEventArgs e)
            => BtnSend.IsEnabled = TxtInput.Text.Trim().Length > 0 && !_isTyping;

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
            await AnimateBotMessage("Conversation cleared. Let's start fresh!\n\nWhat is your name?");
            TxtInput.Focus();
        }

        private async Task ProcessChatInput()
        {
            if (_isTyping) return;
            string raw = TxtInput.Text;
            if (!InputValidator.isValid(raw))
            {
                AppendBotBubble("I didn't catch that. Could you please rephrase?");
                TxtInput.Clear();
                return;
            }

            TxtInput.Clear();
            BtnSend.IsEnabled = false;

            // ── Awaiting name ─────────────────────────────────────────────────
            if (_awaitingName)
            {
                if (!InputValidator.isNameValid(raw))
                {
                    await AnimateBotMessage(
                        "That doesn't look like a valid name.\n" +
                        "Please enter at least two letters.");
                    return;
                }
                _memory.UserName = raw.Trim();
                _awaitingName = false;
                TxtUserBadge.Text = $"User: {_memory.UserName}";
                TxtStatus.Text = $"Chatting as {_memory.UserName}  |  'help' for topics  |  'exit' to quit";
                TxtStatus.Foreground = BrGreen;
                AppendUserBubble(raw.Trim());
                await AnimateBotMessage(_bot.GetGreetingMessage(_memory.UserName), isWelcome: true);
                return;
            }

            // ── Normal chat ───────────────────────────────────────────────────
            AppendUserBubble(raw.Trim());

            if (InputValidator.Sanitise(raw) == "exit")
            {
                await AnimateBotMessage($"Stay safe online, {_memory.UserName}. Goodbye!");
                await Task.Delay(1500);
                Application.Current.Shutdown();
                return;
            }

            // ── NLP intent detection ──────────────────────────────────────────
            string intent = NLPProcessor.DetectIntent(raw);
            string response;

            switch (intent)
            {
                case "add_task":
                    response = await HandleAddTaskIntent(raw);
                    break;

                case "set_reminder":
                    response = HandleReminderIntent(raw);
                    break;

                case "view_tasks":
                    response = HandleViewTasksIntent();
                    break;

                case "complete_task":
                    response = "To mark a task as done, please switch to the Tasks tab and click the ✓ button next to the task.";
                    break;

                case "delete_task":
                    response = "To delete a task, switch to the Tasks tab and click the ✗ button next to the task.";
                    break;

                case "start_quiz":
                    MainTabs.SelectedIndex = 2;
                    response = "Switching you to the Quiz tab! Click 'Start Quiz' when you're ready.";
                    ActivityLog.Log("User navigated to Quiz via chat command");
                    break;

                case "show_log":
                    response = ActivityLog.GetRecentSummary();
                    MainTabs.SelectedIndex = 3;
                    RefreshLogPanel();
                    break;

                default:
                    response = ResponseHandler.GetResponse(raw, _memory);
                    break;
            }

            UpdateSentimentDisplay(_memory.LastSentiment);
            UpdateMemoryPanel();
            TxtGlobalStatus.Text = $"Last message: {DateTime.Now:HH:mm:ss}";
            await AnimateBotMessage(response);
        }

        // ── NLP intent handlers ────────────────────────────────────────────────

        private async Task<string> HandleAddTaskIntent(string raw)
        {
            if (!_dbConnected)
                return "I can't save tasks right now — the database is offline. Please check your MySQL connection.";

            string title = NLPProcessor.ExtractTaskTitle(raw);
            string description = NLPProcessor.BuildTaskDescription(title);
            DateTime? reminder = NLPProcessor.ExtractReminderDate(raw);

            int id = DatabaseManager.AddTask(title, description, reminder);
            if (id < 0)
                return "I had trouble saving that task. Please try again or use the Tasks tab.";

            ActivityLog.LogTaskAdded(title, reminder);
            RefreshTaskPanel();

            string remMsg = reminder.HasValue
                ? $"Reminder set for {reminder.Value:dd MMM yyyy}."
                : "No reminder was set. You can add one in the Tasks tab.";

            return $"Task added: '{title}'\n\n{description}\n\n{remMsg}";
        }

        private string HandleReminderIntent(string raw)
        {
            DateTime? date = NLPProcessor.ExtractReminderDate(raw);
            string title = NLPProcessor.ExtractTaskTitle(raw);

            if (!_dbConnected)
                return "Database is offline — I can't save reminders right now.";

            if (!date.HasValue)
                return "I'd love to set a reminder! Could you tell me when? For example: 'in 3 days' or 'in 1 week'.";

            int id = DatabaseManager.AddTask(title, NLPProcessor.BuildTaskDescription(title), date);
            if (id > 0)
            {
                ActivityLog.LogReminderSet(title, date.Value);
                RefreshTaskPanel();
                return $"Reminder set for '{title}' on {date.Value:dd MMM yyyy}.";
            }

            return "I had trouble saving that reminder. Please use the Tasks tab to set it manually.";
        }

        private string HandleViewTasksIntent()
        {
            if (!_dbConnected)
                return "Database is offline. I can't retrieve your tasks right now.";

            var tasks = DatabaseManager.GetAllTasks();
            if (tasks.Count == 0)
                return "You have no tasks yet. Add one by saying 'Add a task to...' or using the Tasks tab.";

            var pending = tasks.FindAll(t => !t.IsCompleted);
            var completed = tasks.FindAll(t => t.IsCompleted);

            string response = $"You have {pending.Count} pending task(s) and {completed.Count} completed task(s).\n\n";
            response += "Pending:\n";
            foreach (var t in pending)
                response += $"  • {t.Title}" + (t.ReminderDate.HasValue ? $" (Reminder: {t.ReminderDate.Value:dd MMM yyyy})" : "") + "\n";

            MainTabs.SelectedIndex = 1;
            RefreshTaskPanel();
            return response.TrimEnd();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  TAB: TASKS
        // =════════════════════════════════════════════════════════════════════

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabs.SelectedIndex == 1) RefreshTaskPanel();
            if (MainTabs.SelectedIndex == 3) RefreshLogPanel();
        }

        private async void BtnAddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TxtTaskTitle.Text.Trim();
            string desc = TxtTaskDesc.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                TxtTaskStatus.Text = "Please enter a task title.";
                TxtTaskStatus.Foreground = BrRed;
                return;
            }

            if (!_dbConnected)
            {
                TxtTaskStatus.Text = "Database offline — cannot save task.";
                TxtTaskStatus.Foreground = BrRed;
                return;
            }

            DateTime? reminder = DpReminder.SelectedDate;
            if (string.IsNullOrWhiteSpace(desc))
                desc = NLPProcessor.BuildTaskDescription(title);

            int id = DatabaseManager.AddTask(title, desc, reminder);
            if (id > 0)
            {
                ActivityLog.LogTaskAdded(title, reminder);
                TxtTaskTitle.Clear();
                TxtTaskDesc.Clear();
                DpReminder.SelectedDate = null;
                TxtTaskStatus.Text = "Task saved successfully!";
                TxtTaskStatus.Foreground = BrGreen;
                RefreshTaskPanel();

                await Task.Delay(2500);
                TxtTaskStatus.Text = "";
            }
            else
            {
                TxtTaskStatus.Text = "Error saving task. Check DB connection.";
                TxtTaskStatus.Foreground = BrRed;
            }
        }

        private void BtnRefreshTasks_Click(object sender, RoutedEventArgs e)
            => RefreshTaskPanel();

        private void RefreshTaskPanel()
        {
            TaskListPanel.Children.Clear();

            if (!_dbConnected)
            {
                TaskListPanel.Children.Add(MakeLabel("Database offline — tasks unavailable.", BrRed));
                return;
            }

            var tasks = DatabaseManager.GetAllTasks();
            if (tasks.Count == 0)
            {
                TaskListPanel.Children.Add(
                    MakeLabel("No tasks yet. Add one using the form above.", BrMuted));
                return;
            }

            foreach (var task in tasks)
                TaskListPanel.Children.Add(BuildTaskCard(task));
        }

        private Border BuildTaskCard(CyberTask task)
        {
            bool done = task.IsCompleted;

            var titleBlock = new TextBlock
            {
                Text = task.Title,
                Foreground = done ? BrMuted : BrText,
                FontFamily = Mono,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                TextDecorations = done ? TextDecorations.Strikethrough : null
            };

            var descBlock = new TextBlock
            {
                Text = task.Description,
                Foreground = B("#888888"),
                FontFamily = Mono,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0)
            };

            string remText = task.ReminderDate.HasValue
                ? $"🔔 Reminder: {task.ReminderDate.Value:dd MMM yyyy}"
                : "";
            var remBlock = new TextBlock
            {
                Text = remText,
                Foreground = BrYellow,
                FontFamily = Mono,
                FontSize = 11,
                Margin = new Thickness(0, 4, 0, 0),
                Visibility = string.IsNullOrEmpty(remText) ? Visibility.Collapsed : Visibility.Visible
            };

            var dateBlock = new TextBlock
            {
                Text = $"Added: {task.CreatedAt:dd MMM yyyy HH:mm}",
                Foreground = B("#444444"),
                FontFamily = Mono,
                FontSize = 10,
                Margin = new Thickness(0, 4, 0, 0)
            };

            // Action buttons
            var btnComplete = new Button
            {
                Content = "✓ Done",
                Style = (Style)FindResource("BtnSuccess"),
                Height = 30,
                Margin = new Thickness(0, 0, 8, 0),
                Tag = task,
                IsEnabled = !done
            };
            btnComplete.Click += (s, e) =>
            {
                var t = (CyberTask)((Button)s).Tag;
                if (DatabaseManager.MarkCompleted(t.Id))
                {
                    ActivityLog.LogTaskCompleted(t.Title);
                    RefreshTaskPanel();
                }
            };

            var btnDelete = new Button
            {
                Content = "✗ Delete",
                Style = (Style)FindResource("BtnDanger"),
                Height = 30,
                Tag = task
            };
            btnDelete.Click += (s, e) =>
            {
                var t = (CyberTask)((Button)s).Tag;
                if (DatabaseManager.DeleteTask(t.Id))
                {
                    ActivityLog.LogTaskDeleted(t.Title);
                    RefreshTaskPanel();
                }
            };

            var btnRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };
            btnRow.Children.Add(btnComplete);
            btnRow.Children.Add(btnDelete);

            var content = new StackPanel { Margin = new Thickness(14, 12, 14, 12) };
            content.Children.Add(titleBlock);
            content.Children.Add(descBlock);
            content.Children.Add(remBlock);
            content.Children.Add(dateBlock);
            content.Children.Add(btnRow);

            return new Border
            {
                Background = done ? B("#0A0A0A") : B("#0A1220"),
                BorderBrush = done ? B("#222222") : B("#00FFFF22"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 0, 10),
                Child = content
            };
        }

        // ═════════════════════════════════════════════════════════════════════
        //  TAB: QUIZ
        // =════════════════════════════════════════════════════════════════════

        private void BtnStartQuiz_Click(object sender, RoutedEventArgs e)
        {
            _quiz.Start();
            BtnStartQuiz.Visibility = Visibility.Collapsed;
            TxtFinalScore.Text = "";
            FeedbackCard.Visibility = Visibility.Collapsed;
            ShowCurrentQuestion();
        }

        private void ShowCurrentQuestion()
        {
            if (_quiz.IsFinished)
            {
                EndQuiz();
                return;
            }

            var q = _quiz.GetCurrentQuestion();

            // Update progress
            int idx = _quiz.CurrentIndex + 1;
            TxtQuizProgress.Text = $"Question {idx} of {_quiz.TotalQuestions}";
            TxtQuizProgress.Foreground = BrCyan;
            TxtQuizScore.Text = $"Score: {_quiz.Score}/{_quiz.CurrentIndex}";

            // Progress bar width
            double pct = (double)_quiz.CurrentIndex / _quiz.TotalQuestions;
            double maxW = ActualWidth - 100;
            QuizProgressBar.Width = Math.Max(0, pct * maxW);

            // Question card
            QuizQuestionCard.Visibility = Visibility.Visible;
            TxtQuizType.Text = q.IsTrueFalse ? "TRUE / FALSE" : "MULTIPLE CHOICE";
            TxtQuestion.Text = q.QuestionText;

            // Answer buttons
            AnswerPanel.Visibility = Visibility.Visible;
            FeedbackCard.Visibility = Visibility.Collapsed;

            var buttons = new[] { BtnA, BtnB, BtnC, BtnD };
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i < q.Options.Count)
                {
                    buttons[i].Content = $"  {(char)('A' + i)})  {q.Options[i]}";
                    buttons[i].Visibility = Visibility.Visible;
                    buttons[i].IsEnabled = true;
                    buttons[i].Foreground = BrMuted;
                }
                else
                {
                    buttons[i].Visibility = Visibility.Collapsed;
                }
            }

            _awaitingAnswer = true;
        }

        private void BtnAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (!_awaitingAnswer) return;
            _awaitingAnswer = false;

            int selectedIndex = int.Parse(((Button)sender).Tag.ToString());
            var q = _quiz.GetCurrentQuestion();
            bool correct = _quiz.SubmitAnswer(selectedIndex);

            // Colour the buttons
            var buttons = new[] { BtnA, BtnB, BtnC, BtnD };
            for (int i = 0; i < buttons.Length && i < q.Options.Count; i++)
            {
                buttons[i].IsEnabled = false;
                if (i == q.CorrectIndex)
                    buttons[i].Foreground = BrGreen;
                else if (i == selectedIndex && !correct)
                    buttons[i].Foreground = BrRed;
            }

            // Feedback
            TxtFeedbackResult.Text = correct ? "✓  Correct!" : "✗  Incorrect";
            TxtFeedbackResult.Foreground = correct ? BrGreen : BrRed;
            TxtFeedbackExplanation.Text = q.Explanation;
            FeedbackCard.BorderBrush = correct ? B("#00FF8844") : B("#FF505044");
            FeedbackCard.Background = correct ? B("#061206") : B("#120606");
            FeedbackCard.Visibility = Visibility.Visible;

            TxtQuizScore.Text = $"Score: {_quiz.Score}/{_quiz.CurrentIndex}";

            if (_quiz.IsFinished)
                BtnNextQuestion.Content = "SEE FINAL SCORE";
        }

        private void BtnNextQuestion_Click(object sender, RoutedEventArgs e)
        {
            FeedbackCard.Visibility = Visibility.Collapsed;

            if (_quiz.IsFinished)
                EndQuiz();
            else
                ShowCurrentQuestion();
        }

        private void EndQuiz()
        {
            QuizQuestionCard.Visibility = Visibility.Collapsed;
            AnswerPanel.Visibility = Visibility.Collapsed;
            FeedbackCard.Visibility = Visibility.Collapsed;
            BtnStartQuiz.Visibility = Visibility.Visible;
            BtnStartQuiz.Content = "↺  PLAY AGAIN";

            double pct = (double)_quiz.Score / _quiz.TotalQuestions;
            QuizProgressBar.Width = Math.Max(0, pct * (ActualWidth - 100));
            TxtQuizProgress.Text = $"Quiz complete — {_quiz.Score}/{_quiz.TotalQuestions} correct";
            TxtQuizScore.Text = $"{(int)(pct * 100)}%";
            TxtFinalScore.Text = _quiz.GetFinalFeedback();
            TxtFinalScore.Foreground = pct >= 0.75 ? BrGreen : BrYellow;

            ActivityLog.Log($"Quiz ended — {_quiz.Score}/{_quiz.TotalQuestions}");
            RefreshLogPanel();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  TAB: ACTIVITY LOG
        // =════════════════════════════════════════════════════════════════════

        private void BtnRefreshLog_Click(object sender, RoutedEventArgs e)
            => RefreshLogPanel();

        private void RefreshLogPanel()
        {
            LogPanel.Children.Clear();
            var entries = ActivityLog.GetAll();
            TxtLogCount.Text = $"{entries.Count} entries";

            if (entries.Count == 0)
            {
                LogPanel.Children.Add(MakeLabel("No activity recorded yet.", BrMuted));
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var (time, desc) = entries[i];

                var timeBlock = new TextBlock
                {
                    Text = $"[{time:HH:mm:ss}]",
                    Foreground = B("#444444"),
                    FontFamily = Mono,
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Top
                };

                var descBlock = new TextBlock
                {
                    Text = desc,
                    Foreground = BrText,
                    FontFamily = Mono,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap
                };

                var row = new Grid { Margin = new Thickness(0, 0, 0, 1) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                Grid.SetColumn(timeBlock, 0);
                Grid.SetColumn(descBlock, 1);
                row.Children.Add(timeBlock);
                row.Children.Add(descBlock);

                var card = new Border
                {
                    Background = i % 2 == 0 ? B("#0A0A0A") : B("#0D0D10"),
                    BorderBrush = B("#1A1A1A"),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(12, 8, 12, 8),
                    Child = row
                };
                LogPanel.Children.Add(card);
            }

            LogScroller.ScrollToTop();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  CHAT BUBBLE HELPERS
        // =════════════════════════════════════════════════════════════════════

        private void AppendUserBubble(string text)
        {
            var inner = new StackPanel();
            inner.Children.Add(MakeLabel(_memory.UserName, BrMuted));
            inner.Children.Add(MakeMessageText(text, BrText));

            ChatPanel.Children.Add(new Border
            {
                Background = BrUserBg,
                BorderBrush = BrUserBord,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(80, 4, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Right,
                Child = inner
            });
            ChatScroller.ScrollToEnd();
        }

        private void AppendBotBubble(string text, bool isWelcome = false)
        {
            var (bubble, _) = MakeBotBubble(text, isWelcome);
            ChatPanel.Children.Add(bubble);
            ChatScroller.ScrollToEnd();
        }

        private async Task AnimateBotMessage(string full, bool isWelcome = false)
        {
            _isTyping = true;
            BtnSend.IsEnabled = false;

            var (bubble, msgText) = MakeBotBubble("", isWelcome);
            ChatPanel.Children.Add(bubble);
            ChatScroller.ScrollToEnd();

            string built = "";
            foreach (char c in full)
            {
                built += c;
                msgText.Text = built;
                if (c == '\n') ChatScroller.ScrollToEnd();
                await Task.Delay(11);
            }

            ChatScroller.ScrollToEnd();
            _isTyping = false;
            BtnSend.IsEnabled = TxtInput.Text.Trim().Length > 0;
        }

        private (Border bubble, TextBlock msgText) MakeBotBubble(string text, bool isWelcome)
        {
            var msgText = MakeMessageText(text, isWelcome ? BrGreen : BrText);
            var inner = new StackPanel();
            inner.Children.Add(MakeLabel("CyberSecBot", BrCyan, bold: true));
            inner.Children.Add(msgText);

            var bubble = new Border
            {
                Background = BrBotBg,
                BorderBrush = BrBotBord,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 4, 80, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = inner
            };
            return (bubble, msgText);
        }

        private void UpdateSentimentDisplay(string sentiment)
        {
            switch (sentiment)
            {
                case "worried": TxtSentiment.Text = "Mood: worried"; TxtSentiment.Foreground = BrRed; break;
                case "frustrated": TxtSentiment.Text = "Mood: frustrated"; TxtSentiment.Foreground = BrYellow; break;
                case "curious": TxtSentiment.Text = "Mood: curious"; TxtSentiment.Foreground = BrCyan; break;
                case "positive": TxtSentiment.Text = "Mood: positive"; TxtSentiment.Foreground = BrGreen; break;
                default: TxtSentiment.Text = ""; break;
            }
        }

        private void UpdateMemoryPanel()
        {
            if (_memory.InterestedTopics.Count == 0) return;
            MemoryPanel.Visibility = Visibility.Visible;
            TxtMemory.Text = $"Topics: {string.Join("  •  ", _memory.InterestedTopics)}";
            if (!string.IsNullOrEmpty(_memory.FavouriteTopic))
                TxtMemory.Text += $"   |   First interest: {_memory.FavouriteTopic}";
        }

        // ─── TextBlock / label factories ──────────────────────────────────────
        private TextBlock MakeLabel(string text, SolidColorBrush colour, bool bold = false)
            => new TextBlock
            {
                Text = text,
                Foreground = colour,
                FontFamily = Mono,
                FontSize = 10,
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                Margin = new Thickness(0, 0, 0, 4)
            };

        private TextBlock MakeMessageText(string text, SolidColorBrush colour)
            => new TextBlock
            {
                Text = text,
                Foreground = colour,
                FontFamily = Mono,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

        private static SolidColorBrush B(string hex)
            => new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    }

}