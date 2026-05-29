using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;

namespace CybersecurityChatbot
{
    internal class Chatbot
    {
        public String BotName { get; private set; }
        public String UserName { get; set; }
        public String Version { get; private set; }

        public Chatbot()
        {
            BotName = "CyberSecBot";
            UserName = "";
            Version = "2.0";
        }

        public void PlayVoiceGreeting()
        {
            try
            {
                SoundPlayer player = new SoundPlayer("greeting.wav");
                player.Play();
            }
            catch
            {
                // If the audio file is missing or cannot be played, we silently ignore the error
            }
        }

        public string WelcomeMessage()
        {
            return $"Welcome to {BotName} v{Version} — your Cybersecurity Awareness Assistant.\n\n" +
                   "I'm here to help you stay safe online.\n\n" +
                   "Before we begin, what is your name?";
        }

        public string GreetingMessage(string userName)
        {
            return $"Hello, {userName}! I'm {BotName}, your Cybersecurity Awareness Assistant.\n\n" +
                  "I'm here to help you stay safe online. You can ask me about:\n\n" +
                  "  • passwords             • phishing / scams\n" +
                  "  • privacy               • safe browsing\n" +
                  "  • malware               • social engineering\n" +
                  "  • two-factor authentication (2FA)\n\n" +
                  "Type 'help' to see all topics, or 'exit' to quit.";
        }

    }
}
