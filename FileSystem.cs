using System.Text.Json.Serialization;
using System.Text.Json;

namespace losertron4000
{
    static class FileSystem
    {
        private static Directory _dirs;

        public static void Init()
        {
            if (System.OperatingSystem.IsAndroid())
            {
                Path.GoodSep = '/';
                Path.BadSep = '\\';
            }

            using var stream = Microsoft.Maui.Storage.FileSystem.OpenAppPackageFileAsync("map.json").Result;
            using var reader = new StreamReader(stream);
            var rawJson = reader.ReadToEnd();


            _dirs = JsonSerializer.Deserialize<Directory>(rawJson);
        }

        public static Path[] GetDirectories() => GetDirectories(new());
        public static Path[] GetDirectories(Path path)
        {

            Directory targ = SeekToDir(path.DirectoryPath);

            Path[] paths = new Path[targ.Directories.Count];

            for (int i = 0; i < targ.Directories.Count; i++)
            {
                paths[i] = targ.Directories[i].FullPath;
            }

            return paths;
        }

        public static Path[] GetFiles(Path path)
        {
            var dir = SeekToDir(path.DirectoryPath);
            var files = new Path[dir.Files.Count];

            int i = 0;
            foreach (Path file in dir.Files)
            {
                files[i] = path.DirectoryPath / file;
                i++;
            }


            return files;
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
            var dir = SeekToDir(path.DirectoryPath);

            return dir.Files.Contains(path.FileName);
        }

        private static int FindDirectory(Directory dir, Path name)
        {
            for (int i = 0; i < dir.Directories.Count; i++)
            {
                if (dir.Directories[i].Name == name)
                    return i;
            }

            return -1;
        }

        private static Directory SeekToDir(Path path)
        {
            string[] folders = path.ToString().TrimStart('\\', '/').TrimEnd('\\', '/').Split(new char[] { '\\', '/' });

            Directory targ = _dirs;

            for (int i = 0; i < folders.Length; i++)
            {
                int maybe = FindDirectory(targ, folders[i]);

                if (maybe == -1)
                    throw new FileNotFoundException("Failed to seek to " + path);

                targ = targ.Directories[maybe];
            }

            return targ;
        }
    }


    public class Directory
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("fullPath")]
        public string FullPath { get; set; }

        [JsonPropertyName("dirs")]
        public List<Directory> Directories { get; set; }

        [JsonPropertyName("files")]
        public HashSet<string> Files { get; set; }
    }
    //public class Directory
    //{
    //    [JsonPropertyName("name")]
    //    public string name { get; set; }

    //    [JsonPropertyName("dirs")]
    //    public Directory[] directories { get; set; }

    //    [JsonPropertyName("files")]
    //    public string[] files { get; set; }

    //    public Directory(string name, Directory[] dirs, string[] files)
    //    {
    //        this.name = name;
    //        this.files = files;
    //        this.directories = dirs;
    //    }
    //}

    public class Path
    {
        private readonly string _path;


        public static char GoodSep = '\\';
        public static char BadSep = '/';



        public Path(string path)
        {
            _path = path.Replace(BadSep, GoodSep).TrimStart('\\', '/').TrimEnd('\\', '/');
        }
        public Path()
        {
            _path = string.Empty;
            //_path = path.EndsWith("\\") || path.EndsWith("/") ? path.Replace("\\", "/") : path.Replace("\\", "/");
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
                string parent = GetDirectoryName(_path);
                return parent != null ? new Path(parent) : null;
            }
        }

        public Path DirectoryPath
        {
            get
            {
                if (Extension == string.Empty)
                    return new(_path);

                string parent = GetDirectoryName(_path);
                return parent != null ? new Path(parent) : null;
            }
        }

        public string Extension
        {
            get
            {
                return System.IO.Path.GetExtension(_path);
            }
        }

        public Path RelativeTo(Path basePath)
        {
            Uri baseUri = new Uri(basePath._path.EndsWith(GoodSep)
                                  ? basePath._path
                                  : basePath._path + GoodSep);
            Uri targetUri = new Uri(_path);
            Uri relativeUri = baseUri.MakeRelativeUri(targetUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            return new Path(relativePath.Replace(BadSep, GoodSep));
        }

        public string FileName => GetFileName(_path);

        public static Path Cache { get { return Microsoft.Maui.Storage.FileSystem.CacheDirectory; } }
        public static string GetDirectoryName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            path = path.Trim();

            // Handle special cases for root or single-character paths.
            if (path.Length == 1 && (path == "\\" || path == "/"))
            {
                return null;
            }

            // Normalize slashes for consistency.
            path = path.Replace(BadSep, GoodSep);



            int lastSlashIndex = path.LastIndexOf(GoodSep);

            // If there are no slashes, the path has no directory component.
            if (lastSlashIndex == -1)
            {
                return null;
            }

            // If the last slash is at the beginning (e.g., "C:\"), handle it specially.
            if (lastSlashIndex == 0 || (lastSlashIndex == 2 && path[1] == ':'))
            {
                return path.Substring(0, lastSlashIndex + 1);
            }

            // Return the substring up to (but not including) the last slash.
            return path.Substring(0, lastSlashIndex);
        }

        public static string GetFileName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            path = path.Trim();

            // Normalize slashes for consistency.
            path = path.Replace(BadSep, GoodSep);



            int lastSlashIndex = path.LastIndexOf(GoodSep);

            // If there are no slashes, the entire path is the file name.
            if (lastSlashIndex == -1)
            {
                return path;
            }

            // Return the substring after the last slash.
            return path.Substring(lastSlashIndex + 1);
        }

    }
}
