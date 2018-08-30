using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MB.WPF
{

    public class ColorInterpolator
    {
        public static Color[] CreatePalette(int colorPadding, Color begin, Color end, int betweenRandColors)
        {
            Random rand = new Random();
            List<Color> bias = new List<Color>();

            bias.Add(begin);

            PropertyInfo[] props = typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static);

            for (int i = 0; i < betweenRandColors; i++)
            {
                bias.Add((Color)props[rand.Next(props.Length)].GetValue(null));
            }

            bias.Add(end);

            return CreatePalette(colorPadding, bias.ToArray());
        }

        private static Color[] CreatePalette(int colorPadding, params Color[] bias)
        {
            Color[] palette = new Color[colorPadding * (bias.Length - 1)];

            for (int i = 0; i < bias.Length - 1; i++)
            {
                InterpolateInto(bias[i], bias[i + 1], colorPadding, palette, i * colorPadding);
            }

            return palette;
        }

        private static void InterpolateInto(Color a, Color b, int steps, Color[] map, int startIndex)
        {
            double r_diff = ((double)b.R - a.R) / steps;
            double g_diff = ((double)b.G - a.G) / steps;
            double b_diff = ((double)b.B - a.B) / steps;

            double r_cur = a.R;
            double g_cur = a.G;
            double b_cur = a.B;

            for (int i = 0; i < steps; i++)
            {
                map[startIndex + i] = Color.FromRgb((byte)r_cur, (byte)g_cur, (byte)b_cur);

                r_cur += r_diff;
                g_cur += g_diff;
                b_cur += b_diff;
            }
        }
    }
}
