using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbot
{
    public static class ActivityLog
    {

        private static readonly List<(DateTime Time, string Description)> Entries
            = new List<(DateTime, string)>();

        private const int RecentCount = 10;

        public static void Log(string description)
        {
            Entries.Add((DateTime.Now, description));
        }

        public static void LogTaskAdded(string title, DateTime? reminder)
        {
            string rem = reminder.HasValue
                ? $" (Reminder: {reminder.Value:dd MMM yyyy})"
                : " (No reminder)";
            Log($"Task added: '{title}'{rem}");
        }

        public static void LogTaskCompleted(string title)
        {
            Log($"Task completed: '{title}'");
        }

        public static void LogTaskDeleted(string title)
        {
            Log($"Task deleted: '{title}'");
        }

        public static void LogReminderSet(string title, DateTime date)
        {
            Log($"Reminder set for '{title}' on {date:dd MMM yyyy}");
        }

        public static void LogQuizStarted()
        {
            Log("Quiz started");
        }

        public static void LogQuizCompleted(int score, int total)
        {
            Log($"Quiz completed — Score: {score}/{total}");
        }

        public static void LogNLPAction(string intent, string detail)
        {
            Log($"NLP intent '{intent}': {detail}");
        }

        public static string GetRecentSummary()
        {
            if (Entries.Count == 0)
                return "No actions recorded yet this session.";

            var recent = Entries
                .OrderByDescending(e => e.Time)
                .Take(RecentCount)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"Recent activity ({recent.Count} actions):\n");
            for (int i = 0; i < recent.Count; i++)
                sb.AppendLine($"  {i + 1}. [{recent[i].Time:HH:mm:ss}] {recent[i].Description}");

            return sb.ToString().TrimEnd();
        }

        public static List<(DateTime Time, string Description)> GetAll()
        {
            return Entries.OrderByDescending(e => e.Time).ToList();
        }

        public static int TotalCount => Entries.Count;

    }
}
