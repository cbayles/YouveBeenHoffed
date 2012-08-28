using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

namespace YouveBeenHoffed
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0 || new[] { "?", "/?", "-?" }.Contains(args[0]))
                return Help();

            var path = args[0];
            var selectRandom = new[] { "-r", "-random", "/r" }.Contains(path.ToLower());

            try
            {
                var hoffer = new Hasselhoffer();
                var success = selectRandom ? hoffer.SetRandomDesktopWallpaper() : hoffer.SetDesktopWallpaper(path);
                if (success) Console.WriteLine("You've been hoffed!");
                else throw new ApplicationException("Unknwon error. Maybe try another image?", new Win32Exception(Marshal.GetLastWin32Error()));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hoffing aborted. {0}", ex.Message);
                if (ex.InnerException != null) Console.WriteLine(ex.InnerException.Message);
                return 1;
            }
            return 0;
        }

        private static int Help()
        {
            Console.WriteLine(@"
NAME
    YouveBeenHoffed

SYNOPSIS
    Someone walks off without locking their computer. 
    You shame them by changing their wallpaper to a questionable image of David Hasselhoff. 
    Laughter ensues.

SYNTAX
    YouveBeenHoffed [<filePathOrImageUrl>] [-r | -random]

REMARKS
    <filePathOrImageUrl> can be the full path to an image file or a Uri to an image file. 
    -r will select a random image for you.
");
            return 1;
        }
    }

    public class Hasselhoffer
    {
        /// <summary>
        /// SystemParametersInfo:
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SystemParametersInfo(int uiAction, int uiParam, string pvParam, int fWinIni);

        public bool SetDesktopWallpaper (string path)
        {
            const int SPI_SETDESKWALLPAPER = 0x14;
            const int SPIF_UPDATEINIFILE = 0x01; // permanent change in user profile
            const int SPIF_SENDCHANGE = 0x02; // inform all windows something has changed

            ValidatePath(path);

            string tempFile = null;
            if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                tempFile = Path.GetTempFileName();
                try {new WebClient().DownloadFile(path, tempFile);}catch{throw new WebException("Failed to download "+ path);}
                path = tempFile;
            }

            var result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

            if (tempFile != null && File.Exists(tempFile)) File.Delete(tempFile);

            // If SystemParametersInfo succeeds, the return value is a nonzero value. If it fails, the return value is zero
            return result != 0;
        }

        public bool SetRandomDesktopWallpaper()
        {
            var path = GetRandomPic();
            return SetDesktopWallpaper(path);
        }

        private void ValidatePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("You must tell me what file to Hoff them with. Hurry, they're coming!");

            if (!(Uri.IsWellFormedUriString(path, UriKind.Absolute) || File.Exists(path)))
                throw new ArgumentException("Dude, {0} doesn't even exist. Tell me what pic to use!", path);

            var allowedTypes = new[] {".bmp", ".jpg", ".jpeg", ".gif", ".png", ".tiff", ".wmf"};
            if (!allowedTypes.Any(path.ToLower().EndsWith))
                throw new ArgumentException(string.Format("You must use one of these image types: {0}", allowedTypes.Aggregate((a, b) => a + " " + b)));
        }

        private string GetRandomPic()
        {
            var pics = new[]
                {
                    "http://themcelebrity.files.wordpress.com/2012/01/david-hasselhoff-3.jpg",
                    "http://www.phishlabs.com/blog/wp-content/uploads/2010/05/hasselhoff_1600w.jpg",
                    "http://www.scenicreflections.com/files/knight_rider_hasselhoff_wallpaper_1024.jpg",
                    "http://www.recordsale.de/cdpix/d/david_hasselhoff-lovin_feelings.jpg",
                    "http://image.toutlecine.com/photos/a/l/e/alerte-a-malibu-89-tv-11-g.jpg",
                    "http://i488.photobucket.com/albums/rr250/ms_pw/Hasselhoffer/hasselhoff-wallpaper-3.jpg",
                    "http://www.recordsale.de/cdpix/d/david_hasselhoff-crazy_for_you(bmg).jpg",
                    "http://1.bp.blogspot.com/-Ae21Rhzbf-4/TwLbV9AVNnI/AAAAAAAABhA/2wNZfHBe1Tg/s1600/David-Hasselhoff-Pictures-HD-5.jpg",
                };
            var index = new Random().Next(0, pics.Length-1);
            return pics[index];
        }
    }
}
