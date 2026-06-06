// DebugRunner.cs
// Drop this into the project, change Program.cs Main() to call DebugRunner.Run()
// Remove before deploying to friend obviously

using System;

class DebugRunner
{
    public static void Run()
    {
        while (true)
        {
            Console.WriteLine("\n" + new string('─', 40) + "\n");
            Console.WriteLine("=== TelemetryMon Debug Runner ===\n");
            Console.WriteLine(" 1  ShowMessage");
            Console.WriteLine(" 2  PlayAmbientSound");
            Console.WriteLine(" 3  ShowBriefly (jumpscare image)");
            Console.WriteLine(" 4  SpeakRandomMessage (TTS)");
            Console.WriteLine(" 5  PlayUSBSound");
            Console.WriteLine(" 6  FlashCMD");
            Console.WriteLine(" 7  OpenCamera");
            Console.WriteLine(" 8  WallpaperEngineSequence");
            Console.WriteLine(" 9  SomeoneIsInHereSequence");
            Console.WriteLine("10  TheFileSequence");
            Console.WriteLine("11  EjectCDTray");
            Console.WriteLine("12  ReinstallGreeting");
            Console.WriteLine(" 0  Exit\n");
            Console.Write("Pick: ");

            string input = Console.ReadLine()?.Trim() ?? "";

            Console.WriteLine("\nRunning...\n");

            try
            {
                switch (input)
                {
                    case "1": Program.ShowMessage(); break;
                    case "2": Program.PlayAmbientSound(); break;
                    case "3": Program.ShowBriefly(); break;
                    case "4": Program.SpeakRandomMessage(); break;
                    case "5": Program.PlayUSBSound(); break;
                    case "6": Program.FlashCMD(); break;
                    case "7": Program.OpenCamera(); break;
                    case "8": Program.WallpaperEngineSequence(); break;
                    case "9": Program.SomeoneIsInHereSequence(); break;
                    case "10": Program.TheFileSequence(); break;
                    case "11": Program.EjectCDTray(); break;
                    case "12": Program.ReinstallGreeting(); break;
                    case "0": return;
                    default: Console.WriteLine("Invalid option."); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\nDone. Press any key to continue...");
            Console.Read();
        }
    }
}