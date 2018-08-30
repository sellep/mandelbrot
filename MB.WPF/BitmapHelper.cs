using MB.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MB.WPF
{

    public class BitmapHelper
    {

        public static BitmapSource Create(int width, int height, int[,] iframe, SolidColorBrush[] palette, SolidColorBrush notset)
        {
            byte[] raw = new byte[width * height * 3];

            int i, j;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    i = y * width * 3 + x * 3;
                    j = iframe[x, y];

                    SolidColorBrush b = j == FrameHelper.DEFAULT
                        ? notset
                        : palette[j];

                    raw[i + 0] = b.Color.B;
                    raw[i + 1] = b.Color.G;
                    raw[i + 2] = b.Color.R;
                }
            }

            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr24, null, raw, width * 3);
        }
    }
}
