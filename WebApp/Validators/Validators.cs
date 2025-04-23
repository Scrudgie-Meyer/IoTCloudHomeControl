using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebApp.Validators
{
    public class PasswordComplexityAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var password = value as string;

            if (string.IsNullOrWhiteSpace(password))
                return false;

            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasMinimumLength = password.Length >= 8;

            return hasUpper && hasLower && hasMinimumLength;
        }

        public override string FormatErrorMessage(string name)
        {
            return "Password must be at least 8 characters long and include both uppercase and lowercase letters.";
        }
    }
}
