using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace LogonAcceptanceWindow
{
    public class Utils
    {
/*        public static System.Windows.Media.Color AdjustColorBrightness(System.Drawing.Color color, double adjustment)
        {
            int r = color.R;
            int g = color.G;
            int b = color.B;
            r += (int)Math.Floor(adjustment * r);
                if (r > 255) r = 255;
                if (r < 0) r = 0;
                b += (int)Math.Floor(adjustment * g);
                if (b > 255) b = 255;
                if (b < 0) b = 0;
                g += (int)Math.Floor(adjustment * b);
                if (g > 255) g = 255;
                if (g < 0) g = 0;

                System.Drawing.Color result = System.Drawing.Color.FromArgb(255, r, g, b);
                return System.Windows.Media.Color.FromRgb(result.R, result.G, result.B);

        }*/

        public static SolidColorBrush GetColorBrush(String color) {
            SolidColorBrush brush = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(color));
            return brush;
        }
    }
}
