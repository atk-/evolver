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

        public MainWindow()
        {
            InitializeComponent();

            string modelImageFile = @"c:\Users\ATK\Dropbox\python\evpr\blake.png";
            SetModel(modelImageFile);

            canvasWidth = (int)Canvas.Width;
            canvasHeight = (int)Canvas.Height;
            
            evolver = new EvolverLogic(modelImageFile, canvasWidth, canvasHeight);
        }

        public void SetModel(string imagePath)
        {
            Model.Source = new BitmapImage(new Uri(imagePath));
        }

        public void Toggle(object sender, RoutedEventArgs e)
        {
            //evolver.Iterate();
            Iterate100(sender, e);
            Canvas.Source = evolver.CanvasBitmap;
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
