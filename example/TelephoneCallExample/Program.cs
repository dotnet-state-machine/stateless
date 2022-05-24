using System;
using System.Threading.Tasks;

namespace TelephoneCallExample;

internal static class Program {
    private static async Task Main() {
        var phoneCall = new PhoneCall("Lokesh");

        phoneCall.Print();
        await phoneCall.DialedAsync("Prameela");
        phoneCall.Print();
        await phoneCall.ConnectedAsync();
        phoneCall.Print();
        await phoneCall.SetVolumeAsync(2);
        phoneCall.Print();
        await phoneCall.HoldAsync();
        phoneCall.Print();
        await phoneCall.MuteAsync();
        phoneCall.Print();
        await phoneCall.UnmuteAsync();
        phoneCall.Print();
        await phoneCall.ResumeAsync();
        phoneCall.Print();
        await phoneCall.SetVolumeAsync(11);
        phoneCall.Print();


        Console.WriteLine(phoneCall.ToDotGraph());

        Console.WriteLine("Press any key...");
        Console.ReadKey(true);
    }
}