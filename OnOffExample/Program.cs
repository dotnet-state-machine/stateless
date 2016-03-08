using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stateless;

namespace OnOffExample
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string on = "On", off = "Off";
                var space = ' ';

                var onOffSwitch = StateMachine<string, char>.Create(off);

                onOffSwitch.Configure(off).Permit(space, on);
                onOffSwitch.Configure(on).Permit(space, off);

                var conOffSwitch = onOffSwitch.FinishConfiguration();

                Console.WriteLine("Press <space> to toggle the switch. Any other key will raise an error.");

                while (true)
                {
                    Console.WriteLine("Switch is in state: " + conOffSwitch.State);
                    var pressed = Console.ReadKey(true).KeyChar;
                    conOffSwitch.Fire(pressed);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
    }
}
