using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;

namespace losertron4000
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            FileSystem.Init();

            if (FileSystem.FileExists("natsuki\\head\\natsuki_face_forward.png"))
            {
                Bitmap nats = new("natsuki\\head\\natsuki_face_forward.png");
                           
                dokiPreview.Source = nats.CropImage();
                /*var stream = FileSystem.OpenFile("natsuki\\head\\natsuki_face_forward.png");
                //stream.Position = 0;
                ImageSource source = ImageSource.FromStream(() => stream);
                dokiPreview.Source = source;*/

                //dokiPreview.Source = ImageSource.FromStream(() => FileSystem.OpenFile("natsuki\\head\\natsuki_face_forward.png"));
            }

            var bruh = FileSystem.GetFiles("natsuki", "*.png", true);
            gogo.Text = bruh.Length + " files found";
            for(int i = 0; i < bruh.Length; i++)
            {
                Debug.WriteLine(bruh[i]);
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
