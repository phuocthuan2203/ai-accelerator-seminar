using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ToolLendingPlatform.Application.Validators
{
    /// <summary>
    /// Validates username format per Sprint Design §3 API Contract.
    /// Rule #4: Username requires:
    ///   - Minimum 3 characters, maximum 50 characters
    ///   - Only alphanumeric and underscore
    /// </summary>
    public class UsernameValidator
    {
        public static (bool Valid, List<string> Errors) ValidateUsername(string username)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(username))
            {
                errors.Add("Username cannot be empty");
                return (false, errors);
            }

            if (username.Length < 3)
                errors.Add("Username must be at least 3 characters");

            if (username.Length > 50)
                errors.Add("Username must not exceed 50 characters");

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                errors.Add("Username can only contain letters, numbers, and underscores");

            return (errors.Count == 0, errors);
        }
    }
}
