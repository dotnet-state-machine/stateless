using System;
using System.Threading.Tasks;

namespace BugTrackerExample; 

internal static class Program
{
    private static async Task Main()
    {
        var bug = new Bug("Incorrect stock count");

        await bug.AssignAsync("Joe");
        await bug.DeferAsync();
        await bug.AssignAsync("Harry");
        await bug.AssignAsync("Fred");
        await bug.CloseAsync();

        Console.WriteLine();
        Console.WriteLine("State machine:");
        Console.WriteLine(bug.ToDotGraph());

        Console.ReadKey(false);
    }
}