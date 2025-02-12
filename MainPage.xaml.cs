using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace losertron4000
{
    public partial class MainPage : ContentPage
    {
        public static string girl = "natsuki";

        public Dictionary<string, ObservableCollection<ImageItem>> expressions;

        public GirlsGirling _girlDefaults;
        private List<ImageItem> _selectedExpressions;

        private int _selectedGroup = 0;


        //private ImageSource _sample = ImageSource.FromResource("sample.png");

        private bool _loadingImages;

        public MainPage()
        {
            InitializeComponent();
            OnSizeAllocated(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);
            FileSystem.Init();           
            LoadGirls();

        }

        private void LoadGirls()
        {
            Path[] girlies = FileSystem.GetDirectories();

            for (int i = 0; i < girlies.Length; i++)
            {
                string filename = girlies[i].FileName;
                girlPicker.Items.Add(filename.First().ToString().ToUpper() + filename.Substring(1));
            }
            girlPicker.SelectedIndexChanged += ReinitGirls;

            girlPicker.SelectedIndex = girlPicker.Items.IndexOf("Natsuki");
        }

        private void ReinitGirls(object? sender, EventArgs e)
        {
            girl = girlPicker.Items[girlPicker.SelectedIndex].ToLower();
            LoadImageData();
            ConstructImageButtons();
            ChooseDefaults();
            ConstructDoki(false);
        }

        private void LoadImageData()
        {
            var folders = FileSystem.GetDirectories(girl);
            expressions = new(folders.Length);
            tabGrid.Clear();

            _girlDefaults = JsonSerializer.Deserialize<GirlsGirling>(FileSystem.ReadFile(new Path(girl) / "defaults.json"));

            for (int i = 0; i < folders.Length; i++)
            {
                var imgPaths = FileSystem.GetFiles(folders[i]);
                ObservableCollection<ImageItem> list = new();
                for (int j = 0; j < imgPaths.Length; j++)
                {
                    list.Add(new(imgPaths[j]));

                    if (!Cache.TryLoadSource(list[j].Uri, out var img))
                    {
                        Image<Rgba32> bitmap = Bitmap.FullProcessImage(list[j].Uri, Bitmap.CropImage, Bitmap.PreviewSize);
                        Cache.Save(bitmap, list[j].Uri);
                    }

                    list[j].Category = imgPaths[j].DirectoryPath.FileName;
                    //list[j].Aspect = Aspect.AspectFit;


                    //list[j].Clicked += OnExpressionClicked;
                    //list[j].SizeChanged += (s, e) =>
                    //{
                    //    DokiExpression img = (DokiExpression)s;
                    //    if (img.Width > 0)
                    //    {
                    //        img.HeightRequest = img.Width;  // Ensure the image stays square
                    //    }
                    //};
                }

                Border border = new();

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
        }

        private void ExpressionListSize(object? sender, EventArgs e)
        {
            buttonListView.RowHeight = (int)(buttonListView.Width / 3);
        }

        private void ExpressionSize(object? sender, EventArgs e)
        {
            //DokiExpression img = (DokiExpression)sender;
            //if (img.Width > 0 && img.HeightRequest != img.Width)
            //{
            //    img.HeightRequest = img.Width;
            //}
        }

        private void ChooseDefaults()
        {
            _selectedExpressions = new();

            List<ImageItem> megaList = new List<ImageItem>();
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



        private async void ConstructDoki(bool export = false)
        {
            using Image<Rgba32> img = new Image<Rgba32>(960, 960);

            bool natsdown = false;
            if (girl == "natsuki")
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
                for (i = 0; File.Exists(Path.PhotosDirectory / $"{girl}-{i}.png"); i++) ;

                if (!System.IO.Directory.Exists(Path.PhotosDirectory))
                    System.IO.Directory.CreateDirectory(Path.PhotosDirectory);

                img.SaveAsPng(Path.PhotosDirectory / $"{girl}-{i}.png");

#if ANDROID
        // Update the gallery
        Android.Media.MediaScannerConnection.ScanFile(Android.App.Application.Context, new string[] { Path.PhotosDirectory / $"{girl}-{i}.png" }, null, null);
#endif
                await Toast.Make($"{girl} saved to {Path.PhotosDirectory / $"{girl}-{i}.png"}", ToastDuration.Short, 14).Show();                         
            }

        }


        private void ConstructImageButtons()
        {
            //var folders = FileSystem.GetDirectories(girl);
            //expressions = new(folders.Length);

            //itemGrid = new Grid
            //{
            //    ColumnDefinitions =
            //    {
            //        new ColumnDefinition { Width = GridLength.Star },
            //        new ColumnDefinition { Width = GridLength.Star },
            //        new ColumnDefinition { Width = GridLength.Star }
            //    }
            //};
            if (_loadingImages)
                return;

            _loadingImages = true;

            var ofwg = expressions.Values.ToList();

            //for (int i = 0; i < ofwg.Count; i++)
            //{
            //    foreach (var btn in ofwg[i])
            //    {
            //        if (btn.Source is IDisposable disposable)
            //        {
            //            disposable.Dispose();
            //        }
            //        else
            //        {
            //            btn.Source = _sample;
            //        }
            //    }
            //}


            //itemGrid.Clear();

            var theWitch = ofwg[_selectedGroup];

            for (int i = 0; i < theWitch.Count; i++)
            {
                //int y = i / 3;
                //int x = i % 3;

                theWitch[i].TrueUri = Path.Cache / theWitch[i].Uri;
                //theWitch[i].ImageSource = new UriImageSource() { Uri = new(Path.Cache / theWitch[i].Uri), CachingEnabled = true, CacheValidity = TimeSpan.FromDays(1)};
                //itemGrid.Add(theWitch[i], x, y);
            }
            //imageCollection.ItemsSource = theWitch;

            //expressView = new(theWitch, imageCollection1, imageCollection2, imageCollection3);
            new ColListView<ImageItem>(theWitch, buttonListView, 3);
            //buttonListView.RowHeight = (int)(buttonListView.Width / 3);

            //List<ImageItem> pmo = new List<ImageItem> { theWitch[0] };

            //buttonListView.ItemsSource = pmo;



            _loadingImages = false;
        }

        private void SortExpressions()
        {
            var folderIndexMap = _girlDefaults.Folders.ToDictionary(folder => folder.Name, folder => folder.ZIndex);

            _selectedExpressions = _selectedExpressions.OrderBy(item => folderIndexMap.TryGetValue(item.Category, out int zIndex) ? zIndex : int.MaxValue).ToList();
        }

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

        private void UpdateWarnings(ImageItem? item)
        {
            string[] groupIds;

            if (item == null)
            {
                Dictionary<string[], int> occurences = new();
                for (int i = 0; i < _selectedExpressions.Count; i++)
                {
                    string[] group = _girlDefaults.Groups?.FirstOrDefault(ids => ids.Any(grp => _selectedExpressions[i].Uri.ToString().Contains(grp)));

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
                groupIds = _girlDefaults.Groups?.FirstOrDefault(ids => ids.Any(grp => item.Uri.ToString().Contains(grp)));


            for (int i = 0; i < expressions.Count; i++)
            {
                var expGroup = expressions.Values.ToArray()[i];
                foreach (var image in expGroup)
                {
                    if (image.BackgroundColor == Colors.Green)
                        continue;

                    if (groupIds == null)
                    {
                        image.BackgroundColor = Colors.Transparent;
                        continue;
                    }


                    if (!groupIds.Any(group => image.Uri.ToString().Contains(group)))
                    {
                        image.BackgroundColor = Colors.Yellow;
                    }
                    else if (image.BackgroundColor != Colors.Green)
                        image.BackgroundColor = Colors.Transparent;
                }
            }

        }

        private void ToggleOfType(ImageItem item)
        {

            var folder = _girlDefaults.Folders.FirstOrDefault(folder => folder.Name == item.Category);

            if (folder.Max == -1) return;


            int max = !folder.Bypass.Any(bypass => item.Uri.ToString().Contains(bypass)) ? folder.Max : 1;


            while (CountCon(_selectedExpressions, image => image.Category == item.Category) >= max)
            {
                var expression = _selectedExpressions.FirstOrDefault(image => image.Category == item.Category);
                expression.BackgroundColor = Colors.Transparent;
                _selectedExpressions.Remove(expression);
            }
        }

        private async void OnExpressionClicked(object sender, EventArgs e)
        {
            ImageItem item;
            if (sender.GetType() == typeof(DokiExpression))
            {
                var fakeItem = (DokiExpression)sender;

                item = expressions.SelectMany(kv => kv.Value).FirstOrDefault(imageItem => imageItem.Equals(fakeItem));

            }
            else
                item = (ImageItem)sender;

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
            //imageCollection.ItemsSource = expressions.Values.ToList()[_selectedGroup];
            SortExpressions();
            await Task.Run(() => ConstructDoki(false));
        }

        private async void OnSavedClicked(object sender, EventArgs e)
        {
            await Task.Run(() => ConstructDoki(true));
        }

        private void OnTabClicked(object? sender, TappedEventArgs e)
        {
            Label tabLbl = (Label)sender;

            foreach (Label tab in tabGrid.Children)
            {
                tab.BackgroundColor = Colors.Transparent;
            }

            tabLbl.BackgroundColor = (Microsoft.Maui.Graphics.Color)Application.Current.Resources["Primary"];
            _selectedGroup = tabGrid.Children.IndexOf(tabLbl);
            ConstructImageButtons();
        }

        private byte _lastAspect = 3;
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);


            double aspectRatio = width / height;
            byte asp = (byte)(aspectRatio > 1 ? 1 : 0);

            if (_lastAspect == asp)
                return;

            _lastAspect = asp;

            if (asp == 1) //horizontal
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
            else //vertical
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
