using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Evolver
{
    class EvolverLogic
    {
        List<Primitive> shapes = new List<Primitive>();
        long bestFitness;
        int canvasWidth, canvasHeight;

        byte[] modelData;
        byte[] canvasData;

        public RenderTargetBitmap CanvasBitmap { get; private set; }

        public EvolverLogic(string modelImageFile, int canvasWidth, int canvasHeight)
        {
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;

            InitModelImage(modelImageFile);

            for (int i = 0; i < 50; i++)
            {
                if (i < 0)
                    shapes.Add(Rectangle.CreateRandom(canvasWidth, canvasHeight));
                else
                    shapes.Add(Polygon.CreateRandom(3, canvasWidth, canvasHeight));
            }
        }

        private void InitModelImage(string imageFile)
        {
            BitmapSource model = new BitmapImage(new Uri(imageFile)) as BitmapSource;
            int modelStride = model.PixelWidth * (model.Format.BitsPerPixel / 8);
            modelData = new byte[modelStride * model.PixelHeight];
            model.CopyPixels(modelData, modelStride, 0);
        }


        public static long ComputeFitness(byte[] array1, byte[] array2)
        {
            // BitmapSource canvas = Canvas.Source.GetAsFrozen() as BitmapSource;

            //int canvasStride = canvas.PixelWidth * (canvas.Format.BitsPerPixel / 8);
            //canvasData = new byte[canvasStride * canvas.PixelHeight];
            //canvas.CopyPixels(canvasData, canvasStride, 0);

            long diff = 0;
            for (int i = 0; i < array1.Length; i++)
            {
                diff += Math.Abs(array1[i] - array2[i]);
            }

            return diff;
        }

        public void Iterate()
        {
            // do one iteration: mutate, repaint, compute fitness, apply/discard changes
            MutateAll();
            PaintBitmap();

            // bitmap to bytes for computation
            BitmapSource canvas = CanvasBitmap as BitmapSource;
            int canvasStride = canvas.PixelWidth * (canvas.Format.BitsPerPixel / 8);
            canvasData = new byte[canvasStride * canvas.PixelHeight];
            canvas.CopyPixels(canvasData, canvasStride, 0);

            long fitness = EvolverLogic.ComputeFitness(canvasData, modelData);
            Console.WriteLine(fitness + " < " + bestFitness + "?");

            if (fitness <= bestFitness)
            {
                bestFitness = fitness;
            }
            else
            {
                foreach (Primitive p in shapes)
                {
                    if (p.hasSaveState)
                    {
                        p.RestoreState();
                    }
                }
            }
        }

        public void PaintBitmap()
        {
            var dv = new DrawingVisual();
            var dc = dv.RenderOpen();

            foreach (Primitive p in shapes)
            {
                p.Paint(dc);
            }
            dc.Close();

            CanvasBitmap = new RenderTargetBitmap(canvasWidth, canvasHeight, 100, 100, PixelFormats.Pbgra32);

            CanvasBitmap.Render(dv);
        }

        private void MutateAll()
        {
            foreach (Primitive p in shapes)
            {
                if (R.Int(100) == 1)
                {
                    p.Mutate(canvasWidth, canvasHeight);
                }
            }
        }
    }
}
