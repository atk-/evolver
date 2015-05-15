using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Evolver
{
    public class EvolverLogic
    {
        List<Primitive> shapes = new List<Primitive>();
        long bestFitness;
        int canvasWidth, canvasHeight;

        byte[] modelData;
        byte[] canvasData;

        bool running = false;

        private System.Object canvasLock = new System.Object();

        private RenderTargetBitmap CanvasBitmap;

        public EvolverLogic(string modelImageFile, int canvasWidth, int canvasHeight)
        {
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;

            InitModelImage(modelImageFile);

            for (int i = 0; i < 10; i++)
            {
                if (i < 0)
                    shapes.Add(Rectangle.CreateRandom(canvasWidth, canvasHeight));
                else
                    shapes.Add(Polygon.CreateRandom(3, canvasWidth, canvasHeight));
            }

            bestFitness = long.MaxValue;
        }

        public BitmapSource GetBitmap()
        {            
            //CanvasBitmap.Freeze();
            return CanvasBitmap;
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

        public void Toggle()
        {
            running = !running;
        }

        public void MainLoop()
        {
            int loops = 0;
            while (true)
            {
                if (running)
                {
                    loops += 1;
                    this.Iterate();
                    if (loops % 100 == 0)
                    {
                        Console.WriteLine(loops);
                    }

                }
                Thread.Sleep(11);
            }
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
            Console.WriteLine(fitness + " < " + bestFitness + "? " + (fitness < bestFitness));

            if (fitness <= bestFitness)
            {
                bestFitness = fitness;
            }
            else
            {
                // revert this round of mutations and roll back the changes
                foreach (Primitive p in shapes)
                {
                    if (p.hasSaveState)
                    {
                        p.RestoreState();
                    }
                }
                PaintBitmap(); // rather store and recall the old one
            }
        }

        public void PaintBitmap()
        {
            lock (canvasLock)
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
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
                    CanvasBitmap.Freeze();
                    Console.WriteLine("PaintBitmap");
                });
            }
        }

        private void MutateAll()
        {
            //R.Choice(shapes).Mutate(canvasWidth, canvasHeight);

            //int i = R.Int(shapes.Count());
            //int j = R.Int(shapes.Count());

            foreach (Primitive p in shapes)
            {
                p.Mutate(canvasWidth, canvasHeight);
            }
        }
    }
}
