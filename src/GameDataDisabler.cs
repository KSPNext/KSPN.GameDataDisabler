using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KSPN.GameDataDisabler
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class GameDataDisabler : MonoBehaviour
    {
        /// <summary>
        /// Prefix for all messages written to the log.
        /// </summary>
        private const string LogPrefix = "[KSPN.GameDataDisabler]:";

        /// <summary>
        /// File extension for files containing the disabler and enabler patterns.
        /// </summary>
        private const string DisablerFileExtension = "kspn_gdd";

        /// <summary>
        /// Filename suffix used to prevent files from loading.
        /// </summary>
        private const string DisabledSuffix = ".disabled";

        /// <summary>
        /// Character that identifies a comment if placed at the start of a line
        /// inside of a disabler pattern file.
        /// </summary>
        private const char CommentIdentifier = '#';

        /// <summary>
        /// String that identifies the following lines as containing disabler
        /// patterns inside of a disabler pattern file.
        /// </summary>
        private const string DisableIdentifier = "[disable]";

        /// <summary>
        /// String that identifies the following lines as containing enabler
        /// patterns inside of a disabler pattern file.
        /// </summary>
        private const string EnableIdentifier = "[enable]";

        private static void Log(object message) => Debug.Log($"{LogPrefix} {message}");

        private static IEnumerable<UrlDir.UrlFile> GetDisablerFiles()
        {
            foreach (UrlDir.UrlFile candidate in GameDatabase.Instance.root.GetFiles(UrlDir.FileType.Unknown))
            {
                if (candidate.fileExtension == DisablerFileExtension)
                {
                    yield return candidate;
                }
            }
        }

        private static void Parse(Matcher matcher, TextReader reader)
        {
            string line;
            bool disable = true;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                // patterns to disable will follow on next line
                if (line == DisableIdentifier)
                {
                    disable = true;
                }
                // patterns to enable will follow on next line
                else if (line == EnableIdentifier)
                {
                    disable = false;
                }
                // ignore comments and empty lines
                else if (line != string.Empty && line[0] != CommentIdentifier)
                {
                    if (disable)
                    {
                        matcher.AddInclude(line);
                    }
                    else
                    {
                        matcher.AddExclude(line);
                    }
                }
            }
        }

        /// <summary>
        /// Disables a file by appending a suffix to the path inside of the
        /// GameDatabase so that the file cannot be found anymore.
        /// </summary>
        /// <param name="file">File to disable</param>
        private static void Disable(UrlDir.UrlFile file)
        {
            var disabled = new UrlDir.UrlFile(file.parent, new FileInfo($"{file.fullPath}{DisabledSuffix}"));
            file.parent.files[file.parent.files.IndexOf(file)] = disabled;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Start()
        {
            var matcher = new Matcher();

            foreach (UrlDir.UrlFile disablerFile in GetDisablerFiles())
            {
                Log($"parsing {disablerFile.fullPath}");
                using (var sr = new StreamReader(disablerFile.fullPath))
                {
                    Parse(matcher, sr);
                }
            }

            // GameDatabase.Instance.root.GetDirectory("GameData") doesn't work
            UrlDir gameData = GameDatabase.Instance.root.children.Find(dir => dir.type == UrlDir.DirectoryType.GameData);
            IEnumerable<FilePatternMatch> matches = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(gameData.path))).Files;

            foreach (var match in matches)
            {
                // get the GameDatabase entry for the matching file
                UrlDir.UrlFile file = gameData.GetFile(match.Path);

                // sometimes can be null but I don't know why
                if (file != null)
                {
                    Disable(file);
                    Log($"disabled {file.url}.{file.fileExtension}");
                }
            }

            // won't need this object anymore as it's only used once when the game starts
            Destroy(gameObject);
        }
    }
}
