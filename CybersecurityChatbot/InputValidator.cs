using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityChatbot
{
    public static class InputValidator
    {
        public static bool isValid(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (input.Trim().Length < 2)
                return false;

            return true;
        }

        public static bool isNameValid(string name)
        {
            if (!isValid(name))
                return false;

            // Reject purely numeric entries (e.g. "12345")
            if (int.TryParse(name.Trim(), out _))
                return false;

            return true;
        }

        public static string Sanitise(string input)
        {
            return input?.Trim().ToLower() ?? "";
        }

    }
}
