using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Evolver
{
    public class R
    {
        private static Random random = new Random();

        public static byte Byte()
        {
            return (byte)random.Next(256);
        }

        public static int Int(int max)
        {
            return random.Next(max);
        }

        public static int Int(double max)
        {
            return Int((int)max);
        }

        public static double Random()
        {
            return random.NextDouble();
        }

        public static T Choice<T>(IEnumerable<T> list)
        {
            return list.ElementAt(Int(list.Count()));
        }

        public static Point Point(int width, int height)
        {
            return new Point(Int(width), Int(height));
        }
    }

    public abstract class Primitive
    {
        public Color color;
        public Brush brush;

        public Color _color;
        public Brush _brush;

        public bool hasSaveState = false;

        public abstract void Mutate(int cWidth, int cHeight);
        public abstract void Paint(DrawingContext dc);

        public abstract void SaveState();
        public abstract void RestoreState();

        public abstract Primitive Clone();

        public void UpdateBrush()
        {
            brush = new SolidColorBrush(color);
        }
    }

    public class Rectangle : Primitive
    {
        Point ul, lr;
        Point _ul, _lr;

        public double Width
        {
            get
            {
                if (ul != null && lr != null)
                {
                    return Math.Abs(lr.X - ul.X);
                }
                return 0.0;
            }
        }

        public double Height
        {
            get
            {
                if (ul != null && lr != null)
                {
                    return Math.Abs(lr.Y - ul.Y);
                }
                return 0.0;
            }
        }

        public Rectangle(Point ul, Point lr)
        {
            this.ul = ul;
            this.lr = lr;

            // init with random color
            color = Color.FromArgb(R.Byte(), R.Byte(), R.Byte(), R.Byte());
            brush = new SolidColorBrush(color);
        }

        // static random constructor
        public static Rectangle CreateRandom(int width, int height)
        {
            int x1 = R.Int(width);
            int x2 = R.Int(width);
            int y1 = R.Int(height);
            int y2 = R.Int(height);

            return new Rectangle(new Point(Math.Min(x1, x2), Math.Min(y1, y2)),
                new Point(Math.Max(x1, x2), Math.Max(y1, y2)));
        }

        public override void Paint(DrawingContext dc) {
            dc.DrawRectangle(brush, null, new System.Windows.Rect(ul.X, ul.Y, Width, Height));
        }

        public override Primitive Clone()
        {
            var copy = new Rectangle(this.ul, this.lr);
            copy.color = this.color;
            copy.brush = this.brush.Clone();
            return copy;
        }

        public override void SaveState()
        {
            _ul = ul;
            _lr = lr;
            _color = color;
            hasSaveState = true;
        }

        public override void RestoreState()
        {
            if (!hasSaveState) return;
            ul = _ul;
            lr = _lr;
            color = _color;
            brush = new SolidColorBrush(color);

            hasSaveState = false;
        }

        public override void Mutate(int canvasWidth, int canvasHeight)
        {
            // store values
            SaveState();

            if (R.Int(10) == 0)
            {
                ul.X = R.Int(canvasWidth);
                ul.Y = R.Int(canvasHeight);
            }

            if (R.Int(10) == 0)
            {
                lr.X = R.Int(canvasWidth);
                lr.Y = R.Int(canvasHeight);
            }
            
            if (R.Int(10) == 0)
                    color.R = R.Byte();
            if (R.Int(10) == 0)
                    color.G = R.Byte();
            if (R.Int(10) == 0)
                    color.B = R.Byte();
            if (R.Int(10) == 0)
                    color.A = R.Byte();
            
            brush = new SolidColorBrush(color);
        }
    }

    public class Polygon : Primitive
    {
        PointCollection _points;
        PointCollection points;

        private PathGeometry polygonGeometry;

        public Polygon(PointCollection points)
        {
            this.points = points;

            // init with random color
            color = Color.FromArgb(R.Byte(), R.Byte(), R.Byte(), R.Byte());
            brush = new SolidColorBrush(color);

            ComputeGeometry();
        }

        public static Polygon CreateRandom(int numPoints, int cWidth, int cHeight)
        {
            var points = new PointCollection();
            for ( int i = 0; i < numPoints; i++) {
                points.Add(R.Point(cWidth, cHeight));
            }
            return new Polygon(points);
        }

        private void ComputeGeometry() {
            polygonGeometry = new PathGeometry();
            var fig = new PathFigure();

            fig.StartPoint = points[0];
            fig.Segments.Add(new PolyLineSegment(points.Skip(1).ToArray(), true));
            fig.IsClosed = true;

            polygonGeometry.Figures.Add(fig);
            polygonGeometry.FillRule = FillRule.Nonzero;

        }

        public override void Paint(DrawingContext dc)
        {
            dc.DrawGeometry(brush, null, polygonGeometry);
        }

        public override void Mutate(int cWidth, int cHeight)
        {
            SaveState();

            if (R.Random() < .1) { 
                int i = R.Int(points.Count());
                points[i] = R.Point(R.Int(cWidth), R.Int(cHeight));
            }

            if (R.Random() < .1)
            {
                color.R = R.Byte();
            }

            if (R.Random() < .1)
            {
                color.G = R.Byte();
            }

            if (R.Random() < .1)
            {
                color.B = R.Byte();
            }

            UpdateBrush();
            // keep geometry updated
            ComputeGeometry();
        }

        public override void RestoreState()
        {
            brush = _brush.Clone();
            color = _color;
            //Console.Write("restoring " + points);
            points = _points.Clone();
            //Console.WriteLine(" to " + points);
            ComputeGeometry();
            hasSaveState = false;
        }

        public override void SaveState()
        {
            _points = points.Clone();
            _color = color;
            _brush = brush.Clone();
            hasSaveState = true;
        }

        public override Primitive Clone()
        {
            Polygon copy = new Polygon(this.points.Clone());
            copy.color = this.color;
            copy.brush = this.brush.Clone();

            return copy;
        }
    }

}
