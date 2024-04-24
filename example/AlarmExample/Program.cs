namespace AlarmExample
{
    /// <summary>
    /// A simple Console Application that allows for interactive input
    /// to test the <see cref="Alarm"/> implemented as a Stateless state
    /// machine.
    /// </summary>
    internal class Program
    {
        static Alarm? _alarm;

        static void Main(string[] args)
        {
            _alarm = new Alarm(10, 10, 10, 10);

            string input = "";

            WriteHeader();

            while (input != "q")
            {
                Console.Write("> ");

                input = Console.ReadLine()!;

                if (!string.IsNullOrWhiteSpace(input))
                    switch (input.Split(" ")[0])
                    {
                        case "q":
                            Console.WriteLine("Exiting...");
                            break;
                        case "fire":
                            WriteFire(input);
                            break;
                        case "canfire":
                            WriteCanFire();
                            break;
                        case "state":
                            WriteState();
                            break;
                        case "h":
                        case "help":
                            WriteHelp();
                            break;
                        case "c":
                        case "clear":
                            Console.Clear();
                            WriteHeader();
                            break;
                        default:
                            Console.WriteLine("Invalid command. Type 'h' or 'help' for valid commands.");
                            break;
                    }
            }
        }

        static void WriteHelp()
        {
            Console.WriteLine("Valid commands:");
            Console.WriteLine("q               - Exit");
            Console.WriteLine("fire <state>    - Tries to fire the provided commands");
            Console.WriteLine("canfire <state> - Returns a list of fireable commands");
            Console.WriteLine("state           - Returns the current state");
            Console.WriteLine("c / clear       - Clear the window");
            Console.WriteLine("h / help        - Show this again");
        }

        static void WriteHeader()
        {
            Console.WriteLine("Stateless-based alarm test application:");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("");
        }

        static void WriteCanFire()
        {
            foreach (AlarmCommand command in (AlarmCommand[])Enum.GetValues(typeof(AlarmCommand)))
                if (_alarm != null && _alarm.CanFireCommand(command))
                    Console.WriteLine($"{Enum.GetName(typeof(AlarmCommand), command)}");
        }

        static void WriteState()
        {
            if(_alarm != null ) 
                Console.WriteLine($"The current state is {Enum.GetName(typeof(AlarmState), _alarm.CurrentState())}");
        }

        static void WriteFire(string input)
        {
            if (input.Split(" ").Length == 2)
            {
                try
                {
                    if (Enum.TryParse(input.Split(" ")[1], out AlarmCommand command))
                    {
                        if (_alarm != null)
                            _alarm.ExecuteTransition(command);
                    }
                    else
                    {
                        Console.WriteLine($"{input.Split(" ")[1]} is not a valid AlarmCommand.");
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine($"{input.Split(" ")[1]} is not a valid AlarmCommand to the current state.");
                }
            }
            else
            {
                Console.WriteLine("fire requires you to specify the command you want to fire.");
            }
        }
    }
}