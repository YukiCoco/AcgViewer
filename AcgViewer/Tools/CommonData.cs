using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgViewer.Tools
{
     public static class CommonData
    {
        private static int postCount = 0;
        private static int currentPage = 1;
        private static ConfigFile configFile = ConfigFile.LoadOrCreateFile("config.yml");

        public static int PostCount { get => postCount; set {

                postCount = value;
            }
        }

        public static int CurrentPage { get => currentPage; set => currentPage = value; }

        public static bool isNsfw
        {
            get
            {
                bool isCompleted = false;
                bool nsfw = bool.TryParse(configFile["nsfw"],out isCompleted);
                if (isCompleted)
                {
                    return nsfw;
                }
                else
                {
                    return false;
                }
                
            }
            set
            {
                configFile.AddOrSetConfigValue("nsfw", Convert.ToString(value));
            }
        }
    }
}
