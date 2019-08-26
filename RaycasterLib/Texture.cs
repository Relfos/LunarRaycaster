using System;
using System.Drawing;

namespace LunarLabs.Raycaster
{
    public class Texture
    {
        public readonly int Width;
        public readonly int Height;
        public readonly byte[] Pixels;
        public bool hasAlpha { get; private set; }

        public Texture(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.Pixels = new byte[width * height * 4];
            this.hasAlpha = false;
        }

        public void SetPixel(int x, int y, byte r, byte g, byte b, byte a = 255)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
            {
                return;
            }

            int ofs = (y * Width + x) * 4;
            Pixels[ofs + 0] = (byte)r;
            Pixels[ofs + 1] = (byte)g;
            Pixels[ofs + 2] = (byte)b;
            Pixels[ofs + 3] = (byte)a;

            if (a == 0)
            {
                hasAlpha = true;
            }
        }

        public void GetPixel(int x, int y, out byte r, out byte g, out byte b, out byte a)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
            {
                r = 0;
                g = 0;
                b = 0;
                a = 0;
                return;
            }

            int ofs = (y * Width + x) * 4;
            r = Pixels[ofs + 0];
            g = Pixels[ofs + 1];
            b = Pixels[ofs + 2];
            a = Pixels[ofs + 3];
        }

        public byte GetChannel(int x, int y, int channel)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
            {
                return 0;
            }

            int ofs = (y * Width + x) * 4;
            return Pixels[ofs + channel];
        }

        public static Texture Crop(int x , int y, int width, int height, Func<int, int, Color> GetPixel)
        {
            var result = new Texture(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var color = GetPixel(x + i, y + j);
                    result.SetPixel(i, j, color.R, color.G, color.B, color.A);
                }
             }

            return result;
        }
    }
}
