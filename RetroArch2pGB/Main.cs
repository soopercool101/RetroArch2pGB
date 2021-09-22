using System.Diagnostics;

namespace RetroArch2pGB
{
    public static class RetroArch2pGB
    {
        public static void Main(string romPath, bool separateSaves, bool uniqueP1save = false)
        {
            Console.WriteLine("Rom Path: " + romPath);
            // Get installation directory
            var filepath = Path.GetDirectoryName(AppContext.BaseDirectory);
            if (string.IsNullOrEmpty(filepath))
            {
                Console.WriteLine("File Path could not be found.");
                return;
            }

            // Make sure RetroArch is present with the SameBoy core
            var retroArchPath = Path.Combine(filepath, "retroarch.exe");
            if (!File.Exists(retroArchPath))
            {
                Console.WriteLine("RetroArch could not be found at " + retroArchPath);
                return;
            }
            var sameBoyPath = Path.Combine(filepath, "cores", "sameboy_libretro.dll");
            if (!File.Exists(sameBoyPath))
            {
                Console.WriteLine("SameBoy_libretro could not be found at " + sameBoyPath);
                return;
            }

            // Set the rom paths (default to the passed-in path)
            var rom1path = romPath;
            var rom2path = romPath;

            // Get the temp file directory (only used for separate save)
            var tempFileDir = Path.Combine(filepath, "2pGbTemp");

            if (separateSaves)
            {
                // Get the Rom's unique name and its extension (which should be .gb/.gbc)
                var romName = Path.GetFileNameWithoutExtension(romPath);
                var romExt = Path.GetExtension(romPath);
                // Create the temporary directory to copy files to
                Directory.CreateDirectory(tempFileDir);
                // Copy the rom to have a unique named variant. RetroArch saves are based on filename!
                rom2path = Path.Combine(tempFileDir, romName + " [Player 2]" + romExt);
                File.Copy(romPath, rom2path);
                // If we don't want to use the default save for p1, we need to copy and rename a file for that too.
                if (uniqueP1save)
                {
                    rom1path = Path.Combine(tempFileDir, romName + " [Player 1]" + romExt);
                    File.Copy(romPath, rom1path);
                }
            }

            // Start RetroArch
            Process.Start(retroArchPath, $"-L \"{sameBoyPath}\" --subsystem \"gb_link_2p\" \"{rom1path}\" \"{rom2path}\"");

            // Wait for RetroArch to exit
            Process.GetProcessesByName("Retroarch").FirstOrDefault(x => (x.MainModule?.FileName ?? "").Equals(retroArchPath, StringComparison.OrdinalIgnoreCase))?.WaitForExit();

            // Clear temp files if any were created
            if (Directory.Exists(tempFileDir))
            {
                // Recursive delete the temp directory
                Directory.Delete(tempFileDir, true);
            }
        }
    }
}