using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbot
{
    public static class SentimentDetector
    {

        private static readonly Dictionary<string, List<string>> SentimentKeywords =
           new Dictionary<string, List<string>>
       {
            {
                "worried", new List<string>
                {
                    "worried", "scared", "afraid", "anxious", "nervous", "fear",
                    "unsafe", "concerned", "panic", "stressed", "terrified",
                    "uneasy", "overwhelmed", "hacked", "been hacked", "got hacked"
                }
            },
            {
                "frustrated", new List<string>
                {
                    "frustrated", "annoyed", "angry", "useless", "doesn't work",
                    "not working", "stupid", "confused", "confusing", "dont understand",
                    "don't understand", "makes no sense", "complicated", "difficult"
                }
            },
            {
                "curious", new List<string>
                {
                    "curious", "interested", "wondering", "want to know", "tell me more",
                    "explain", "how does", "what is", "why does", "can you explain",
                    "i want to learn", "teach me", "how do i", "what are"
                }
            },
            {
                "positive", new List<string>
                {
                    "great", "awesome", "thanks", "thank you", "helpful", "love it",
                    "good", "excellent", "perfect", "amazing", "brilliant", "nice"
                }
            }
       };

        public static string Detect(string input)
        {
            string lower = input.ToLower();

            foreach (var sentiment in SentimentKeywords)
            {
                foreach (var keyword in sentiment.Value)
                {
                    if (lower.Contains(keyword))
                        return sentiment.Key;
                }
            }

            return "neutral";
        }

        public static string GetEmpathyLine(string sentiment, string userName)
        {
            switch (sentiment)
            {
                case "worried":
                    return $"I completely understand your concern, {userName}. " +
                           "It's natural to feel worried — cybersecurity threats are real, " +
                           "but knowledge is your best defence.\n\n";

                case "frustrated":
                    return $"I hear you, {userName}. Cybersecurity can feel overwhelming at first. " +
                           "Let me break this down as clearly as possible.\n\n";

                case "curious":
                    return $"Great question, {userName}! Wanting to learn is the first step " +
                           "to staying safe online.\n\n";

                case "positive":
                    return $"Thank you, {userName}! I'm glad I could help.\n\n";

                default:
                    return "";
            }
        }

    }
}
