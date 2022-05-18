using System;

namespace BugTrackerExample
{
    internal static class Program
    {
        private static void Main()
        {
            var bug = new Bug("Incorrect stock count");

            bug.Assign("Joe");
            bug.Defer();
            bug.Assign("Harry");
            bug.Assign("Fred");
            bug.Close();

            Console.WriteLine();
            Console.WriteLine("State machine:");
            Console.WriteLine(bug.ToDotGraph());

            Console.ReadKey(false);
        }
    }
}
