using System;
using System.Threading.Tasks;

namespace JsonExample; 

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Creating member from JSON");
        var aMember = Member.FromJson("{ \"State\":\"1\",\"Name\":\"Jay\"}");

        Console.WriteLine($"Member {aMember.Name} created, membership state is {aMember.State}");

        await aMember.SuspendAsync();
        await aMember.ReactivateAsync();
        await aMember.TerminateAsync();

        Console.WriteLine("Member JSON:");

        var jsonString = aMember.ToJson();
        Console.WriteLine(jsonString);

        var anotherMember = Member.FromJson(jsonString);

        if (aMember.Equals(anotherMember))
        {
            Console.WriteLine("Members are equal");
        }

        Console.WriteLine("Press any key...");
        Console.ReadKey();
    }
}