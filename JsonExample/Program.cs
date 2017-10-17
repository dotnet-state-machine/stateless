using System;

namespace JsonExample
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Creating member from JSON");
            var aMember = Member.FromJson("{ \"State\":\"1\",\"Name\":\"Jay\"}");

            Console.WriteLine($"Member {aMember.Name} created, membership state is {aMember.State}");

            aMember.Suspend();
            aMember.Reactivate();
            aMember.Terminate();

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
}

