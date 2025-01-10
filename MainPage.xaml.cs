using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Image = Microsoft.Maui.Controls.Image;

namespace losertron4000
{
    public partial class MainPage : ContentPage
    {
        public static string girl = "natsuki";

        public Dictionary<string, List<ImageItem>> expressions = new();


        public MainPage()
        {
            InitializeComponent();
            FileSystem.Init();
            Bitmap nats = new("natsuki\\head\\natsuki_face_forward.png");
            dokiPreview.Source = nats.CropImage(false);
            ConstructImageButtons();
            //if (FileSystem.FileExists("natsuki\\head\\natsuki_face_forward.png"))
            //{

            //}
            //else
            //    Debug.WriteLine("could not load face");

            //var bruh = FileSystem.GetDirectories("natsuki");
            //gogo.Text = bruh.Length + " files found";
            //for(int i = 0; i < bruh.Length; i++)
            //{
            //    Debug.WriteLine(bruh[i]);
            //}

            //testimg1.Source = new Bitmap("natsuki\\head\\natsuki_face_forward.png").CropImage();
            //testimg2.Source = new Bitmap("natsuki\\head\\natsuki_face_forward.png").CropImage();

            //Debug.WriteLine(string.Join(", ", FileSystem.GetFiles("natsuki\\head\\").Select(f => f.ToString())));
        }

        private void ConstructImageButtons()
        {
            var folders = FileSystem.GetDirectories(girl);
            expressions = new(folders.Length);

            /*itemGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };*/


            itemGrid.Clear();
            int bruh = 0;
            for (int i = 0; i < folders.Length; i++)
            {
                
                var imgPaths = FileSystem.GetFiles(folders[i]);                
                List<ImageItem> list = new();
                for (int j = 0; j < imgPaths.Length; j++)
                {
                    list.Add(new(imgPaths[j]));
                    list[j].ImagePreview = new Bitmap(list[j].Uri).CropImage();
                    list[j].Source = list[j].ImagePreview;
                    list[j].Aspect = Aspect.AspectFit;

                    list[j].SizeChanged += (s, e) =>
                    {
                        Image img = (Image)s;
                        if (img.Width > 0)
                        {
                            img.HeightRequest = img.Width;  // Ensure the image stays square
                        }
                    };

                    int y = bruh / 3;
                    int x = bruh % 3;
                    bruh++;
                    itemGrid.Add(list[j], x, y);
                    //list[j].HorizontalOptions = LayoutOptions.FillAndExpand;
                }

                expressions.Add(folders[i].FileName, list);
                if (i == 5)
                    break;
            }
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {

            /* count++;

             if (count == 1)
                 CounterBtn.Text = $"Clicked {count} time";
             else
                 CounterBtn.Text = $"Clicked {count} times";

             SemanticScreenReader.Announce(CounterBtn.Text);*/
        }
    }

}
