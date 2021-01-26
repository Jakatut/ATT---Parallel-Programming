using System;
using System.Globalization;
using System.Linq;

namespace A1_Parallel_Programming
{
    /// <summary>
    /// Validations allows for time and password validation with messages.
    /// </summary>
    class Validation
    {
        /// <summary>
        /// The maximum length a password input should be.
        /// </summary>
        public const int MaxPasswordLength = 128;

        /// <summary>
        /// The minimum length a password input should be.
        /// </summary>
        public const int MinPasswordLength = 3;

        /// <summary>
        /// The timeformat string to follow.
        /// </summary>
        public const string TimeFormat = "g";

        /// <summary>
        /// Time validates the given time is:
        ///     - Following the format of hh:mm:ss 
        /// </summary>
        /// <param name="limit">The time input to validates.</param>
        /// <returns>bool - true if the password is valid.</returns>
        public static bool Time(string limit)
        {
            try
            {
                // Parse hh:mm:ss
                TimeSpan.ParseExact(limit, TimeFormat, CultureInfo.InvariantCulture);
            }
            catch (System.FormatException)
            {
                // The limit provided does not follow the correct format.
                Console.WriteLine("Invalid time limit. Please use the format hh:mm:ss.\n");
                return false;
            }
            return true;
        }


        /// <summary>
        /// Password validates the given password is:
        ///     - At least 3 characters long.
        ///     - 128 characters or less.
        ///     - All letters (no symbols or numbers).
        ///     - All upper-cased.
        /// </summary>
        /// <param name="password">The password input to validate.</param>
        /// <returns>bool - true if the password is valid.</returns>
        public static bool Password(string password)
        {
            if (password.Length < MinPasswordLength)
            {
                Console.WriteLine("The password must contain at least 3 characters.\n");
                return false;
            }

            if (password.Length > MaxPasswordLength)
            {
                Console.WriteLine("The password must be 128 characters or less.\n");
                return false;
            }

            if (!password.All(c => Char.IsLetter(c)))
            {
                Console.WriteLine("The password must be all letters (No symbols or numbers).\n");
                return false;
            }

            if (!password.All(c => Char.IsUpper(c)))
            {
                Console.WriteLine("The password must be all UPPER-CASE.\n");
                return false;
            }

            return true;
        }
    }
}