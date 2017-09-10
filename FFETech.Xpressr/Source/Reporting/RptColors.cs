/*

This file is part of Xpressr.

Copyright 2017 FFE-Tech e. U. Inh. Florian Feilmeier
website: www.ffe-tech.com
contact: info@ffe-tech.com

Xpressr is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Xpressr is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with Xpressr.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;

// Farbkonvertierung:
// http://www.mycsharp.de/wbb2/thread.php?threadid=78941
namespace FFETech.Xpressr.Reporting
{
    public static class RptColors
    {
        #region Fields

        public const int None = -1;
        public const int Black = 0;
        public const int Navy = 8388608;
        public const int Gray = 8421504;
        public const int Blue = 16711680;
        public const int Maroon = 128;
        public const int Purple = 8388736;
        public const int Red = 255;
        public const int Fuchsia = 16711935;
        public const int Green = 32768;
        public const int Teal = 8421376;
        public const int Lime = 65280;
        public const int Aqua = 16776960;
        public const int Olive = 32896;
        public const int Silver = 12632256;
        public const int Yellow = 65535;
        public const int White = 16777215;
        public const int SaddleBrown = 1262987;

        #endregion

        #region Constructors

        static RptColors()
        {
        }

        #endregion

        #region Public Static Methods

        public static int RGBToColor(byte r, byte g, byte b)
        {
            return r | g << 8 | b << 16;
        }

        public static void ColorToRGB(int color, out byte r, out byte g, out byte b)
        {
            r = (byte)(color & 255);
            g = (byte)((color >> 8) & 255);
            b = (byte)((color >> 16) & 255);
        }

        public static string ColorToHEX(int color)
        {
            byte r, g, b;
            ColorToRGB(color, out r, out g, out b);
            return "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
        }

        public static int HEXToColor(string color)
        {
            color = color.TrimStart('#');
            if (color.Length != 6)
                throw new InvalidOperationException("Invalid hex color");

            byte r, g, b;

            r = byte.Parse(color.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(color.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(color.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return RGBToColor(r, g, b);
        }

        public static void RGBToHSL(byte r, byte g, byte b, out double h, out double s, out double l)
        {
            double _r = (r / 255d);
            double _g = (g / 255d);
            double _b = (b / 255d);

            double _min = Math.Min(Math.Min(_r, _g), _b);
            double _max = Math.Max(Math.Max(_r, _g), _b);
            double _delta = _max - _min;

            h = 0;
            s = 0;
            l = ((_max + _min) / 2.0d);

            if (_delta != 0)
            {
                if (l < 0.5d)
                    s = (_delta / (_max + _min));
                else
                    s = (_delta / (2.0d - _max - _min));

                if (_r == _max)
                    h = (_g - _b) / _delta;
                else if (_g == _max)
                    h = 2d + (_b - _r) / _delta;
                else if (_b == _max)
                    h = 4d + (_r - _g) / _delta;
            }
        }

        public static void HSLToRGB(double h, double s, double l, out byte r, out byte g, out byte b)
        {
            if (s == 0)
            {
                r = (byte)Math.Round(l * 255d);
                g = (byte)Math.Round(l * 255d);
                b = (byte)Math.Round(l * 255d);
            }
            else
            {
                double t1, t2;
                double th = h / 6.0d;

                if (l < 0.5d)
                    t2 = l * (1d + s);
                else
                    t2 = (l + s) - (l * s);

                t1 = 2d * l - t2;

                double tr, tg, tb;
                tr = th + (1.0d / 3.0d);
                tg = th;
                tb = th - (1.0d / 3.0d);

                tr = HSLColorCalc(tr, t1, t2);
                tg = HSLColorCalc(tg, t1, t2);
                tb = HSLColorCalc(tb, t1, t2);

                r = (byte)Math.Round(tr * 255d);
                g = (byte)Math.Round(tg * 255d);
                b = (byte)Math.Round(tb * 255d);
            }
        }

        public static void AdjustBrightnessByRGB(ref byte r, ref byte g, ref byte b, double factor)
        {
            double h, s, l;
            RGBToHSL(r, g, b, out h, out s, out l);
            HSLToRGB(h, s, l + ((1 - l) * factor), out r, out g, out b);
        }

        public static int AdjustBrightness(int color, double factor)
        {
            byte r, g, b;
            ColorToRGB(color, out r, out g, out b);
            AdjustBrightnessByRGB(ref r, ref g, ref b, factor);
            return RGBToColor(r, g, b);
        }

        public static int SelectForeColor(int color, int darkcolor, int lightcolor)
        {
            byte r, g, b;
            ColorToRGB(color, out r, out g, out b);
            if (r < 80 && g < 80 && b < 80)
                return lightcolor;
            else
                return darkcolor;
        }

        public static int SelectForeColor(int color)
        {
            return SelectForeColor(color, Black, White);
        }

        public static int[] GenerateColorSeries(int count)
        {
            List<int> result = new List<int>();
            Random random = new Random();
            int max = RGBToColor(255, 255, 255);

            while (result.Count < count)
                result.Add(random.Next(max));

            return result.ToArray();
        }

        #endregion

        #region Private Static Methods

        private static double HSLColorCalc(double c, double t1, double t2)
        {
            if (c < 0)
                c += 1d;
            if (c > 1)
                c -= 1d;
            if (6.0d * c < 1.0d)
                return t1 + (t2 - t1) * 6.0d * c;
            if (2.0d * c < 1.0d)
                return t2;
            if (3.0d * c < 2.0d)
                return t1 + (t2 - t1) * (2.0d / 3.0d - c) * 6.0d;
            return t1;
        }

        #endregion
    }
}