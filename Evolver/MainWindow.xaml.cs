﻿using System;
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
using System.Windows.Threading;

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

            Canvas.SourceUpdated += Canvas_SourceUpdated;

            Task.Run(() => {
                evolver = new EvolverLogic(modelImageFile, canvasWidth, canvasHeight);
                evolver.MainLoop();
            });
        }

        private void Canvas_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            Console.WriteLine("canvas source updated");
        }

        private static Action emptyDelegate = delegate { };

        private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Canvas.Dispatcher.Invoke(DispatcherPriority.Render, emptyDelegate);
            evolver.InvokePropertyChanged("CanvasBitmap");
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
