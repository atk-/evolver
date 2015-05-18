using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Evolver
{
    public class EvolverLogic : INotifyPropertyChanged
    {
        //List<Primitive> shapes = new List<Primitive>();

        Drawing previousDrawing;
        Drawing drawing;

        long bestFitness;
        int canvasWidth, canvasHeight;

        byte[] modelData;
        byte[] canvasData;

        bool running = false;

        private System.Object canvasLock = new System.Object();

        private RenderTargetBitmap _canvasBitmap;
        public RenderTargetBitmap CanvasBitmap
        {
            get { return _canvasBitmap; }
            set { 
                _canvasBitmap = value;
                InvokePropertyChanged("CanvasBitmap");
            }
        }

        int numGenerations = 0;

        public EvolverLogic(string modelImageFile, int canvasWidth, int canvasHeight)
        {
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;

            CanvasBitmap = new RenderTargetBitmap(canvasWidth, canvasHeight, 100, 100, PixelFormats.Pbgra32);

            InitModelImage(modelImageFile);

            drawing = new Drawing(canvasWidth, canvasHeight);
            drawing.Init();

            bestFitness = long.MaxValue;
        }

        public BitmapSource GetBitmap()
        {            
            //CanvasBitmap.Freeze();
            return Dispatcher.CurrentDispatcher.Invoke(() =>
                CanvasBitmap);
            
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

        public void SaveCanvasBitmap()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                using (Stream s = new FileStream("canvas.png", FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    var bmp = BitmapFrame.Create(drawing.PaintBitmap());
                    encoder.Save(s);
                }
            });
        }

        public void Toggle()
        {
            running = !running;
            if (!running)
            {
                SaveCanvasBitmap();
            }
        }

        public void MainLoop()
        {
            while (true)
            {
                if (running)
                {
                    this.Iterate();
                    if (numGenerations % 100 == 0)
                    {
                        Console.WriteLine(numGenerations);
                    }

                }
                Thread.Sleep(11);
            }
        }

        public void Iterate()
        {
            // do one iteration: mutate, repaint, compute fitness, apply/discard changes
            numGenerations++;
            previousDrawing = drawing.Clone();
            
            drawing.MutateAll();
            CanvasBitmap = drawing.PaintBitmap();

            // bitmap to bytes for computation
            BitmapSource canvas = CanvasBitmap as BitmapSource;
            int canvasStride = canvas.PixelWidth * (canvas.Format.BitsPerPixel / 8);
            canvasData = new byte[canvasStride * canvas.PixelHeight];
            canvas.CopyPixels(canvasData, canvasStride, 0);

            long fitness = EvolverLogic.ComputeFitness(canvasData, modelData);
            Console.WriteLine("#{0}: {1} < {2}? {3}", numGenerations, fitness, bestFitness, (fitness < bestFitness));

            if (fitness <= bestFitness)
            {
                bestFitness = fitness;
            }
            else
            {
                // revert this round of mutations and roll back the changes
                //foreach (Primitive p in shapes)
                //{
                //    if (p.hasSaveState)
                //    {
                //        p.RestoreState();
                //    }
                //}
                drawing = previousDrawing.Clone();
                CanvasBitmap = drawing.PaintBitmap(); // rather store and recall the old one
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
