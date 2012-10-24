using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Numerics;

namespace MandelbrotSL
{
    public class PlotCanvas : System.Collections.Generic.IEnumerable<int>
    {
        int[,] canvas;
        public int Width { get; set; }
        public int Height { get; set; }
        public Complex TopLeft { get; set; }
        public Complex BottomRight { get; set; }
        double xStep;
        double xOrigin;
        double yStep;
        double yOrigin;

        public PlotCanvas(int width, int height, Complex topLeft, Complex bottomRight)
        {
            // Set size
            this.Width = width;
            this.Height = height;
            canvas = new int[Width, Height];

            // set the points for determine the rectangle
            this.TopLeft = topLeft;
            this.BottomRight = bottomRight;

            // Set scale
            xStep = (bottomRight.Real - topLeft.Real) / Width;
            xOrigin = topLeft.Real;
            yStep = (bottomRight.Imaginary - topLeft.Imaginary) / Height;
            yOrigin = topLeft.Imaginary;
        }

        public int this[int x, int y]
        {
            get { return canvas[x, y]; }
            set { canvas[x, y] = value; }
        }

        public Complex GetPoint(int x, int y)
        {
            return new Complex(x * xStep + xOrigin, y * yStep + yOrigin);
        }

        public void Clear()
        {
            for (int x = 0; x < canvas.GetLength(0); ++x)
                for (int y = 0; y < canvas.GetLength(1); ++y)
                    canvas[x, y] = -1;
        }

        public System.Collections.Generic.IEnumerator<int> GetEnumerator()
        {
            foreach (var i in canvas)
                yield return i;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return canvas.GetEnumerator();
        }
    }
}
