using System;
using System.Collections.Generic;
using System.Drawing;
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

        public static SolidColorBrush? GetColorBrush(String? color) {
            SolidColorBrush brush;
            if (IsValidColor(color))
            {
                var mediaColor = System.Windows.Media.ColorConverter.ConvertFromString(color);
                brush = new SolidColorBrush((System.Windows.Media.Color)mediaColor);
                return brush;
            }
            return null;
        }
        //Determine if color is not null and is a valid color
        private static Boolean IsValidColor(String? color) {
            
            if ((color is not null) || (color != ""))
            {
                try
                {
                    var colorConverts = System.Windows.Media.ColorConverter.ConvertFromString(color);
                    return true;
                }
                catch
                {
                    //Bad color value log to file
                    return false;
                }
            }
            else { 
                return false;
            }
        }
    }
}
