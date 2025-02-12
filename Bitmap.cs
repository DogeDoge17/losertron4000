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

        public delegate void PreprocessImage(Image<Rgba32> img);

        public Bitmap(Path path)
        {
            if (FileSystem.FileExists(path))
                stream = FileSystem.OpenFile(path);
            else
                throw new FileNotFoundException();
        }

        public Bitmap(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Converts an ImageSharp image to a usable MAUI image source
        /// </summary>
        public static ImageSource ImageToSource(Image<Rgba32> img)
        {
            var stream = new MemoryStream();
            img.Save(stream, new PngEncoder());
            stream.Position = 0;
            return ImageSource.FromStream(() => stream);
            
        }

        /// <summary>
        /// Loads a file and then pipes the stream into an <see cref="SixLabors.ImageSharp.Image"/>
        /// </summary>
        /// <param name="path"> </param>
        /// <exception cref="FileNotFoundException"></exception>
        public static Image<Rgba32> FileToImage(Path path)
        {
            if (FileSystem.FileExists(path))
            {
                using var stream = FileSystem.OpenFile(path);
                return SixLabors.ImageSharp.Image.Load<Rgba32>(stream);
            }
            else
                throw new FileNotFoundException();
        }

        public static ImageSource FullProcessSource(Path path, params PreprocessImage[] funcs)
        {
            using var img = FileToImage(path);

            for(int i = 0; i < funcs.Length; i++)
            {
               funcs[i](img);
            }

            return ImageToSource(img);
        }
        public static Image<Rgba32> FullProcessImage(Path path, params PreprocessImage[] funcs)
        {
            var img = FileToImage(path);

            for (int i = 0; i < funcs.Length; i++)
            {
                funcs[i](img);
            }

            return img;
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
        }

        public static void PreviewSize(Image<Rgba32> img) => img.Mutate(ctx => ctx.Resize(250, 0)); //img.Mutate(ctx => ctx.Resize(0, 494));
        public static void NoTransparency(Image<Rgba32> img) => img.Mutate(ctx => ctx.BackgroundColor(SixLabors.ImageSharp.Color.ParseHex("2c2c2c")));

        public static void CropImage(Image<Rgba32> img)
        {
            int minX = img.Width;
            int minY = img.Height;
            int maxX = 0;
            int maxY = 0;

            img.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {                    
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        if (row[x].A != 0)
                        {
                            if (x < minX) minX = x;
                            if (y < minY) minY = y;
                            if (x > maxX) maxX = x;
                            if (y > maxY) maxY = y;
                        }
                    }
                }
            });

            if (minX > maxX || minY > maxY)           
                return;
            
            Rectangle sourceRect = new Rectangle(minX, minY, (maxX - minX) + 1, (maxY - minY) + 1);
            img.Mutate(ctx => ctx.Crop(sourceRect));
        }

        ~Bitmap()
        {
            //if(stream.GetType() != typeof(MemoryStream))
            //    stream.Dispose();
        }

        
    }
}
