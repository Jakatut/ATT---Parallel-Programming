using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace A1_Parallel_Programming
{
    /// <summary>
    /// Class <c>Bruteforce</c> runs parallel and non-parallel bruteforce password cracking tests.
    /// </summary>
    class Bruteforce
    {
        private static bool matchFound = false;
        private static bool running = false;
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private static List<char> Alphabet = new List<char>();
        private static String password = "";
        private static String crackedPassword = "";
        private static int count = 0;

        private static TimeSpan timeLimit = new TimeSpan();
        private static TimeSpan elapsedTime = new TimeSpan();
        private static Stopwatch runningTime = new Stopwatch();
        private static System.Timers.Timer timer;

        public static void Main(string[] args)
        {
            // Populate the password alphabet.
            for (char i = 'A'; i < 'Z'; ++i)
            {
                Alphabet.Add(i);
            }

            // Get input.
            password = Input.Password();
            timeLimit = Input.Time();

            // Crack the password in a non-parallel fashion.
            setup();
            crack(password);


            // Crack the password in a parrallel fashion.
            setup();
            parallelCrack(password);
        }


        /// <summary>
        /// setup sets resets the tracking and state data of a bruteforce attack.
        /// </summary>
        private static void setup()
        {
            running = true;
            runningTime.Reset();
            count = 0;
        }


        /// <summary>
        /// crack executes a brute force attack using a recursive combination function.
        /// </summary>
        /// <param name="password">
        /// The password to be cracked. This is used only as a comparison when a new password is made.
        /// </param>
        private static void crack(string password)
        {
            Console.WriteLine("*****Non-Parallel Bruteforce*****");
            for (int i = Validation.MinPasswordLength; i < Validation.MaxPasswordLength; ++i)
            {
                // If a match was found, stop the loop.
                if (runCombinationMatch(i))
                {
                    break;
                }

                if (!running)
                {
                    break;
                }
            };
            outputStats();
        }


        /// <summary>
        /// parallelCrack executes a brute force attack using a recursive combination function run in parallel.
        /// The parrallel execution begins when we try a new password length. Each length will create a new parallel task
        /// </summary>
        /// <param name="password">
        /// The password to be cracked. This is used only as a comparison when a new password is made.
        /// </param>
        private static void parallelCrack(string password)
        {
            Console.WriteLine("*****Parallel Bruteforce*****");
            // Setup cancellation of tasks after time limit has been hit.
            cancellationTokenSource.CancelAfter((int)timeLimit.TotalMilliseconds);
            ParallelOptions parallelOptions = new ParallelOptions { CancellationToken = cancellationTokenSource.Token };
            setup();
            try
            {
                // Create password of every length 
                Parallel.For(Validation.MinPasswordLength, Validation.MaxPasswordLength, parallelOptions, ((int i, ParallelLoopState state) =>
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                    // If a match is found, cancel the parallel exections.
                    if (runCombinationMatch(i))
                    {
                        state.Break();
                    }
                }));
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Parallel tasks cancelled.");
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
            outputStats();
        }


        /// <summary>
        /// runCombinationMatch runs the combination function for a given password length.
        /// </summary>
        /// <param name="length">The length each generated combination must be.</param>
        /// <returns>bool - True if a match was found.</returns>
        private static bool runCombinationMatch(int length)
        {
            runningTime = Stopwatch.StartNew();
            if (findMatchingCombination("", length))
            {
                elapsedTime = runningTime.Elapsed;
                matchFound = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// findMatchingCombination is a recursive function that creates new combinations of a string of upper-cased alphabetic characters.
        /// The algorithm works by iterating through an alphabet, and prepending to a new letter in the alphabet each time this happen, a length count goes down by 1.
        /// eventually, the prepending string builds up in length until the length cound is at 0. At this point, you have a new combination of characters to compare.
        /// The algorith works sequentially. for example, if we started with the inputs ("", 2), the first iteration would just create a prefix of 1 letter, esentially
        /// copying the alphabet.
        /// recursing into the function, we subtract the length and have an inptut of ("A", 1), ("B", 1), ..., ("Z", 1).
        /// Each of these calls to findMatchingCombination will create a new set of calls to findMatchingCombination for each prefix we already have.
        /// For the case of ("A", 1), we would have more calls like ("AA", 0), ("AB", 0), ("AC", 0), ... ("AZ", 0).
        /// When the calls are made to findMatchingCombination, our check of length 0 will allow us to check if the password matches.
        /// A count is incremented for each password check.
        /// If the check passes, running stops.
        /// </summary>
        /// <param name="prefix">The previous combination to append to.</param>
        /// <param name="length">The length of the combintation we want to generate (often time that value - 1).</param>
        /// <returns>bool - true if a match occurs.</returns>
        private static bool findMatchingCombination(string prefix, int length)
        {
            if (!running)
            {
                return false;
            }

            if(isElapsedTimeAtOrPastLimit())
            {
                return false;
            }

            if (length == 0)
            {
                if (running)
                {
                    Interlocked.Increment(ref count);
                    // Check if the password matches.
                    // Break out of recursion if it does.
                    if (prefix != "" && prefix == password)
                    {
                        running = false;
                        crackedPassword = prefix;
                        return true;
                    }
                }
                return false;
            }

            // Add every character to the prefix and pass on until length is 0.
            for (int i = 0; i < Alphabet.Count; ++i)
            {
                String newPrefix = prefix + Alphabet[i];

                // breakout of recursion if there was no match.
                if (running && findMatchingCombination(newPrefix, length - 1))
                {
                    return true;
                }
                if (!running)
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// isElapsedTimeAtOrPastLimit checks if the elapsed time is at or passed the time limit.
        /// </summary>
        /// <returns>bool - true if the elapsed tiem is at or passed the time limit.</returns>
        private static bool isElapsedTimeAtOrPastLimit()
        {
            if (runningTime.ElapsedMilliseconds >= timeLimit.TotalMilliseconds)
            {
                // Just to make output look a little better. Really, this will go past a few ms, but no comparisons are made.
                elapsedTime = timeLimit;
                running = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Outputs the status of the match, elapsed time, and the comparison count.
        /// </summary>
        private static void outputStats()
        {
            if (!matchFound)
            {
                Console.WriteLine("Failed to find a match in time.");
            } else {
                Console.WriteLine("Match found!");
                Console.WriteLine("Password: " + crackedPassword);
            }
            Console.WriteLine("Elapsed time: {0}", elapsedTime);
            Console.WriteLine("Comparison count: {0}\n", count);
        }
    }
}
