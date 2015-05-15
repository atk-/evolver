using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Evolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        EvolverLogic evolver;

        long bestFitness = long.MaxValue;

        int canvasWidth, canvasHeight;

        System.Timers.Timer timer = new System.Timers.Timer(200);

        public MainWindow()
        {
            InitializeComponent();

            string modelImageFile = @"c:\Users\ATK\Dropbox\python\evpr\blake.png";
            SetModel(modelImageFile);

            canvasWidth = (int)Canvas.Width;
            canvasHeight = (int)Canvas.Height;

            timer.Elapsed += OnTimerElapsed;

            Task.Run(() => {
                evolver = new EvolverLogic(modelImageFile, canvasWidth, canvasHeight);
                evolver.MainLoop();
            });
        }

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() => { Canvas.Source = evolver.GetBitmap(); });
        }

        public void SetModel(string imagePath)
        {
            Model.Source = new BitmapImage(new Uri(imagePath));
        }

        public void Toggle(object sender, RoutedEventArgs e)
        {
            //evolver.Iterate();

            evolver.Toggle();
            timer.Enabled = !timer.Enabled;
            //Iterate100(sender, e);
            //Canvas.Source = evolver.CanvasBitmap;
        }

        public void Iterate100(object sender, RoutedEventArgs e)
        {
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                evolver.Iterate();
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds + "  " + bestFitness);
        }


    }
}
