using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ToolLendingPlatform.Application.Validators
{
    /// <summary>
    /// Validates password strength per Sprint Design §3 API Contract.
    /// Rule #3: Password requires:
    ///   - Minimum 6 characters
    ///   - At least 1 uppercase letter
    ///   - At least 1 lowercase letter
    ///   - At least 1 digit
    /// </summary>
    public class PasswordValidator
    {
        public static (bool Valid, List<string> Errors) ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Password cannot be empty");
                return (false, errors);
            }

            if (password.Length < 6)
                errors.Add("Password must be at least 6 characters");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("Password must contain at least 1 uppercase letter");

            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("Password must contain at least 1 lowercase letter");

            if (!Regex.IsMatch(password, @"\d"))
                errors.Add("Password must contain at least 1 digit");

            return (errors.Count == 0, errors);
        }
    }
}
