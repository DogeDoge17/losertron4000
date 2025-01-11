using Microsoft.Maui.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Text.Json;

namespace losertron4000
{
    public partial class MainPage : ContentPage
    {
        public static string girl = "natsuki";

        public Dictionary<string, List<ImageItem>> expressions = new();
        public GirlsGirling _girlDefaults = new();
        private List<ImageItem> _selectedExpressions = new();

        private int _selectedGroup = 0;


        private ImageSource _sample = ImageSource.FromResource("sample.png");

        public MainPage()
        {
            InitializeComponent();
            SetLayout(DeviceDisplay.MainDisplayInfo.Width, DeviceDisplay.MainDisplayInfo.Height);
            FileSystem.Init();
            Debug.WriteLine(Path.Cache);
            //dokiPreview.Source = new Bitmap("natsuki\\head\\natsuki_face_forward.png").CropImage(false);//Bitmap.FileToSource("natsuki\\head\\natsuki_face_forward.png");//Bitmap.FullProcessSource("natsuki\\head\\natsuki_face_forward.png", Bitmap.CropImage);//nats.CropImage(false);
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
                        //Bitmap bitmap = new Bitmap(list[j].Uri).CropImage(false);
                        Image<Rgba32> bitmap = Bitmap.FullProcessImage(list[j].Uri, Bitmap.CropImage, Bitmap.PreviewSize);
                        Cache.Save(bitmap, list[j].Uri);
                        //list[j].Source = ImageSource.FromFile(Path.Cache / list[j].Uri);
                    }
                    //else
                    // list[j].Source = img;

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
           //List<ImageButton> megaList = 

        }

        private void ConstructDoki(bool export = false)
        {
            using Image<Rgba32> img = new Image<Rgba32>(960, 960);

            for (int i = 0; i < _selectedExpressions.Count; i++)
            {
                img.Mutate(ctx => ctx.DrawImage(Bitmap.FileToImage(_selectedExpressions[i].Uri), 1));
            }

            dokiPreview.Source = Bitmap.ImageToSource(img);
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


            ConstructDoki();
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

            if (aspectRatio > 1)
            {
                centralGrid.ColumnDefinitions = new()
                {
                     new ColumnDefinition { Width = GridLength.Star },
                     new ColumnDefinition { Width = GridLength.Star },
                };

                centralGrid.RowDefinitions = new();

                centralGrid.SetColumn(dokiPreview, 1);
                centralGrid.SetColumn(bottomBit, 0);

                centralGrid.SetRow(dokiPreview, 0);
                centralGrid.SetRow(bottomBit, 0);
            }
            else
            {
                centralGrid.RowDefinitions = new()
                {
                     new RowDefinition { Height= GridLength.Star },
                     new RowDefinition { Height = GridLength.Star },
                };

                centralGrid.ColumnDefinitions = new();

                centralGrid.SetRow(dokiPreview, 0);
                centralGrid.SetRow(bottomBit, 1);

                centralGrid.SetColumn(dokiPreview, 0);
                centralGrid.SetColumn(bottomBit, 0);
            }
        }
    }

}
