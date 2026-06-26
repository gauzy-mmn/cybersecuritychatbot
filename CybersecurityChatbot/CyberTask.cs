using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbot
{

    /// <summary>
    /// Represents a single cybersecurity task stored in the MySQL database.
    /// </summary>

    public class CyberTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? ReminderDate { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Returns a friendly one-line summary for display in the task list.
        /// </summary>
        public string Summary()
        {
            string status = IsCompleted ? "[Done]" : "[Pending]";
            string reminder = ReminderDate.HasValue
                ? $"  Reminder: {ReminderDate.Value:dd MMM yyyy}"
                : "";
            return $"{status} {Title}{reminder}";
        }

    }
}
