using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Evolver
{
    public class Drawing
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        List<Primitive> shapes;

        private Drawing()
        {
        }

        public Drawing(int width, int height)
        {
            shapes = new List<Primitive>();

            Width = width;
            Height = height;
        }

        public void Init()
        {
            for (int i = 0; i < 10; i++)
            {
                if (i < 0)
                    shapes.Add(Rectangle.CreateRandom(Width, Height));
                else
                    shapes.Add(Polygon.CreateRandom(3, Width, Height));
            }
        }

        public void MutateAll()
        {
            //R.Choice(shapes).Mutate(canvasWidth, canvasHeight);

            //int i = R.Int(shapes.Count());
            //int j = R.Int(shapes.Count());

            foreach (Primitive p in shapes)
            {
                p.Mutate(Width, Height);
            }
        }

        public RenderTargetBitmap PaintBitmap()
        {
            return Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                var dv = new DrawingVisual();
                var dc = dv.RenderOpen();

                foreach (Primitive p in shapes)
                {
                    p.Paint(dc);
                }
                dc.Close();

                RenderTargetBitmap bmp = new RenderTargetBitmap(Width, Height, 100, 100, PixelFormats.Pbgra32);
                bmp.Render(dv);
                //CanvasBitmap = bmp;
                return bmp;
            });
        }

        public Drawing Clone()
        {
            Drawing copy = new Drawing(Width, Height);
            copy.shapes = this.shapes.Select(x => x.Clone()).ToList();
            return copy;
        }
    }
}
