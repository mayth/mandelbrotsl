using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Numerics;

namespace MandelbrotSL
{
    public partial class MainPage : UserControl
    {
        static readonly int limit = 20;
        static readonly Complex defaultTopLeft = new Complex(-2.0, 1.5);
        static readonly Complex defaultBottomRight = new Complex(1.0, -1.5);

        public MainPage()
        {
            InitializeComponent();

            Dispatcher.BeginInvoke(
                () =>
                {
                    var bitmap = new WriteableBitmap(canvasImage, null);
                    canvasImage.Source = bitmap;

                    plotIndicator.Visibility = System.Windows.Visibility.Visible;
                    var plotCanvas = new PlotCanvas(bitmap.PixelWidth, bitmap.PixelHeight, defaultTopLeft, defaultBottomRight);
                    foreach (var y in Enumerable.Range(0, plotCanvas.Height))
                        foreach (var x in Enumerable.Range(0, plotCanvas.Width))
                        {
                            plotCanvas[x, y] = MandelbrotSet.Calculate(plotCanvas.GetPoint(x, y), limit);
                            bitmap.Pixels[x + y * plotCanvas.Width] = plotCanvas[x, y] == -1 ? MakeArgb(255, 0, 0, 0) : MakeArgb(0, 255, 255, 255);
                            bitmap.Invalidate();
                        }
                    //foreach (var i in Enumerable.Range(0, plotCanvas.Width * plotCanvas.Height))
                    //    bitmap.Pixels[i] = (plotCanvas.ElementAt(i) == -1) ? MakeArgb(255, 0, 0, 0) : MakeArgb(255, 255, 255, 255);
                    topBoundIndicator.Text = plotCanvas.TopLeft.Imaginary.ToString("F2");
                    leftBoundIndicator.Text = plotCanvas.TopLeft.Real.ToString("F2");
                    bottomBoundIndicator.Text = plotCanvas.BottomRight.Imaginary.ToString("F2");
                    rightBoundIndicator.Text = plotCanvas.BottomRight.Real.ToString("F2");
                    plotIndicator.Visibility = System.Windows.Visibility.Collapsed;
                });
        }

        static int MakeArgb(byte a, byte r, byte g, byte b)
        {
            return (a << 24) + (r << 16) + (g << 8) + b;
        }
    }
}
