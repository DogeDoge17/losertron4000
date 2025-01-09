using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

namespace losertron4000
{
    static class FileSystem
    {
        static HashSet<string> allDirs = new();

        public static async void Init()
        {
            using var stream = await Microsoft.Maui.Storage.FileSystem.OpenAppPackageFileAsync("map.txt");
            using var reader = new StreamReader(stream);
            var contents = reader.ReadToEnd().Replace("\\", "/").Replace("\r", "").Trim().Split("\n");

            allDirs = new(contents.Length, StringComparer.Ordinal);

            for (int i = 0; i < contents.Length; i++)
            {
                allDirs.Add(new Path(contents[i]));
            }            
        }

        public static Path[] GetDirectories(string path = "", string searchPattern = "*", bool recursive = false)
        {
            path = path.Replace('\\', '/');
            string normalizedPath = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";

            Regex patternRegex = new Regex("^" + Regex.Escape(searchPattern).Replace("\\*", ".*") + "$", RegexOptions.IgnoreCase);

            return allDirs
            .Where(entry => string.IsNullOrEmpty(normalizedPath) || entry.StartsWith(normalizedPath))
            .Select(entry => string.IsNullOrEmpty(normalizedPath) ? entry.Split('/')[0] : entry.Substring(normalizedPath.Length).Split('/')[0])
            .Where(subEntry => !string.IsNullOrEmpty(subEntry) && patternRegex.IsMatch(subEntry))
            .Distinct()
            .Select(subEntry => string.IsNullOrEmpty(normalizedPath) ? subEntry : normalizedPath + subEntry)
            .Where(entry => allDirs.Any(e => e.StartsWith(entry + "/")))
            .Select(p => new Path(p)).ToArray();           
        }

        public static Path[] GetFiles(string path = "", string searchPattern = "*", bool recursive = false)
        {
            path = path.Replace('\\', '/');
            string normalizedPath = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/');

            Regex patternRegex = new Regex("^" + Regex.Escape(searchPattern).Replace("\\*", ".*") + "$", RegexOptions.IgnoreCase);

            return allDirs
                .Where(entry => string.IsNullOrEmpty(normalizedPath) || entry.ToString().StartsWith(normalizedPath))
                .Where(entry => !entry.ToString().EndsWith("/")) // Files do not end with '/'
                .Select(entry => string.IsNullOrEmpty(normalizedPath) ? entry : entry.ToString().Substring(normalizedPath.Length))
                .Where(subEntry => patternRegex.IsMatch(subEntry.Split('/')[0]))
                .Where(subEntry => !recursive && !subEntry.Contains('/') || recursive) // Check recursion depth
                .Select(subEntry => normalizedPath + subEntry)
                .Select(p => new Path(p)).ToArray();
        }

        public static Stream OpenFile(Path path)
        {
            return Microsoft.Maui.Storage.FileSystem.OpenAppPackageFileAsync(path).Result;
        }

        public static MemoryStream OpenFileMem(Path path)
        {
            return (MemoryStream)Microsoft.Maui.Storage.FileSystem.OpenAppPackageFileAsync(path).Result;
        }

        public static string ReadFile(Path path)
        {
            using var stream = OpenFile(path);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static byte[] ReadAllBytes(Path path)
        {
            using var stream = OpenFile(path);
            using var reader = new BinaryReader(stream);
            return reader.ReadBytes((int)stream.Length);

        }

        public static bool FileExists(Path path)
        {
            return allDirs.Contains(path) || allDirs.Any(entry => entry.StartsWith(path));
        }
    }


    public class Path
    {
        private readonly string _path;

        public Path(string path)
        {
            _path = path.EndsWith("\\") || path.EndsWith("/") ? path.Replace("\\", "/") : path.Replace("\\", "/");
        }

        public static implicit operator string(Path p) => p._path;


        public static implicit operator Path(string s) => new Path(s);

        public static Path operator /(Path left, Path right)
        {
            return new Path(System.IO.Path.Combine(left._path, right._path));
        }

        public override string ToString()
        {
            return _path;
        }
        public Path ParentPath
        {
            get
            {
                string parent = System.IO.Path.GetDirectoryName(_path);
                return parent != null ? new Path(parent) : null;
            }
        }

        public Path RelativeTo(Path basePath)
        {
            Uri baseUri = new Uri(basePath._path.EndsWith("\\")
                                  ? basePath._path
                                  : basePath._path + "\\");
            Uri targetUri = new Uri(_path);
            Uri relativeUri = baseUri.MakeRelativeUri(targetUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            return new Path(relativePath.Replace("/", "\\\\"));
        }

        public string FileName => System.IO.Path.GetFileName(_path);
    }
}
