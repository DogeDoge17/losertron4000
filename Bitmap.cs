using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace losertron4000
{
    /// <summary>
    /// Provide seamless transition between ImageSharp and MAUI. Abstracts file loading
    /// </summary>
    public class Bitmap
    {
        public Stream stream;

        public Bitmap(Path path)
        {
            stream = FileSystem.OpenFile(path);
        }

        public Bitmap(Stream stream)
        {
            this.stream = stream;
        }

        public static implicit operator ImageSource(Bitmap bmp)
        {
            return ImageSource.FromStream(() => bmp.stream);
        }

        public static implicit operator Image<Rgba32>(Bitmap bmp)
        {
            return SixLabors.ImageSharp.Image.Load<Rgba32>(bmp.stream);
            //return ImageSource.FromStream(() => bmp.stream);
        }

        public static implicit operator Bitmap(Image<Rgba32> img)
        {
            MemoryStream stream = new MemoryStream();

            img.Save(stream, new PngEncoder());
            stream.Position = 0;
            return new(stream);
            //return SixLabors.ImageSharp.Image.Load<Rgba32>(bmp.stream);
            //return ImageSource.FromStream(() => bmp.stream);
        }

        public Bitmap CropImage()
        {
            using Image<Rgba32> img = this;


            int minW = int.MaxValue, minH = int.MaxValue;
            int maxW = int.MinValue, maxH = int.MinValue;

            if (img.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
            {
                Debug.WriteLine("Allowed to dangerous read");           
                Span<Rgba32> span = memory.Span;

                for (int i = 0; i < span.Length; i++)
                {
                    int y = i / img.Width;
                    int x = i % img.Width;

                    Rgba32 pixel = span[i];

                    if (pixel.A != 0)
                    {
                        if (minW > x) minW = x;
                        if (minH > y) minH = y;
                        if (maxW < x) maxW = x;
                        if (maxH < y) maxH = y;
                    }
                }
            }
            else
            {
                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        //img.span
                        //int index = y * img.Width + x;

                        Debug.WriteLine("Falling back onto slow");

                        Rgba32 pixel = img[x, y];

                        if (pixel.A != 0)
                        {
                            if (minW > x) minW = x;
                            if (minH > y) minH = y;
                            if (maxW < x) maxW = x;
                            if (maxH < y) maxH = y;
                        }

<<<<<<< Updated upstream
=======
                        //Debug.WriteLine($"Pixel at ({x}, {y}): R={pixel.R}, G={pixel.G}, B={pixel.B}, A={pixel.A}");
>>>>>>> Stashed changes
                    }
                }
            }

            Rectangle sourceRect = new Rectangle(minW, minH, maxW - minW, maxH - minH);

            img.Mutate(ctx => ctx.Crop(sourceRect));

            return img;
        }

        ~Bitmap()
        {
            stream.Dispose();
        }

        
    }
}
