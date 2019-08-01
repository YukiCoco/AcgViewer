using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcgViewer.Tools
{
     public class CommonData
    {
        private static int currentPreImgDownloadCount = 0;
        private static int postCount = 0;
        private static int currentPage = 1;
        public static int CurrentPreImgDownloadCount { get => currentPreImgDownloadCount; set {
                if(value == 0)
                {
                    MainWindow.dataLib.IsNotSearching = true;
                }
                else
                {
                    MainWindow.dataLib.IsNotSearching = false;
                }
                currentPreImgDownloadCount = value; } }

        public static int PostCount { get => postCount; set {

                postCount = value;
            }
        }

        public static int CurrentPage { get => currentPage; set => currentPage = value; }
    }
}
