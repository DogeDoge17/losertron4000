using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;
using Microsoft.Maui.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;
using System.Threading;

namespace losertron4000
{
    public partial class MainPage : ContentPage
    {
        public static string girl = "natsuki";

        public Dictionary<string, List<ImageItem>> expressions;
        public GirlsGirling _girlDefaults;
        private List<ImageItem> _selectedExpressions;

        private int _selectedGroup = 0;


        private ImageSource _sample = ImageSource.FromResource("sample.png");

        private bool _loadingImages;

        public MainPage()
        {
            InitializeComponent();
            SetLayout(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);
            FileSystem.Init();
            Debug.WriteLine(Path.Cache);
            //dokiPreview.Source = new Bitmap("natsuki\\head\\natsuki_face_forward.png").CropImage(false);//Bitmap.FileToSource("natsuki\\head\\natsuki_face_forward.png");//Bitmap.FullProcessSource("natsuki\\head\\natsuki_face_forward.png", Bitmap.CropImage);//nats.CropImage(false);
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
            itemGrid.Clear();
            tabGrid.Clear();

            _girlDefaults = JsonSerializer.Deserialize<GirlsGirling>(FileSystem.ReadFile(new Path(girl) / "defaults.json"));
            

            for (int i = 0; i < folders.Length; i++)
            {
                var imgPaths = FileSystem.GetFiles(folders[i]);
                List<ImageItem> list = new();
                for (int j = 0; j < imgPaths.Length; j++)
                {
                    list.Add(new(imgPaths[j]));

                    if (!Cache.TryLoadSource(list[j].Uri, out var img))
                    {
                        Image<Rgba32> bitmap = Bitmap.FullProcessImage(list[j].Uri, Bitmap.CropImage, Bitmap.PreviewSize);
                        Cache.Save(bitmap, list[j].Uri);                     
                    }

                    list[j].Category = imgPaths[j].DirectoryPath.FileName;
                    list[j].Aspect = Aspect.AspectFit;

                    list[j].Clicked += OnExpressionClicked;
                    list[j].SizeChanged += (s, e) =>
                    {
                        ImageButton img = (ImageButton)s;
                        if (img.Width > 0)                            
                        {
                            img.HeightRequest = img.Width;  // Ensure the image stays square
                        }
                    };
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

        private void ChooseDefaults()
        {
            _selectedExpressions = new();
            List<ImageButton> megaList = new List<ImageButton>();
            for (int i = 0; i < expressions.Count; i++)
                megaList.AddRange(expressions.Values.ToList()[i]);
            

        }

        private void ConstructDoki(bool export = false)
        {
            using Image<Rgba32> img = new Image<Rgba32>(960, 960);
            for (int i = 0; i < _selectedExpressions.Count; i++)
            {
                //ik ik ik its hardcoded but igdaf atp
                SixLabors.ImageSharp.Point drawOffset = _selectedExpressions[i].Uri.ToString().Contains("crossed") && girl == "natsuki" ? new SixLabors.ImageSharp.Point(-18, -22) : new SixLabors.ImageSharp.Point(0, 0);

               
                img.Mutate(ctx => ctx.DrawImage(Bitmap.FileToImage(_selectedExpressions[i].Uri), drawOffset, 1));
            }

            Bitmap.CropImage(img);

            if(!export)
                dokiPreview.Source = Bitmap.ImageToSource(img);
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

                Toast.Make($"{girl} saved to {Path.PhotosDirectory / $"{girl}-{i}.png"}", ToastDuration.Short, 14).Show(); // Text size set to 12                
            }

        }


        private void ConstructImageButtons()
        {
            //var folders = FileSystem.GetDirectories(girl);
            //expressions = new(folders.Length);

            ///*itemGrid = new Grid
            //{
            //    ColumnDefinitions =
            //    {
            //        new ColumnDefinition { Width = GridLength.Star },
            //        new ColumnDefinition { Width = GridLength.Star },
            //        new ColumnDefinition { Width = GridLength.Star }
            //    }
            //};*/
            if (_loadingImages)
                return;

            _loadingImages = true;

            var ofwg = expressions.Values.ToList();

            for (int i = 0; i < ofwg.Count; i++)
            {
                foreach (var btn in ofwg[i])
                {
                    if (btn.Source is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    else
                    {
                        btn.Source = _sample;
                    }
                }
            }


            itemGrid.Clear();

            var theWitch = ofwg[_selectedGroup];

            for (int i = 0; i < theWitch.Count; i++)
            {
                int y = i / 3;
                int x = i % 3;

                theWitch[i].Source = ImageSource.FromFile(Path.Cache / theWitch[i].Uri);                

                itemGrid.Add(theWitch[i], x, y);
            }
            _loadingImages = false;
        }


        private void SortExpressions()
        {
            var folderIndexMap = _girlDefaults.Folders.ToDictionary(folder => folder.Name, folder => folder.ZIndex);

            // Sort items by category based on the ZIndex found in folderIndexMap
            _selectedExpressions = _selectedExpressions.OrderBy(item => folderIndexMap.TryGetValue(item.Category, out int zIndex) ? zIndex : int.MaxValue).ToList();
        }

        private void OnExpressionClicked(object sender, EventArgs e)
        {
            ImageItem item = (ImageItem)sender;

            if (_selectedExpressions.IndexOf(item) == -1)
            {
                //if (item.Category != "extra")
                //    ToggleOfType(item);
                _selectedExpressions.Add(item);
                item.BackgroundColor = Colors.Green;
            }
            else
            {
                item.BackgroundColor = Colors.Transparent;
                _selectedExpressions.Remove(item);
            }
            SortExpressions();
            ConstructDoki(false);
        }

        private void OnSavedClicked(object sender, EventArgs e)
        {
            ConstructDoki(true);
        }

        private void OnTabClicked(object? sender, TappedEventArgs e)
        {
            Label lbl = (Label)sender;

            foreach (Label tab in tabGrid.Children)
            {
                tab.BackgroundColor = Colors.Transparent;
            }

            lbl.BackgroundColor = (Microsoft.Maui.Graphics.Color)Application.Current.Resources["Primary"];
            _selectedGroup = tabGrid.Children.IndexOf(lbl);
            ConstructImageButtons();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            SetLayout(width, height);
        }

        private void SetLayout(double width, double height)
        {
            double aspectRatio = width / height;

            if (aspectRatio > 1) //horizontal
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
