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
using System.Collections.Generic;
using System.Linq;

namespace MandelbrotSL
{
    public class MandelbrotSet
    {
        public static int Calculate(Complex c, int limit, double threshold)
        {
            var z = Complex.Zero;
            foreach(var i in Enumerable.Range(0, limit))
            {
                if (z.Magnitude > threshold)
                    return i;   // c is not an element of mandelbrot set. return the number of steps to start diverging.
                z = z * z + c;
            }
            return -1;  // c is an element of mandelbrot set.
        }
    }
}
