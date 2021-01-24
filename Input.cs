using System;
using System.Globalization;

namespace A1_Parallel_Programming
{
    /// <summary>
    /// Input allows for password and time input with validation and prompts.
    /// </summary>
    class Input
    {
        /// <summary>
        /// Password gets password input from the command line with a prompt.
        /// </summary>
        /// <returns>String - the validated password.</returns>
        public static String Password()
        {
            String password;
            do
            {
                Console.WriteLine("***************************************");
                Console.WriteLine("Enter a password (3 - 128 characters): ");
                password = Console.ReadLine();
                Console.WriteLine();
            } while (!Validation.Password(password));

            Console.WriteLine();

            return password;
        }

        /// <summary>
        /// Time gets time input in the format of hh:mm:ss from the command line with a prompt.
        /// </summary>
        /// <returns>TimeSpan - the validated timespan.</returns>
        public static TimeSpan Time()
        {
            String timeInput = "";
            do
            {
                Console.WriteLine("***********************");
                Console.WriteLine("Time Limit (hh:mm:ss): ");
                timeInput = Console.ReadLine();
            } while (!Validation.Time(timeInput));
            Console.WriteLine();
            
            return TimeSpan.ParseExact(timeInput, Validation.TimeFormat, CultureInfo.InvariantCulture);
        }
    }
}