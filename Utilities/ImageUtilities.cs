using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Net;
using System.Drawing.Imaging;

namespace Utilities
{
    public static class ImageUtilities
    {
        public static Image getImageFromURI(string uri)
        {
            var wc = new WebClient();
            Image x = Image.FromStream(wc.OpenRead(uri));
            return x;
        }

        public static void saveImage(Image im, string directory, string fileName)
        {
            string finalFileName = directory + "\\" + fileName + ".jpg";
            im.Save(finalFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        public static byte[] readLocalPNGRawData(string filepath,  out int width, out int height, PixelFormat f = PixelFormat.Format8bppIndexed)
        {
            Bitmap x = (Bitmap)Bitmap.FromFile(filepath);
            return Bitmap2ByteArray(x, out width, out height, f);
        }

        public static byte[] readPNGRawDataFromURL(string URL, out int width, out int height, 
            PixelFormat f = PixelFormat.Format8bppIndexed)
        {
            WebClient wb = new WebClient();
            Bitmap x = (Bitmap)Bitmap.FromStream(wb.OpenRead(URL));
            return Bitmap2ByteArray(x, out width, out height,f);
        }

        public static byte[] Bitmap2ByteArray(Bitmap x, out int width, out int height, 
            PixelFormat f = PixelFormat.Format8bppIndexed)
        {
            BitmapData bmapdata = x.LockBits(new Rectangle(0, 0, x.Width,
                 x.Height), ImageLockMode.ReadWrite, f);
            IntPtr ptr = bmapdata.Scan0;

            int bytes = Math.Abs(bmapdata.Stride) * x.Height;
            width = bmapdata.Stride;
            height = x.Height;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            //// Set every third value to 255. A 24bpp bitmap will look red.  
            //for (int counter = 0; counter < rgbValues.Length; counter ++)
            //    Console.WriteLine(rgbValues[counter]);

            return rgbValues;
        }


        public static void savePNGRawData(string filepath, int width, int height, byte[] data, PixelFormat f = PixelFormat.Format8bppIndexed)
        {

            if (data.Length==0)
            {
                Console.WriteLine("Input data length is 0");
                return;
            }
            Bitmap x = new Bitmap(width, height, f);
            BitmapData bmapdata = x.LockBits(new Rectangle(0, 0, x.Width,
                 x.Height), ImageLockMode.ReadWrite, f);
            IntPtr ptr = bmapdata.Scan0;

            int bytes = width * height;

            if (bytes > data.Length) return;

            System.Runtime.InteropServices.Marshal.Copy(data, 0, ptr, bytes);
            // Unlock the bits.
            x.UnlockBits(bmapdata);

            x.Save(filepath);
        }
    }
}
