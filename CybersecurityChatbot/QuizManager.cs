using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbot
{
    internal class QuizManager
    {
        // ── Question model ────────────────────────────────────────────────────
        public class QuizQuestion
        {
            public string QuestionText { get; set; }
            public List<string> Options { get; set; }   // null = true/false
            public int CorrectIndex { get; set; }   // 0-based index
            public string Explanation { get; set; }
            public bool IsTrueFalse { get; set; }
        }

        // ── State ─────────────────────────────────────────────────────────────
        public int CurrentIndex { get; private set; } = 0;
        public int Score { get; private set; } = 0;
        public bool IsActive { get; private set; } = false;
        public bool IsFinished => CurrentIndex >= _questions.Count;
        public int TotalQuestions => _questions.Count;

        private readonly List<QuizQuestion> _questions;

        public QuizManager()
        {
            _questions = BuildQuestions();
        }

        // ── Question bank (12 questions) ──────────────────────────────────────
        private List<QuizQuestion> BuildQuestions()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    QuestionText = "What should you do if you receive an email asking for your password?",
                    Options      = new List<string> { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                    CorrectIndex = 2,
                    Explanation  = "Reporting phishing emails helps protect you and others. Legitimate organisations never ask for passwords via email."
                },
                new QuizQuestion
                {
                    QuestionText = "True or False: Using the same password on multiple websites is safe if the password is strong.",
                    IsTrueFalse  = true,
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "False. If one site is breached, attackers will try that password on every other site — this is called credential stuffing."
                },
                new QuizQuestion
                {
                    QuestionText = "What does 'https' in a website URL indicate?",
                    Options      = new List<string> { "The website is popular", "The connection is encrypted and secure", "The website is owned by the government", "The website loads faster" },
                    CorrectIndex = 1,
                    Explanation  = "HTTPS means your connection to the website is encrypted using SSL/TLS, protecting your data in transit."
                },
                new QuizQuestion
                {
                    QuestionText = "What is phishing?",
                    Options      = new List<string> { "A type of malware that deletes files", "A sport involving fish", "A scam where attackers pose as trusted organisations to steal information", "A tool used by IT departments" },
                    CorrectIndex = 2,
                    Explanation  = "Phishing attacks use fake emails, SMS, or websites to trick users into revealing sensitive information."
                },
                new QuizQuestion
                {
                    QuestionText = "True or False: Two-factor authentication (2FA) makes your account completely unhackable.",
                    IsTrueFalse  = true,
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "False. 2FA greatly reduces risk but no system is completely unhackable. It is still a critical security layer to enable."
                },
                new QuizQuestion
                {
                    QuestionText = "Which of the following is the strongest password?",
                    Options      = new List<string> { "password123", "MyDog2010", "X#9kL!mQ2@wZ", "qwerty" },
                    CorrectIndex = 2,
                    Explanation  = "A strong password uses a random mix of uppercase, lowercase, numbers, and symbols with no recognisable words."
                },
                new QuizQuestion
                {
                    QuestionText = "What is ransomware?",
                    Options      = new List<string> { "Software that speeds up your computer", "Malware that encrypts your files and demands payment to restore them", "A type of antivirus programme", "A firewall configuration tool" },
                    CorrectIndex = 1,
                    Explanation  = "Ransomware locks your files and demands a ransom — usually in cryptocurrency — to unlock them. Regular backups are your best defence."
                },
                new QuizQuestion
                {
                    QuestionText = "True or False: It is safe to use public Wi-Fi for online banking if the website uses HTTPS.",
                    IsTrueFalse  = true,
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "False. Public Wi-Fi exposes you to man-in-the-middle attacks and network sniffing. Always use a VPN or mobile data for banking."
                },
                new QuizQuestion
                {
                    QuestionText = "What is social engineering in cybersecurity?",
                    Options      = new List<string> { "Building social media platforms", "Manipulating people into revealing confidential information", "Writing social media bots", "Engineering social network algorithms" },
                    CorrectIndex = 1,
                    Explanation  = "Social engineering exploits human psychology rather than technical vulnerabilities — for example, impersonating IT support to get your password."
                },
                new QuizQuestion
                {
                    QuestionText = "Which action best protects you against malware?",
                    Options      = new List<string> { "Disabling your firewall", "Downloading software from any website", "Keeping your operating system and antivirus updated", "Opening all email attachments" },
                    CorrectIndex = 2,
                    Explanation  = "Regular updates patch known security vulnerabilities. Outdated systems are one of the most common entry points for malware."
                },
                new QuizQuestion
                {
                    QuestionText = "True or False: An authenticator app is more secure than receiving OTPs via SMS.",
                    IsTrueFalse  = true,
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 0,
                    Explanation  = "True. SMS can be intercepted via SIM-swapping attacks. Authenticator apps like Google Authenticator generate codes locally and are far more secure."
                },
                new QuizQuestion
                {
                    QuestionText = "What should you do if you suspect your account has been hacked?",
                    Options      = new List<string> { "Wait and see what happens", "Change your password immediately and enable 2FA", "Delete the account", "Ignore it" },
                    CorrectIndex = 1,
                    Explanation  = "Act fast — change your password immediately, enable 2FA, review recent activity, and alert the service provider if needed."
                }
            };
        }

        // ── Control methods ───────────────────────────────────────────────────

        public void Start()
        {
            CurrentIndex = 0;
            Score = 0;
            IsActive = true;
            ActivityLog.LogQuizStarted();
        }

        public void Reset()
        {
            CurrentIndex = 0;
            Score = 0;
            IsActive = false;
        }

        /// <summary>
        /// Returns the current question, or null if the quiz is finished.
        /// </summary>
        public QuizQuestion GetCurrentQuestion()
        {
            if (IsFinished) return null;
            return _questions[CurrentIndex];
        }

        /// <summary>
        /// Submits an answer (0-based index). Returns true if correct.
        /// Advances the question index automatically.
        /// </summary>
        public bool SubmitAnswer(int selectedIndex)
        {
            if (IsFinished) return false;

            bool correct = selectedIndex == _questions[CurrentIndex].CorrectIndex;
            if (correct) Score++;
            CurrentIndex++;

            if (IsFinished)
            {
                IsActive = false;
                ActivityLog.LogQuizCompleted(Score, TotalQuestions);
            }

            return correct;
        }

        /// <summary>
        /// Returns a final score message based on how well the user did.
        /// </summary>
        public string GetFinalFeedback()
        {
            double pct = (double)Score / TotalQuestions * 100;

            if (pct == 100)
                return $"Perfect score! {Score}/{TotalQuestions} — You are a true cybersecurity expert!";
            if (pct >= 75)
                return $"Great job! {Score}/{TotalQuestions} — You have a solid understanding of cybersecurity.";
            if (pct >= 50)
                return $"Good effort! {Score}/{TotalQuestions} — Keep learning to strengthen your knowledge.";

            return $"Score: {Score}/{TotalQuestions} — Keep studying! Cybersecurity knowledge is your best defence online.";
        }

    }
}
