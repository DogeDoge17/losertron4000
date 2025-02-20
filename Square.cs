using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace losertron4000
{
    class Square : ContentView
    {
        public AspectClamp Clamp { get; set; }

        protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
        {
            double size = Clamp == AspectClamp.Width ? widthConstraint : heightConstraint;
            return new Size(size, size);
        }
        protected override void OnSizeAllocated(double width, double height)
        {
            double size = Clamp == AspectClamp.Width ? width : height;

            if (Clamp == AspectClamp.Width)
                HeightRequest = size;
            else
                WidthRequest = size;


            base.OnSizeAllocated(size, size);
        }
    }
    enum AspectClamp
    {
        Height,
        Width
    }
}
