using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CybersecurityChatbot
{
    public static class Nlpprocessor
    {

        // ── Intent → keyword patterns dictionary ──────────────────────────────
        private static readonly Dictionary<string, List<string>> IntentPatterns =
            new Dictionary<string, List<string>>
        {
            {
                "add_task", new List<string>
                {
                    "add task", "add a task", "create task", "create a task",
                    "new task", "i need to", "i have to", "set a task",
                    "make a task", "log a task", "save a task"
                }
            },
            {
                "set_reminder", new List<string>
                {
                    "remind me", "set a reminder", "set reminder",
                    "don't let me forget", "dont let me forget",
                    "reminder for", "remind me to", "notify me"
                }
            },
            {
                "view_tasks", new List<string>
                {
                    "show tasks", "show my tasks", "view tasks", "list tasks",
                    "what tasks", "my tasks", "see my tasks", "display tasks",
                    "what do i have to do", "pending tasks"
                }
            },
            {
                "complete_task", new List<string>
                {
                    "mark done", "mark as done", "mark complete",
                    "mark as complete", "i finished", "i completed",
                    "task done", "completed task", "finish task"
                }
            },
            {
                "delete_task", new List<string>
                {
                    "delete task", "remove task", "cancel task",
                    "get rid of task", "delete the task", "remove the task"
                }
            },
            {
                "start_quiz", new List<string>
                {
                    "start quiz", "begin quiz", "take quiz", "quiz me",
                    "test me", "test my knowledge", "play quiz",
                    "cybersecurity quiz", "start the quiz", "i want a quiz"
                }
            },
            {
                "show_log", new List<string>
                {
                    "show log", "activity log", "show activity",
                    "what have you done", "what did you do",
                    "recent actions", "show history", "your actions",
                    "what actions", "log please"
                }
            }
        };

        // ── Reminder time extraction patterns ─────────────────────────────────
        // Extracts "in X days/weeks" or "tomorrow" from user input
        private static readonly Dictionary<string, int> TimePhrases =
            new Dictionary<string, int>
        {
            { "tomorrow",   1  },
            { "in 1 day",   1  },
            { "in 2 days",  2  },
            { "in 3 days",  3  },
            { "in 4 days",  4  },
            { "in 5 days",  5  },
            { "in 6 days",  6  },
            { "in 7 days",  7  },
            { "in a week",  7  },
            { "in 1 week",  7  },
            { "in 2 weeks", 14 },
            { "in a month", 30 },
            { "next week",  7  },
        };

        // ── Main detection method ─────────────────────────────────────────────

        /// <summary>
        /// Detects the user's intent from natural language input.
        /// Returns the intent string, or "unknown" if nothing matches.
        /// </summary>
        public static string DetectIntent(string input)
        {
            string lower = input.ToLower().Trim();

            foreach (var intent in IntentPatterns)
            {
                foreach (var pattern in intent.Value)
                {
                    if (lower.Contains(pattern))
                    {
                        ActivityLog.LogNLPAction(intent.Key, $"Triggered by: '{pattern}'");
                        return intent.Key;
                    }
                }
            }

            return "unknown";
        }

        /// <summary>
        /// Extracts a task title from natural language.
        /// Strips common command words to isolate the meaningful content.
        /// e.g. "Add a task to enable 2FA" → "enable 2FA"
        /// </summary>
        public static string ExtractTaskTitle(string input)
        {
            string lower = input.ToLower();

            // Strip common command prefixes
            string[] prefixesToRemove =
            {
                "add a task to", "add a task for", "add task to", "add task for",
                "create a task to", "create task to", "new task to", "new task for",
                "i need to", "i have to", "remind me to", "remind me about",
                "set a reminder to", "set reminder to", "don't let me forget to",
                "add a task", "create a task", "add task", "create task",
                "i need", "i have"
            };

            string result = input.Trim();
            foreach (var prefix in prefixesToRemove)
            {
                if (lower.StartsWith(prefix))
                {
                    result = input.Substring(prefix.Length).Trim();
                    break;
                }
            }

            // Capitalise first letter
            if (result.Length > 0)
                result = char.ToUpper(result[0]) + result.Substring(1);

            return result.Length >= 3 ? result : input.Trim();
        }

        /// <summary>
        /// Extracts a reminder date from natural language.
        /// Returns null if no time phrase is detected.
        /// e.g. "in 3 days" → DateTime.Now.AddDays(3)
        ///      "tomorrow"  → DateTime.Now.AddDays(1)
        /// </summary>
        public static DateTime? ExtractReminderDate(string input)
        {
            string lower = input.ToLower();

            // Check named time phrases
            foreach (var phrase in TimePhrases)
            {
                if (lower.Contains(phrase.Key))
                    return DateTime.Now.AddDays(phrase.Value);
            }

            // Regex: "in X days" or "in X weeks" for any number
            var match = Regex.Match(lower, @"in (\d+) (day|days|week|weeks)");
            if (match.Success)
            {
                int amount = int.Parse(match.Groups[1].Value);
                bool isWeek = match.Groups[2].Value.StartsWith("week");
                return DateTime.Now.AddDays(isWeek ? amount * 7 : amount);
            }

            return null;
        }

        /// <summary>
        /// Builds a human-readable description for a cybersecurity task title.
        /// Provides context-specific descriptions for common task keywords.
        /// </summary>
        public static string BuildTaskDescription(string title)
        {
            string lower = title.ToLower();

            if (lower.Contains("2fa") || lower.Contains("two-factor") || lower.Contains("two factor"))
                return "Enable two-factor authentication on your accounts to add a critical second layer of security.";
            if (lower.Contains("password"))
                return "Review and update your passwords using a password manager to ensure they are strong and unique.";
            if (lower.Contains("privacy"))
                return "Review your account privacy settings to ensure your personal data is properly protected.";
            if (lower.Contains("antivirus") || lower.Contains("anti-virus"))
                return "Install or update your antivirus software to protect against malware and viruses.";
            if (lower.Contains("backup"))
                return "Back up your important files to a secure location — both cloud and offline copies are recommended.";
            if (lower.Contains("update") || lower.Contains("patch"))
                return "Apply the latest security updates and patches to your operating system and applications.";
            if (lower.Contains("vpn"))
                return "Set up a VPN to protect your internet connection, especially on public Wi-Fi networks.";

            // Generic fallback
            return $"Complete the cybersecurity task: {title}";
        }

    }
}
