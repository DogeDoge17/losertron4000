using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace losertron4000
{
    public partial class MainPage : ContentPage
    {
        private string _girl = "natsuki";

        public Dictionary<string, ObservableCollection<DokiExpression>> expressions;

        public GirlsGirling _girlDefaults;
        private List<DokiExpression> _selectedExpressions;

        private int _selectedGroup = 0;


        //private ImageSource _sample = ImageSource.FromResource("sample.png");

        private bool _loadingImages;

        public MainPage()
        {
            InitializeComponent();
            OnSizeAllocated(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);
            FileSystem.Init();           
            InitDokis();
        }

        /// <summary>
        /// Starts the initialization of all data related to the dokis
        /// </summary>
        private void InitDokis()
        {
            Path[] girlies = FileSystem.GetDirectories();

            for (int i = 0; i < girlies.Length; i++)
            {
                string filename = girlies[i].FileName;
                girlPicker.Items.Add(filename.First().ToString().ToUpper() + filename.Substring(1)); // makes the name capitalised to be all fancy
            }
            girlPicker.SelectedIndexChanged += ReinitDoki;

            girlPicker.SelectedIndex = girlPicker.Items.IndexOf(_girl.First().ToString().ToUpper() + _girl.Substring(1)); // indirectly calls ReinitDoki
        }

        /// <summary>
        /// Loads and sets up all the information of the new chosen doki
        /// </summary>
        private void ReinitDoki(object? sender, EventArgs e)
        {
            _girl = girlPicker.Items[girlPicker.SelectedIndex].ToLower();
            LoadImageData();
            ConstructImageButtons();
            ChooseDefaults();
            ConstructDoki(false);
        }

        /// <summary>
        /// Creates the tabs to switch expression categories and then creates images for the cache if they dont exist.
        /// Loads the defaults/doki specific info
        /// </summary>
        private void LoadImageData()
        {
            var folders = FileSystem.GetDirectories(_girl);
            expressions = new(folders.Length);
            tabGrid.Clear();

            _girlDefaults = JsonSerializer.Deserialize<GirlsGirling>(FileSystem.ReadFile(new Path(_girl) / "defaults.json"));

            List<Task> previewGenTasks = new List<Task>();

            for (int i = 0; i < folders.Length; i++)
            {
                var imgPaths = FileSystem.GetFiles(folders[i]);
                ObservableCollection<DokiExpression> list = new();
                for (int j = 0; j < imgPaths.Length; j++)
                {
                    list.Add(new(imgPaths[j]));

                    if (!Cache.TryLoadSource(list[j].Uri, out var img))
                    {
                        Path path = list[j].Uri;
                        previewGenTasks.Add(Task.Run(() => {
                            Image<Rgba32> bitmap = Bitmap.FullProcessImage(path, Bitmap.CropImage, Bitmap.PreviewSize);
                            Cache.Save(bitmap, path);
                        }));                        
                    }

                    list[j].Category = imgPaths[j].DirectoryPath.FileName;
                }

                Label tab = new Label();
                tab.Text = folders[i].FileName;
                tab.HorizontalTextAlignment = TextAlignment.Center;
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += OnTabClicked;
                tab.GestureRecognizers.Add(tapGestureRecognizer);

                tab.BackgroundColor = i != _selectedGroup ? Colors.Transparent : (Microsoft.Maui.Graphics.Color)Application.Current.Resources["Primary"]; ;//

                tabGrid.Add(tab, i, 0);
                expressions.Add(folders[i].FileName, list);
            }

            Task.WhenAll(previewGenTasks).Wait();
        }

        /// <summary>
        /// Adds the default expressions of the doki into the _selectedExpressions for them to be displayed (supplied by defaults.json)
        /// </summary>
        private void ChooseDefaults()
        {
            _selectedExpressions = new();

            List<DokiExpression> megaList = new List<DokiExpression>();
            for (int i = 0; i < expressions.Count; i++)
                megaList.AddRange(expressions.Values.ToList()[i]);

            HashSet<string> defaults = new();

            for (int i = 0; i < _girlDefaults.Folders.Count; i++)
                for (int j = 0; j < _girlDefaults.Folders[i].Default.Length; j++)
                    defaults.Add(_girlDefaults.Folders[i].Default[j]);

            megaList.ForEach(image => { if (defaults.Contains(new Path(image.Uri).FileName.ToString())) { _selectedExpressions.Add(image); image.BackgroundColor = Colors.Green; } });
            SortExpressions();
            UpdateWarnings(null);
        }

        /// <summary>
        /// Makes a bitmap and then draws all selected features of the doki onto it.
        /// </summary>
        /// <param name="export">Decides if it should use the bitmap in the preview image or to save it to the disk.</param>
        private async void ConstructDoki(bool export = false)
        {
            using Image<Rgba32> img = new Image<Rgba32>(960, 960);

            bool natsdown = false;
            if (_girl == "natsuki")
                natsdown = _selectedExpressions.FirstOrDefault(image => image.Uri.ToString().Contains("crossed"), null) != null;

            for (int i = 0; i < _selectedExpressions.Count; i++)
            {
                //ik ik ik its hardcoded but igdaf atp
                SixLabors.ImageSharp.Point drawOffset = natsdown && !_selectedExpressions[i].Uri.ToString().Contains("crossed") ? new SixLabors.ImageSharp.Point(18, 22) : new SixLabors.ImageSharp.Point(0, 0);
                img.Mutate(ctx => ctx.DrawImage(Bitmap.FileToImage(_selectedExpressions[i].Uri), drawOffset, 1));
            }

            if (!export)
            {
                Bitmap.CropImage(img);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    dokiPreview.Source = Bitmap.ImageToSource(img);                    
                });               
            }
            else
            {
                int i = 0;
                for (i = 0; File.Exists(Path.PhotosDirectory / $"{_girl}-{i}.png"); i++) ; // generates a file name

                if (!System.IO.Directory.Exists(Path.PhotosDirectory))
                    System.IO.Directory.CreateDirectory(Path.PhotosDirectory);

                img.SaveAsPng(Path.PhotosDirectory / $"{_girl}-{i}.png");
#if ANDROID
        Android.Media.MediaScannerConnection.ScanFile(Android.App.Application.Context, new string[] { Path.PhotosDirectory / $"{_girl}-{i}.png" }, null, null);
#endif

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Toast.Make($"{_girl} saved to {Path.PhotosDirectory / $"{_girl}-{i}.png"}", ToastDuration.Short, 14).Show().Wait();                                         
                });
            }

        }

        /// <summary>
        /// Loads the correct list of ImageButtons the selected tab and then sets up the list view to display them 
        /// </summary>
        private void ConstructImageButtons()
        {            
            if (_loadingImages)
                return;

            _loadingImages = true;
            
            ObservableCollection<DokiExpression> theWitch = expressions.Values.ToList()[_selectedGroup];

            for (int i = 0; i < theWitch.Count; i++)                          
                theWitch[i].TrueUri = Path.Cache / theWitch[i].Uri;
          
            new ColListView<DokiExpression>(theWitch, buttonListView, 3);

            _loadingImages = false;
        }        

        /// <summary>
        /// Ensures all features of the doki are layered correctly (sorts z-layer)
        /// </summary>
        private void SortExpressions()
        {
            var folderIndexMap = _girlDefaults.Folders.ToDictionary(folder => folder.Name, folder => folder.ZIndex);

            _selectedExpressions = _selectedExpressions.OrderBy(item => folderIndexMap.TryGetValue(item.Category, out int zIndex) ? zIndex : int.MaxValue).ToList();
        }       

        /// <summary>
        /// Finds any incompatible expresses with the last selected one and highlights them yellow.
        /// </summary>
        /// <param name="item">The category to base the conflicts off of</param>
        private void UpdateWarnings(DokiExpression? item)
        {
            string[] groupIds;

            if (item == null)
            {
                Dictionary<string[], int> occurences = new();
                for (int i = 0; i < _selectedExpressions.Count; i++)
                {
                    string[] group = _girlDefaults.Groups?.FirstOrDefault(ids => ids.Any(grp => _selectedExpressions[i].Uri.ToString().Contains(grp))) ?? new string[0];

                    if (group == null)
                        continue;

                    if (occurences.ContainsKey(group))
                        occurences[group]++;
                    else
                        occurences.Add(group, 0);
                }
                groupIds = occurences.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            }
            else
                groupIds = _girlDefaults.Groups?.FirstOrDefault(ids => ids.Any(grp => item.Uri.ToString().Contains(grp))) ?? new string[0];

            for (int i = 0; i < expressions.Count; i++)
            {
                var expGroup = expressions.Values.ToArray()[i];
                foreach (var image in expGroup)
                {                    
                    if (groupIds == null && (image.BackgroundColor != Colors.Green && image.BackgroundColor != Colors.YellowGreen))
                    {
                        image.BackgroundColor = Colors.Transparent;
                        continue;
                    }

                    if (!groupIds.Any(group => image.Uri.ToString().Contains(group)))
                    {
                        image.BackgroundColor = image.BackgroundColor == Colors.Green || image.BackgroundColor == Colors.YellowGreen ? Colors.YellowGreen : Colors.Yellow;
                    }
                    else if (image.BackgroundColor != Colors.Green && image.BackgroundColor != Colors.YellowGreen)
                        image.BackgroundColor = Colors.Transparent;
                }
            }

            for(int i = 0; i < _selectedExpressions.Count; i++)
            {
                if (_selectedExpressions[i].BackgroundColor == Colors.YellowGreen && groupIds.Any(group => _selectedExpressions[i].Uri.ToString().Contains(group)))
                    _selectedExpressions[i].BackgroundColor = Colors.Green;
            }
        }

        /// <summary>
        /// Counts how many elements matches the expression
        /// </summary>        
        /// <param name="source">The array to iterate through</param>
        /// <param name="predicate">The function that does the checking</param>
        /// <returns>The number of matchs</returns>
        private static int CountCon<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            int found = 0;

            foreach (TSource element in source)
            {
                if (predicate(element))
                {
                    found++;
                }
            }
            return found;
        }

        /// <summary>
        /// Ensures that the correct number of expressions are selected by category
        /// </summary>
        /// <param name="item">The Expression that contains the category to be toggled</param>
        private void ToggleOfType(DokiExpression item)
        {
            var folder = _girlDefaults.Folders.FirstOrDefault(folder => folder.Name == item.Category) ;

            if (folder.Max == -1) 
                return;

            int max = !folder.Bypass.Any(bypass => item.Uri.ToString().Contains(bypass)) ? folder.Max : 1;

            while (CountCon(_selectedExpressions, image => image.Category == item.Category) >= max)
            {
                var expression = _selectedExpressions.FirstOrDefault(image => image.Category == item.Category);
                expression.BackgroundColor = Colors.Transparent;
                _selectedExpressions.Remove(expression);
            }
        }

        /// <summary>
        /// Toggles the pressed expression between selected and not selected
        /// </summary>        
        private async void OnExpressionClicked(object sender, EventArgs e)
        {
            DokiExpression item;
            if (sender is ExpressionButton fakeItem)            
                item = expressions.SelectMany(keyVal => keyVal.Value).FirstOrDefault(imageItem => imageItem.Equals(fakeItem));            
            else
                item = (DokiExpression)sender;

            if (_selectedExpressions.IndexOf(item) == -1)
            {
                ToggleOfType(item);
                _selectedExpressions.Add(item);
                item.BackgroundColor = Colors.Green;
                UpdateWarnings(item);
            }
            else
            {
                item.BackgroundColor = Colors.Transparent;
                _selectedExpressions.Remove(item);
            }

            SortExpressions();
            await Task.Run(() => ConstructDoki(false));
        }

        /// <summary>
        /// Instructs ConstructDoki to save the doki to the disk
        /// </summary>
        private async void OnSavedClicked(object sender, EventArgs e)
        {
            await Task.Run(() => ConstructDoki(true));
        }

        /// <summary>
        /// changes the selected expression group depending on the tab clicked
        /// </summary>        
        private void OnTabClicked(object? sender, TappedEventArgs e)
        {
            if (!(sender is Label tabLbl))
                return;

            foreach (Label tab in tabGrid.Children)
            {
                tab.BackgroundColor = Colors.Transparent;
            }

            tabLbl.BackgroundColor = (Microsoft.Maui.Graphics.Color)Application.Current.Resources["Primary"];
            _selectedGroup = tabGrid.Children.IndexOf(tabLbl);
            ConstructImageButtons();
        }

        /// <summary>
        /// Ensures the ExpressionButtons are all 1:1 by setting the row height of the listview
        /// </summary>        
        private void ExpressionListSize(object? sender, EventArgs e)
        {
            buttonListView.RowHeight = (int)(buttonListView.Width / 3);
        }
        
        /// <summary>
        /// stops unnecessary layout shifts
        /// </summary>
        private byte _lastAspect = 3;

        /// <summary>
        /// Handles the layout changes when the orientation is modified
        /// </summary>
        /// <param name="width">The new width of the window</param>
        /// <param name="height">The new height of the window</param>
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            double aspectRatio = width / height;
            byte asp = (byte)(aspectRatio > 1 ? 1 : 0);

            if (_lastAspect == asp)
                return;

            _lastAspect = asp;

            if (asp == 1) // horizontal
            {
                centralGrid.ColumnDefinitions = new()
                {
                     new ColumnDefinition { Width = GridLength.Star },
                     new ColumnDefinition { Width = GridLength.Star },
                };

                imageGrid.RowDefinitions = new()
                {
                     new RowDefinition { Height = GridLength.Star },
                     new RowDefinition { Height = GridLength.Auto },
                };

                centralGrid.RowDefinitions = new();

                centralGrid.SetColumn(imageGrid, 1);
                centralGrid.SetColumn(bottomBit, 0);

                centralGrid.SetRow(imageGrid, 0);
                centralGrid.SetRow(bottomBit, 0);

                imageGrid.SetRow(dokiPreview, 0);
                imageGrid.SetRow(topBar, 1);
            }
            else // vertical
            {
                centralGrid.RowDefinitions = new()
                {
                     new RowDefinition { Height= GridLength.Star },
                     new RowDefinition { Height = GridLength.Star },
                };

                imageGrid.RowDefinitions = new()
                {
                     new RowDefinition { Height = GridLength.Auto },
                     new RowDefinition { Height = GridLength.Star },
                };

                centralGrid.ColumnDefinitions = new();

                centralGrid.SetRow(imageGrid, 0);
                centralGrid.SetRow(bottomBit, 1);

                centralGrid.SetColumn(imageGrid, 0);
                centralGrid.SetColumn(bottomBit, 0);

                imageGrid.SetRow(topBar, 0);
                imageGrid.SetRow(dokiPreview, 1);
            }
        }        
    }

}
