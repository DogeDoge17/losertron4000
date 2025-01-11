using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace losertron4000
{
    public class ImageItem : ImageButton
    {
        public Bitmap ImagePreview { get { return _imagePreview; } set { _imagePreview = value; Source = _imagePreview; } }
        private Bitmap _imagePreview { get; set; }


        public Path Uri { get; private set; }

        public string Category { get; set; }

        public bool Selected { get; set; }

        public ImageItem(Path imageUri) => Uri = imageUri;
        public ImageItem() { }

        public void SetUri(Path imageUri) => Uri = imageUri;
    }
}
