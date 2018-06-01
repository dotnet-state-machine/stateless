using System;

namespace BugTrackerExample
{
    class Program
    {
        static void Main(string[] args)
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
