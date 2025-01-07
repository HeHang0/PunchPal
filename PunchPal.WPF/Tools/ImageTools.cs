using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace PunchPal.WPF.Tools
{
    public static class ImageTools
    {
        public static ImageSource AppIcon
        {
            get
            {
                var appIcon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
                return appIcon?.ToBitmapSource();
            }
        }

        public static ImageSource ToBitmapSource(this Bitmap bmp)
        {
            BitmapFrame bf;

            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bf = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
            return bf;
        }

        public static ImageSource ToBitmapSource(this Icon icon)
        {
            return Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
