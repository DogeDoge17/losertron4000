using System.Text.Json.Serialization;
using System.Text.Json;

namespace losertron4000
{
    static class FileSystem
    {
        private static Directory _dirs;

        /// <summary>
        /// Sets the file separator and then loads the file map from map.json
        /// </summary>
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

        /// <summary>
        /// Gets all the directories in the base directory
        /// </summary>
        /// <returns>All directories found</returns>
        public static Path[] GetDirectories() => GetDirectories(new());

        /// <summary>
        /// Gets all subdirectories in a specific directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets all files in a directory
        /// </summary>
        /// <param name="path">The path to the folder to be searched</param>
        /// <returns>All files found</returns>
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

        /// <summary>
        /// Opens a file stream for the file at the spec
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Stream OpenFile(Path path)
        {
            return Microsoft.Maui.Storage.FileSystem.OpenAppPackageFileAsync(path).Result;
        }


        public static string ReadFile(Path path)
        {
            using var stream = OpenFile(path);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static bool FileExists(Path path)
        {
            var dir = SeekToDir(path.DirectoryPath);

            return dir.Files.Contains(path.FileName);
        }

        /// <summary>
        /// finds 
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static int FindDirectory(Directory dir, Path name)
        {
            for (int i = 0; i < dir.Directories.Count; i++)
            {
                if (dir.Directories[i].Name == name)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// The directories work essentially as linked lists by design so we have to iterate to get the <see cref="Directory"/>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        private static Directory SeekToDir(Path path)
        {
            if (path == string.Empty)
                return _dirs;

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

    /// <summary>
    /// copy of std::filesystem::path because i like it
    /// </summary>    
    public class Path
    {
        private readonly string _path;

#if WINDOWS
        public static char GoodSep = '\\';
        public static char BadSep = '/';
#else
        public static char GoodSep = '/';
        public static char BadSep = '\\';
#endif

        public Path(string path) => _path = path.Replace(BadSep, GoodSep).TrimStart('\\', '/').TrimEnd('\\', '/');
        public Path() => _path = string.Empty;

        public static implicit operator string(Path p) => p._path;
        public static implicit operator Path(string s) => new Path(s);

        /// <summary>
        /// Concatenates two paths with the appropriate separators 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Path operator /(Path left, Path right)
        {
            return new Path((left._path.TrimEnd('\\', '/') + GoodSep + right._path.TrimStart('\\', '/')).Replace(BadSep, GoodSep));
        }

        public override string ToString() => _path;        

        /// <summary>
        /// Gets the name of the parent directory
        /// </summary>
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

        /// <summary>
        /// Gets the .* of the file
        /// </summary>
        public string Extension => System.IO.Path.GetExtension(_path);

#if ANDROID
        public static Path PhotosDirectory = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures)?.AbsolutePath / new Path("losertron4000");
#elif IOS || MACCATALYST
        public static Path PhotosDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) / new Path("losertron4000");
#elif WINDOWS
public static Path PhotosDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) / new Path("losertron4000");
#endif        

        public string FileName => GetFileName(_path);

        /// <summary>
        /// Shorthand to access the app's cache directory
        /// </summary>
        public static Path Cache => Microsoft.Maui.Storage.FileSystem.CacheDirectory;

        public static string GetDirectoryName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            path = path.Trim();

            if (path.Length == 1 && (path == "\\" || path == "/"))
            {
                return path;
            }

            path = path.Replace(BadSep, GoodSep);

            int lastSlashIndex = path.LastIndexOf(GoodSep);

            if (lastSlashIndex == -1)
            {
                return path;
            }

            if (lastSlashIndex == 0 || (lastSlashIndex == 2 && path[1] == ':'))
            {
                return path.Substring(0, lastSlashIndex + 1);
            }

            return path.Substring(0, lastSlashIndex);
        }

        public static string GetFileName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            path = path.Trim();

            path = path.Replace(BadSep, GoodSep);

          
            int lastSlashIndex = path.LastIndexOf(GoodSep);

            if (lastSlashIndex == -1)
            {
                return path;
            }

            return path.Substring(lastSlashIndex + 1);
        }

    }
}
