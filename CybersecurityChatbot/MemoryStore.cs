using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbot
{
    public class MemoryStore
    {
        //These properties are used to store user details and conversation context for the session.
        public string UserName { get; set; } = "";
        public string FavouriteTopic { get; set; } = "";
        public string LastTopic { get; set; } = "";
        public string LastSentiment { get; set; } = "";
        public int MessageCount { get; set; } = 0;

        // All topics the user has engaged with this session
        public List<string> InterestedTopics { get; private set; } = new List<string>();

        public void RecordTopicInterest(string topic)
        {
            if (!InterestedTopics.Contains(topic))
                InterestedTopics.Add(topic);

            // First topic engaged becomes the "favourite" for recall purposes
            if (string.IsNullOrEmpty(FavouriteTopic))
                FavouriteTopic = topic;

            LastTopic = topic;
        }

        // Records the sentiment of the user's last message for potential recall.
        public string GetRecallLine()
        {
            if (!string.IsNullOrEmpty(FavouriteTopic))
                return $"As someone interested in {FavouriteTopic}, this is especially relevant for you.\n\n";

            return "";
        }
        // Method to reset the memory store, clearing all stored information. This can be called at the end of a session or when starting fresh.
        public void Reset()
        {
            FavouriteTopic = "";
            LastTopic = "";
            LastSentiment = "";
            MessageCount = 0;
            InterestedTopics.Clear();
        }

    }
}
