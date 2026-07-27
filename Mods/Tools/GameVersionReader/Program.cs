using System;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace GameVersionReader
{
    // Stamps the topmost "Version:" changelog entry in a ReadMe.md with the
    // 7 Days To Die version it was built against, read out of Assembly-CSharp.dll.
    // Invoked from the 0-SCore post-build event; never fails the build.
    internal static class Program
    {
        private const string DefaultGameInstallPath = @"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die";

        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: GameVersionReader <ReadMe.md path> [game install path]");
                return 0;
            }

            string readmePath = args[0];
            string gameInstallPath = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1])
                ? args[1]
                : DefaultGameInstallPath;

            string dllPath = Path.Combine(gameInstallPath, "7DaysToDie_Data", "Managed", "Assembly-CSharp.dll");

            string version = GetGameVersion(dllPath);
            if (version == null)
            {
                Console.Error.WriteLine($"GameVersionReader: could not determine game version from '{dllPath}'; ReadMe.md left untouched.");
                return 0;
            }

            StampReadme(readmePath, version);
            return 0;
        }

        private static string GetGameVersion(string strFolder)
        {
            if (!File.Exists(strFolder))
                return null;

            try
            {
                using var module = ModuleDefinition.ReadModule(strFolder, new ReaderParameters { ReadWrite = false });
                var constants = module.Types.First(d => d.Name == "Constants");
                var major = constants.Fields.SingleOrDefault(c => c.Name == "cVersionMajor");
                var minor = constants.Fields.SingleOrDefault(c => c.Name == "cVersionMinor");
                var build = constants.Fields.SingleOrDefault(c => c.Name == "cVersionBuild");
                if (major == null || minor == null)
                    return null;

                if (!int.TryParse(major.Constant.ToString(), out int convertedMajor))
                    return null;
                if (!int.TryParse(minor.Constant.ToString(), out int convertedMinor))
                    return null;

                // cVersionMinor packs two digits, e.g. 10 means ".1.0".
                string minorVersion = $"{convertedMinor / 10}.{convertedMinor % 10}";

                // It's doubtful that we'll ever see a stable release above a version of 10. Right?
                string version = convertedMajor > 10
                    ? "Alpha" + convertedMajor + "." + minorVersion
                    : "v" + convertedMajor + "." + minorVersion;

                if (build != null && int.TryParse(build.Constant.ToString(), out int convertedBuild))
                    version += $" (b{convertedBuild})";

                return version;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"GameVersionReader: failed to read '{strFolder}': {ex.Message}");
                return null;
            }
        }

        private static void StampReadme(string readmePath, string gameVersion)
        {
            if (!File.Exists(readmePath))
                return;

            // Normalize to LF for the split/join round-trip; the file is LF-only and
            // rewriting with Environment.NewLine would touch every line and blow up the diff.
            string text = File.ReadAllText(readmePath).Replace("\r\n", "\n");
            var lines = text.Split('\n').ToList();

            int versionIndex = lines.FindIndex(l => l.TrimStart().StartsWith("Version:"));
            if (versionIndex < 0)
                return;

            int nextIndex = versionIndex + 1;
            string stampedLine = "\tGame Version: " + gameVersion;
            if (nextIndex < lines.Count && lines[nextIndex].TrimStart().StartsWith("Game Version:"))
            {
                if (lines[nextIndex] == stampedLine)
                    return; // already up to date

                lines[nextIndex] = stampedLine; // game was rebuilt against a different version - update in place
                File.WriteAllText(readmePath, string.Join("\n", lines), new UTF8Encoding(true));
                return;
            }

            lines.Insert(nextIndex, stampedLine);

            File.WriteAllText(readmePath, string.Join("\n", lines), new UTF8Encoding(true));
        }
    }
}