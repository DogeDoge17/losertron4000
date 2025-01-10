using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace losertron4000
{
    public class ImageItem : Image
    {
        public Path Uri { get; private set; }
        public Bitmap ImagePreview { get; set; }

        public string Category { get; set; }

        public bool Selected { get; set; }

        public ImageItem(Path imageUri) => Uri = imageUri;
        public ImageItem() { }

        public void SetUri(Path imageUri) => Uri = imageUri;
    }
}
