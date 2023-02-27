using R2API;
using Path = System.IO.Path;
using System.Collections.Generic;
using RoR2;
using System.IO;

namespace Daredevil
{
    public static class Languages
    {
        public const string LanguageFileName = "en.language";
        public static string LanguageFilePath = Path.GetDirectoryName(DaredevilMain.PInfo.Location);
        public static void Init()
        {

            if (Directory.Exists(LanguageFilePath))
            {
                RoR2.Language.collectLanguageRootFolders += AddOurFolder;
            }
        }

        private static void AddOurFolder(List<string> list)
        {
            //Adding our root folder with the languages means ror2 will automatically load our language files.
            list.Add(LanguageFilePath);
        }
    }
}
