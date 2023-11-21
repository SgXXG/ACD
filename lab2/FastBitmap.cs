using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ACG_1
{
    public class FastBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }

        // Массив целых чисел, который представляет собой пиксели изображения.
        // Каждый элемент массива представляет собой цвет одного пикселя в
        // формате ARGB (Alpha, Red, Green, Blue).
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        public int bitmapColor = System.Drawing.Color.Gray.ToArgb();

        // Дескриптор управления памятью для массива Bits. Он используется
        // для закрепления массива в памяти, чтобы избежать его сборки
        // сборщиком мусора.
        protected GCHandle BitsHandle { get; private set; }

        public static FastBitmap FromBitmap(Bitmap bitmap)
        {
            var fastBitmap = new FastBitmap(bitmap.Width, bitmap.Height);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int g = 0; g < bitmap.Height; g++)
                {
                    fastBitmap.Bits[g * bitmap.Width + i] = bitmap.GetPixel(i, g).ToArgb();
                }
            }
            return fastBitmap;
        }
        public FastBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void Clear()
        {
            Array.Fill<Int32>(Bits, bitmapColor);
        }
        public void SetPixel(int index, Color colour)
        {
            int col = colour.ToArgb();
            Bits[index] = col;
        }

        public void SetPixel(int index, int color)
        {
            Bits[index] = color;
        }

        public void SetPixel(int x, int y, int color)
        {
            Bits[x + (y * Width)] = color;
        }

        public Color GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            var color = Color.FromArgb(Bits[index]);
            return color;
        }

        public void Dispose()
        {
            BitsHandle.Free();
        }
    }
}
