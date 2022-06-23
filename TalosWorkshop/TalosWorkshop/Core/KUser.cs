using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace TalosWorkshop.Core
{
    public class KUser
    {
        private static string ImagesPrefix = @"Z:\talos_www";
        private static string WebImagesPath = @"\assets\images\wallpapers\";

        private static string FullPath => ImagesPrefix + WebImagesPath;
        public int ID { get; } = -1;
        public string Username { get; set; } = "default_user";
        public string PersoanlID { get; set; } = "";
        public string Password { get; set; } = "";
        public bool IsMessagesAllowed { get; set; } = true;

        public string HumanReadableUsername { get
            {
                string result = "";
                if (!String.IsNullOrEmpty(Username)) result += Username;

                if (String.IsNullOrEmpty(result))
                {
                    result += PersoanlID;
                }
                else
                {
                    if (!String.IsNullOrEmpty(PersoanlID)) result += $" [{PersoanlID}]";
                }

                return result;
            } }

        private string _wallpaper = "";
        public string Wallpaper {
            get
            {
                if (String.IsNullOrEmpty(_wallpaper)) return "";
                string short_path = _wallpaper.Replace('/', '\\').Replace(WebImagesPath, "");
                return Path.Combine(FullPath, short_path);
            }
            set
            {
                if (value.StartsWith('/') || String.IsNullOrEmpty(value))
                {
                    _wallpaper = value;
                    return;
                }
                if (File.Exists(value))
                {
                    FileInfo info = new(value);
                    string n_val = Path.Combine(FullPath, info.Name);
                    File.Copy(value, n_val,true);
                    _wallpaper = n_val.Replace(ImagesPrefix,"").Replace('\\','/');
                }
                else
                {
                    throw new FileNotFoundException("Файл с обоями рабочего стола не найден");
                }
            }
        }
        public string DBWallpaper => _wallpaper;
        public KUser() { }
        public KUser(int ID,string Wallpaper = "")
        {
            this.ID = ID;
            _wallpaper = Wallpaper;
        }
    }
}
