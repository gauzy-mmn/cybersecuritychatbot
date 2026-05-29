using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbot
{
    public static class KeywordResponder
    {
        /// A dictionary mapping keywords to lists of possible responses.

        private static readonly Random RNum = new Random();

        // Each topic has a list of 5 tips. When the user asks about a topic, we select one tip at random to display.
        private static readonly List<string> PasswordTips = new List<string>
        {
            "Password Tip:\nUse at least 12 characters and mix uppercase, lowercase, " +
            "numbers and symbols.\nExample: MyD0g!sC@lledBob2024\n\n" +
            "Longer passwords are exponentially harder for attackers to crack.",

            "Password Tip:\nNever reuse the same password across multiple websites.\n\n" +
            "If one site is breached, attackers will try that password on every other account you own. " +
            "Use a password manager like Bitwarden to generate unique passwords for each site.",

            "Password Tip:\nAvoid using personal details in your passwords — " +
            "like your name, birthday, or pet's name.\n\n" +
            "These are the very first things an attacker tries when specifically targeting you.",

            "Password Tip:\nA password manager stores and generates strong passwords for you. " +
            "You only need to remember one master password.\n\n" +
            "Recommended options: Bitwarden (free and open source), 1Password, or KeePass.",

            "Password Tip:\nChange your password immediately if a service you use reports a breach.\n\n" +
            "Visit haveibeenpwned.com to check whether your email has appeared in known data breaches."
        };

        private static readonly List<string> PhishingTips = new List<string>
        {
            "Phishing Tip:\nBe cautious of emails asking for personal information. " +
            "Scammers disguise themselves as trusted organisations like SARS, your bank, or Telkom.\n\n" +
            "Always verify the sender's full email address carefully before clicking anything.",

            "Phishing Tip:\nWatch for urgent language like 'Your account will be closed!' — " +
            "this pressure tactic is a classic phishing red flag.\n\n" +
            "Legitimate companies will never threaten you into acting immediately via email.",

            "Phishing Tip:\nHover your mouse over any link before clicking it. " +
            "If the URL looks unusual or doesn't match the company's real domain, do not click.\n\n" +
            "When in doubt, type the website address directly into your browser yourself.",

            "Phishing Tip:\nReal banks and government departments will NEVER ask " +
            "for your password, PIN, or OTP via email or SMS.\n\n" +
            "If you receive such a message, contact the organisation directly through their official number.",

            "Phishing Tip:\nSpear phishing targets you personally — using your name, " +
            "employer, or recent purchases to appear legitimate.\n\n" +
            "Even emails that feel personal can be fakes. Always verify through official channels."
        };

        private static readonly List<string> PrivacyTips = new List<string>
        {
            "Privacy Tip:\nReview the privacy settings on all your social media accounts. " +
            "Limit who can see your posts, location tags, and contact details.\n\n" +
            "Oversharing publicly gives scammers the information they need to target you.",

            "Privacy Tip:\nBe careful what personal information you share online. " +
            "Your full name, ID number, address, and phone number can all be used for identity theft.\n\n" +
            "Think before you post — once it is online, it is very difficult to remove.",

            "Privacy Tip:\nUse a VPN (Virtual Private Network) on public Wi-Fi. " +
            "It encrypts your internet traffic so others on the same network cannot spy on you.\n\n" +
            "Recommended options: ProtonVPN (has a free tier) or Mullvad.",

            "Privacy Tip:\nCheck which apps have permission to access your camera, " +
            "microphone, and location on your phone.\n\n" +
            "Revoke access for any app that does not genuinely need it.",

            "Privacy Tip:\nConsider using DuckDuckGo instead of Google for sensitive searches.\n\n" +
            "DuckDuckGo does not track, store, or sell your search history."
        };

        private static readonly List<string> SafeBrowsingTips = new List<string>
        {
            "Safe Browsing Tip:\nOnly visit websites that start with https:// — " +
            "the 's' stands for secure and means your connection is encrypted.\n\n" +
            "Look for the padlock icon in your browser's address bar as confirmation.",

            "Safe Browsing Tip:\nAvoid using public Wi-Fi at airports or cafes " +
            "for banking or entering personal information.\n\n" +
            "If you must use public Wi-Fi, connect through a VPN to protect your data.",

            "Safe Browsing Tip:\nKeep your browser updated at all times. " +
            "Updates patch known security vulnerabilities that attackers actively exploit.\n\n" +
            "Enable automatic updates so you are always protected.",

            "Safe Browsing Tip:\nInstall an ad blocker like uBlock Origin in your browser.\n\n" +
            "Malicious advertisements (malvertising) can install malware on your device " +
            "simply by being displayed on a page — even legitimate websites can carry them.",

            "Safe Browsing Tip:\nNever download files or software from websites you do not fully trust.\n\n" +
            "Always download applications from the developer's official website or " +
            "a trusted app store like the Microsoft Store or Google Play."
        };

        // This dictionary gets populated with all the keywords for each topic, mapping them to the topic name.
        private static readonly Dictionary<string, string> KeywordTopicMap =
            new Dictionary<string, string>
        {
            // Passwords Keywords
            { "password",     "password" },
            { "passwords",    "password" },
            { "passphrase",   "password" },
            { "credentials",  "password" },
            { "pin",          "password" },
            {"pass",         "password" },

            // Phishing / Scams keywords
            { "phish",        "phishing" },
            { "phishing",     "phishing" },
            { "scam",         "phishing" },
            { "spam",         "phishing" },
            { "fake email",   "phishing" },
            { "smishing",     "phishing" },

            // Privacy keywords
            { "privacy",      "privacy"  },
            { "private",      "privacy"  },
            { "personal data","privacy"  },
            { "tracking",     "privacy"  },
            { "surveillance", "privacy"  },

            // Safe browsing keywords
            { "brows",        "browsing" },
            { "browser",      "browsing" },
            { "internet",     "browsing" },
            { "website",      "browsing" },
            { "https",        "browsing" },
            { "online safety","browsing" },

            // Malware keywords
            { "malware",      "malware"  },
            { "virus",        "malware"  },
            { "ransomware",   "malware"  },
            { "spyware",      "malware"  },
            { "trojan",       "malware"  },
            { "antivirus",    "malware"  },
            { "infected",     "malware"  },

            // Social engineering keywords
            { "social engineer",  "socialengineering" },
            { "manipulat",        "socialengineering" },
            { "pretexting",       "socialengineering" },
            { "baiting",          "socialengineering" },
            { "vishing",          "socialengineering" },
            { "impersonat",       "socialengineering" },

            // Two-factor authentication keywords
            { "2fa",              "2fa" },
            { "two factor",       "2fa" },
            { "two-factor",       "2fa" },
            { "authentication",   "2fa" },
            { "otp",              "2fa" },
            { "authenticator",    "2fa" },
            { "one-time",         "2fa" },
        };

        //This list contains phrases that indicate the user wants a follow-up tip on the same topic, without mentioning the topic again.
        private static readonly List<string> FollowUpPhrases = new List<string>
        {
            "tell me more", "more", "another tip", "give me another",
            "explain more", "go on", "continue", "what else",
            "more info", "more details", "elaborate", "keep going",
            "next tip", "another one", "more please", "expand"
        };

        //These methods are used to check if the user's input matches any of the follow-up phrases, and to generate a response based on the detected topic and sentiment.
        public static string GetResponse(string input, MemoryStore memory)
        {
            string lower = InputValidator.Sanitise(input);
            memory.MessageCount++;

            // 1. Detect sentiment and build empathy prefix to make the conversation between the user and bot seem natural and engaging
            string sentiment = SentimentDetector.Detect(lower);
            memory.LastSentiment = sentiment;
            string empathy = SentimentDetector.GetEmpathyLine(sentiment, memory.UserName);

            // 2. Follow-up: continue the last topic without re-detecting keyword
            if (IsFollowUp(lower) && !string.IsNullOrEmpty(memory.LastTopic))
                return empathy + GetTopicResponse(memory.LastTopic, memory, isFollowUp: true);

            // 3. General queries (greetings, meta questions, help)
            string generalResponse = HandleGeneral(lower, memory);
            if (generalResponse != null)
                return empathy + generalResponse;

            // 4. Keyword topic matching
            string detectedTopic = DetectTopic(lower);
            if (detectedTopic != null)
            {
                memory.RecordTopicInterest(TopicDisplayName(detectedTopic));
                return empathy + GetTopicResponse(detectedTopic, memory, isFollowUp: false);
            }

            // 5. Fallback (When the users enters keywords that don't match any topic, or if the input is unclear)
            return $"I'm not sure I understand that, {memory.UserName}.\n\n" +
                   "Try asking me about: passwords, phishing, privacy, malware, " +
                   "safe browsing, social engineering, or two-factor authentication.\n\n" +
                   "Type 'help' to see the full list of topics.";
        }

        //This method checks if the user's input matches any of the predefined follow-up phrases, indicating they want more information on the last topic discussed.
        private static string HandleGeneral(string lower, MemoryStore memory)
        {
            string name = memory.UserName;

            if (lower.Contains("how are you") || lower.Contains("how r you"))
                return $"I'm running at full security capacity, {name}!\n" +
                       "How can I help you stay safe online today?";

            if (lower.Contains("your name") || lower.Contains("who are you"))
                return "I am CyberSecBot, your Cybersecurity Awareness Assistant.\n" +
                       "I'm here to help South Africans stay safer online — one conversation at a time.";

            if (lower.Contains("purpose") || lower.Contains("what can you do") || lower.Contains("what can i ask"))
                return "My purpose is to educate you on cybersecurity threats and best practices.\n\n" +
                       "I can help you with:\n" +
                       "  • Passwords\n" +
                       "  • Phishing & Scams\n" +
                       "  • Privacy\n" +
                       "  • Safe Browsing\n" +
                       "  • Malware\n" +
                       "  • Social Engineering\n" +
                       "  • Two-Factor Authentication (2FA)\n\n" +
                       "Type 'help' to see the full topic list at any time.";

            if (lower.Contains("thank"))
            {
                string topics = memory.InterestedTopics.Count > 0
                    ? $"\n\nTopics you've explored so far: {string.Join(", ", memory.InterestedTopics)}."
                    : "";
                return $"You're welcome, {name}! Staying informed is your best defence against cybercrime.{topics}";
            }

            if (lower == "hello" || lower == "hi" ||
                lower.StartsWith("hello ") || lower.StartsWith("hi "))
                return $"Hello again, {name}! What cybersecurity topic would you like to explore?";

            if (lower.Contains("help"))
                return "Here are the topics I can help you with:\n\n" +
                       "  • passwords             • phishing / scams\n" +
                       "  • privacy               • safe browsing\n" +
                       "  • malware               • social engineering\n" +
                       "  • 2fa / authentication\n\n" +
                       "Just ask me anything about these topics — or type 'exit' to quit.";

            // Periodic memory recall every 6 messages
            if (memory.MessageCount % 6 == 0 && !string.IsNullOrEmpty(memory.FavouriteTopic))
                return $"By the way, {name} — since you've been exploring {memory.FavouriteTopic}, " +
                       "here's another tip you might find useful:\n\n" +
                       GetRandomTipForTopic(memory.LastTopic);

            return null; // Not a general query — proceed to keyword matching
        }

        //This method detects the topic of the user's input by checking for keywords. It returns the topic name if found, or null if no topic is detected.
        private static string DetectTopic(string lower)
        {
            foreach (var entry in KeywordTopicMap)
            {
                if (lower.Contains(entry.Key))
                    return entry.Value;
            }
            return null;
        }

        //This method checks if the user's input matches any of the predefined follow-up phrases, indicating they want more information on the last topic discussed.
        private static string GetTopicResponse(string topic, MemoryStore memory, bool isFollowUp)
        {
            memory.LastTopic = topic;
            string recall = memory.GetRecallLine();
            string followUpNote = isFollowUp ? "Here's more on that topic:\n\n" : "";

            switch (topic)
            {
                case "password":
                    memory.RecordTopicInterest("passwords");
                    return followUpNote + recall + GetRandomItem(PasswordTips) +
                           "\n\nI've noted that you're interested in passwords. " +
                           "Ask me for 'another tip' anytime!";

                case "phishing":
                    memory.RecordTopicInterest("phishing");
                    return followUpNote + recall + GetRandomItem(PhishingTips) +
                           "\n\nWant another phishing tip? Just ask!";

                case "privacy":
                    memory.RecordTopicInterest("privacy");
                    return followUpNote + recall + GetRandomItem(PrivacyTips) +
                           "\n\nI'll remember that privacy matters to you.";

                case "browsing":
                    memory.RecordTopicInterest("safe browsing");
                    return followUpNote + recall + GetRandomItem(SafeBrowsingTips) +
                           "\n\nAsk me for 'another tip' to get more safe browsing advice.";

                case "malware":
                    memory.RecordTopicInterest("malware");
                    return followUpNote + recall + MalwareResponse();

                case "socialengineering":
                    memory.RecordTopicInterest("social engineering");
                    return followUpNote + recall + SocialEngineeringResponse();

                case "2fa":
                    memory.RecordTopicInterest("two-factor authentication");
                    return followUpNote + recall + TwoFactorResponse();

                default:
                    return "I'm not sure about that specific topic. " +
                           "Type 'help' to see what I can assist with.";
            }
        }

        //This method checks if the user's input matches any of the predefined follow-up phrases, indicating they want more information on the last topic discussed.
        private static bool IsFollowUp(string lower)
        {
            return FollowUpPhrases.Any(phrase => lower.Contains(phrase));
        }

        private static string GetRandomItem(List<string> list)
        {
            return list[RNum.Next(list.Count)];
        }

        private static string GetRandomTipForTopic(string topic)
        {
            switch (topic)
            {
                case "password": return GetRandomItem(PasswordTips);
                case "phishing": return GetRandomItem(PhishingTips);
                case "privacy": return GetRandomItem(PrivacyTips);
                case "browsing": return GetRandomItem(SafeBrowsingTips);
                default: return GetTopicResponse(topic, new MemoryStore(), false);
            }
        }

        private static string TopicDisplayName(string topic)
        {
            switch (topic)
            {
                case "password": return "passwords";
                case "phishing": return "phishing";
                case "privacy": return "privacy";
                case "browsing": return "safe browsing";
                case "malware": return "malware";
                case "socialengineering": return "social engineering";
                case "2fa": return "two-factor authentication";
                default: return topic;
            }
        }

        private static string MalwareResponse()
        {
            return "Malware — What You Need to Know:\n\n" +
                   "Common types:\n" +
                   "  • Virus      — spreads by attaching to files and programmes.\n" +
                   "  • Ransomware — encrypts your files and demands payment to unlock them.\n" +
                   "  • Spyware    — secretly records your keystrokes and activity.\n" +
                   "  • Trojan     — disguises itself as legitimate software.\n\n" +
                   "How to protect yourself:\n" +
                   "  1. Install reputable antivirus software and keep it updated.\n" +
                   "  2. Never open email attachments from unknown senders.\n" +
                   "  3. Only download software from official, trusted sources.\n" +
                   "  4. Back up your important files regularly — both locally and in the cloud.\n" +
                   "     If ransomware strikes, backups mean you won't lose everything.\n" +
                   "  5. Keep your operating system and apps fully updated at all times.\n\n" +
                   "Ask me 'tell me more' for additional malware prevention advice.";
        }

        private static string SocialEngineeringResponse()
        {
            return "Social Engineering — Hacking People, Not Technology:\n\n" +
                   "Instead of breaking into systems, criminals manipulate people directly.\n\n" +
                   "Common tactics:\n" +
                   "  • Pretexting — creating a fake scenario to earn your trust.\n" +
                   "    Example: 'I'm from IT support. I need your password to fix a critical issue.'\n\n" +
                   "  • Baiting    — leaving a USB drive labelled 'Salary Increases 2024'\n" +
                   "    in a car park, hoping a curious employee will plug it in.\n\n" +
                   "  • Vishing    — voice phishing via scam phone calls.\n" +
                   "    Attackers pose as bank staff, SARS officials, or tech support.\n\n" +
                   "How to protect yourself:\n" +
                   "  1. Always verify a caller's or visitor's identity before sharing anything.\n" +
                   "  2. Legitimate IT staff will NEVER ask for your password.\n" +
                   "  3. Trust your instincts — if something feels wrong, it probably is.\n" +
                   "  4. Report suspicious contact to your IT or security team immediately.\n\n" +
                   "Ask me 'tell me more' to continue exploring this topic.";
        }

        private static string TwoFactorResponse()
        {
            return "Two-Factor Authentication (2FA):\n\n" +
                   "2FA adds a second layer of security on top of your password.\n\n" +
                   "How it works — you need TWO things to log in:\n" +
                   "  1. Something you KNOW  — your password.\n" +
                   "  2. Something you HAVE  — a one-time code sent to your phone\n" +
                   "     or generated by an authenticator app.\n\n" +
                   "Why 2FA is so important:\n" +
                   "Even if a criminal steals your password, they still cannot log in\n" +
                   "without also having physical access to your phone.\n" +
                   "This single step blocks the vast majority of account takeover attacks.\n\n" +
                   "How to set it up:\n" +
                   "  1. Go to the security settings on your account (Gmail, banking, etc.).\n" +
                   "  2. Enable '2FA' or 'Two-Step Verification'.\n" +
                   "  3. Use an authenticator app — it is more secure than SMS codes.\n" +
                   "     Recommended apps: Google Authenticator, Microsoft Authenticator.\n\n" +
                   "Ask me 'tell me more' for more 2FA guidance.";
        }

    }
}
