using Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class CheckFileExtensions
    {
        public static List<string> ImageExtensions = new List<string>() {"png","PNG","jpeg","JPEG", "jpg", "JPG", "BMP","bmp","GIF","gif","TIFF","tiff"};
        public static List<string> VideoExtensions = new List<string>() { "mp4","MP4"};

        private static string getExtension(string name)
        {
            string[] fields = name.Split('.');
            return fields[fields.Length - 1];
        }
        public static bool IsAnImage(string fileName)
        {
            
            if(ImageExtensions.Contains(getExtension(fileName)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsAVideo(string fileName)
        {

            if (VideoExtensions.Contains(getExtension(fileName)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsAllowedType(string filename, string type)
        {
            switch(type)
            {
                case DataFormat.Image: return IsAnImage(filename);
                case DataFormat.Video: return IsAVideo(filename);
            }
            return false;
        }
        public static bool IsAllowedType(string filename, List<string> types)
        {
            foreach(string type in types)
            {
                if(IsAllowedType(filename,type))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
