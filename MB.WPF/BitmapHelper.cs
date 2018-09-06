using MB.Core;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace MB.WPF
{

    public class BitmapHelper
    {

        public static BitmapSource Create(int width, int height, int[,] iframe, Color[] palette, Color notset)
        {
            byte[] raw = new byte[width * height * 3];

            int i, j;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    i = y * width * 3 + x * 3;
                    j = iframe[x, y];

                    Color color = j == FrameHelper.DEFAULT
                        ? notset
                        : palette[j];

                    raw[i + 0] = color.B;
                    raw[i + 1] = color.G;
                    raw[i + 2] = color.R;
                }
            }

            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr24, null, raw, width * 3);
        }
    }
}
