using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace Steam_Workshop_Sorter
{
    class Program
    {

        public static string bordertop = "╔═════════════════════════════════════════════════════════════════╗"; // Decorative Bar
        public static string borderbottom = "╚═════════════════════════════════════════════════════════════════╝"; // Decorative Bar
        public static string title = "[ Steam Workshop Sorter v1.0 ]"; // Application Title
        public static string text = ""; // Empty string used for general text
        public static string choice = ""; // Empty string used for answering Y/N
        public static string ModInfo = ""; // For mod title / information

        public static string SteamInstallPath = ""; // Steam Install Directory
        public static string workshopPath = ""; // Steam Install Directory + Workshop Path
        public static string currentBackupDirectory = ""; // Directory to backup mods beore renaming

        public static int ID = 0;
        static void Main(string[] args)
        {
            Intro(); // Run Basic Console Clear and Console Title on Application Start

            WriteTitle(); // Write Title of Application

            // ** This section asks for confirmation on Steam Workshop directory **
            Console.ForegroundColor = ConsoleColor.Yellow;
            text = "Is this your Steam Workshop Content Folder?";
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.DarkYellow;

            GetSteamDirectory(); // Gets Steam Install Directory 

            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Cyan;
            text = "[Y/N] : ";
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
            Console.SetCursorPosition((Console.WindowWidth + text.Length) / 2, Console.CursorTop - 1);

            choice = Console.ReadLine();
            YesNoHandler(); // Handles Yes/No replies
            // ** This section asks for confirmation on Steam Workshop directory **

            // ** This section creates backup folder and backs up mods**
            if (YesNoHandler() == "Y")
            {
                BackupFiles();
            }
            else if (YesNoHandler() == "N")
            {
                AskUserForDirectory(); // Ask user to input directory manually
            }

        }

        public static void GetSteamDirectory()
        {
            bool BitOperating = Environment.Is64BitOperatingSystem; // Check if user is 64-Bit or not
            string text = "";

            if (BitOperating == true) // If system is 64-bit, run the following:
            {
                SteamInstallPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", null); // Get steam folder directory from registry

                if (SteamInstallPath != null) // If steam path is not null
                {
                    workshopPath = SteamInstallPath + @"\steamapps\workshop\content";
                    text = workshopPath;
                    Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
                }
                else // If steam path is not set
                {
                    AskUserForDirectory();
                }
            }

            else if (BitOperating == false) // If system is 32-bit, run the following:
            {
                SteamInstallPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null); // Get steam folder directory from registry

                if (SteamInstallPath != null) // If steam path is set in registry
                {
                    workshopPath = SteamInstallPath + @"\steamapps\workshop\content";
                    text = workshopPath;
                    Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
                }
                else // If steam path is not set
                {
                    AskUserForDirectory();
                }
            }
        }

        public static void AskUserForDirectory()
        {
            // ** Ask user for directory **
            choice = ""; // Resets variable
            ID = 1; // For different title message
            Console.Clear(); // Clear Console of any text
            WriteTitle(); // Write title of Application

            Console.ForegroundColor = ConsoleColor.Yellow;
            text = @"Please specify your Steam Workshop path!";
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Cyan;
            text = "Steam Workshop Path : ";
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
            Console.SetCursorPosition((Console.WindowWidth + text.Length) / 2, Console.CursorTop - 1);

            string userPath = Console.ReadLine();

            if (System.IO.Directory.Exists(userPath)) // Check if Directory exists
            {
                workshopPath = userPath; // Set workshop Path to user defined path
                BackupFiles();
            }
            else
            {
                Console.Title = "ERROR: Path is invalid, check for mistakes!";
                AskUserForDirectory();
            }

        }

        public static void BackupFiles()
        {

            long unixSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();

            try
            {

                string RimworldDirectory = workshopPath + @"\294100";

                if (Directory.Exists(workshopPath + "\\Renamed_Mods_" + unixSeconds))
                {
                    Directory.Delete(workshopPath + "\\Renamed_Mods_", true);
                    CopyFilesRecursively(RimworldDirectory, workshopPath + "\\Renamed_Mods_" + unixSeconds);
                    currentBackupDirectory = workshopPath + "\\Renamed_Mods_" + unixSeconds;
                    RenameMods();
                }
                else
                {
                    CopyFilesRecursively(RimworldDirectory, workshopPath + "\\Renamed_Mods_" + unixSeconds);
                    currentBackupDirectory = workshopPath + "\\Renamed_Mods_" + unixSeconds;
                    RenameMods();
                }
            }
            catch
            {
                currentBackupDirectory = workshopPath + "\\Renamed_Mods_" + unixSeconds;
                RenameMods();
            }
        }

        public static void RenameMods()
        {
            ID = 2;
            Console.Clear();
            Console.WriteLine("");
            WriteTitle();

            // string RimworldDirectory = workshopPath + @"\294100";
            //var directories = Directory.GetDirectories(RimworldDirectory);

            string[] folders = System.IO.Directory.GetDirectories(currentBackupDirectory, "*", System.IO.SearchOption.TopDirectoryOnly);

            foreach (string ModFolders in folders)
            {
                string removeString = currentBackupDirectory + "\\";
                string InfoToGrab = ModFolders.Remove(ModFolders.IndexOf(removeString), removeString.Length);

                if (IsDigitsOnly(InfoToGrab) == true)
                {
                    string RealModName = GetModInfo(InfoToGrab);

                    string Parsed = RemoveSpecialCharacters(RealModName);

                    Directory.Move(ModFolders, ModFolders + "_" + Parsed.Replace(" ", "_"));

                }
                else
                {
                    // Ignore it!
                }

            }

            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Cyan;
            text = "COMPLETED! [Press Any Key]";
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_]+", "_", RegexOptions.Compiled);
        }

        public static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        public static string GetModInfo(string ModName)
        {
            WebClient client = new WebClient();
            string ModInfoHTML = client.DownloadString("https://steamcommunity.com/sharedfiles/filedetails/?id=" + ModName);

            Console.Clear();
            WriteTitle();

            ModInfo = GetStringBetweenCharacters(ModInfoHTML, "<title>Steam Workshop::", "</title>");

            Console.WriteLine("");
            text = ModInfo;
            Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));

            return ModInfo;
        }

        public static string GetStringBetweenCharacters(string input, string charFrom, string charTo)
        {
            String St = input;

            int pFrom = St.IndexOf(charFrom) + charFrom.Length;
            int pTo = St.LastIndexOf(charTo);

            String result = St.Substring(pFrom, pTo - pFrom);

            return result;
        }

        public static void WriteTitle()
        {
            // ** This section writes the title and borders **
            if (ID == 0)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (bordertop.Length / 2)) + "}", bordertop));
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (title.Length / 2)) + "}", title));
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (borderbottom.Length / 2)) + "}", borderbottom));
                Console.WriteLine("");
            }
            else if (ID == 1)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (bordertop.Length / 2)) + "}", bordertop));
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (title.Length / 2)) + "}", title));
                Console.WriteLine("");
                text = @"Example: C:\Program Files (x86)\Steam\steamapps\workshop\content";
                Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");
                Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (borderbottom.Length / 2)) + "}", borderbottom));
                Console.WriteLine("");
            }
            else if (ID == 2)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (bordertop.Length / 2)) + "}", bordertop));
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (title.Length / 2)) + "}", title));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("");
                Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (borderbottom.Length / 2)) + "}", borderbottom));
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Yellow;
                text = "Working...";
                Console.WriteLine(String.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
            }
            // ** This section writes the title and borders **
        }

        public static void Intro()
        {
            int firstRun = 0;

            if (firstRun == 0)
            {
                Console.Title = "";
                Console.Clear();
                firstRun++;
            }
            else
            {
                Console.Clear();
            }
        }
        public static string YesNoHandler()
        {
            if (choice == "Y")
            {
                return "Y";
            }

            else if (choice == "N")
            {
                return "N";
            }

            else
            {
                Main(null);
                return null;
            }
        }

        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                FileInfo info = new FileInfo(newPath);

                if (info.Length > 0)
                {
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                }
                else
                {
                    // ignore it
                }
            }
        }

    }
}
