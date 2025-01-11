using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace losertron4000
{
    public static class Cache
    {
        public static void Save(object item, Path path)
        {
            if (!System.IO.Directory.Exists((Path.Cache / path).DirectoryPath))
                System.IO.Directory.CreateDirectory((Path.Cache / path).DirectoryPath);

            File.WriteAllText(Path.Cache / path, JsonSerializer.Serialize(item));           
        }

        public static void Save(Image<Rgba32> image, Path path)
        {
            if (!System.IO.Directory.Exists((Path.Cache / path).DirectoryPath))
                System.IO.Directory.CreateDirectory((Path.Cache / path).DirectoryPath);

            image.SaveAsPng(Path.Cache / path);
        }

        public static async Task SaveAsync(Image<Rgba32> image, Path path)
        {
            if (!System.IO.Directory.Exists((Path.Cache / path).DirectoryPath))
                System.IO.Directory.CreateDirectory((Path.Cache / path).DirectoryPath);

            //await image.SaveAsPngAsync(Path.Cache / path);

            await image.SaveAsPngAsync(Path.Cache / path);
        }


        public static bool TryLoad<T>(Path path, out T targ)
        {
            if (!File.Exists(Path.Cache / path))
            {
                targ = default;
                return false;
            }
            using (var stream = File.OpenRead(Path.Cache / path))
                targ = JsonSerializer.Deserialize<T>(stream);

            return true;
        }
        public static bool TryLoadImage(Path path, out Image<Rgba32> targ)
        {
            if (!File.Exists(Path.Cache / path))
            {
                targ = default;
                return false;
            }

            targ = SixLabors.ImageSharp.Image.Load<Rgba32>(Path.Cache / path);

            return true;
        }

        public static bool TryLoadSource(Path path, out ImageSource targ)
        {
            if (!File.Exists(Path.Cache / path))
            {
                targ = default;
                return false;
            }

            targ = ImageSource.FromFile(Path.Cache / path);

            return true;
        }
    }
}
