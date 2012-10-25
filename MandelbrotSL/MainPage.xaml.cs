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
using System.Threading.Tasks;

namespace MandelbrotSL
{
    public partial class MainPage : UserControl
    {
        static readonly int defaultIterationLimit = 20;
        static readonly double defaultThreshold = 2.0;
        static readonly Complex defaultTopLeft = new Complex(-2.0, 1.5);
        static readonly Complex defaultBottomRight = new Complex(1.0, -1.5);
        static readonly double defaultScaleStep = 0.1;
        // an allowable margin of computational error
        static readonly double eps = 0.001;

        WriteableBitmap bitmap;
        PlotCanvas canvas;
        bool isDrawing = false;
        int iterationLimit = defaultIterationLimit;
        double threshold = defaultThreshold;
        Complex topLeft = defaultTopLeft;
        Complex bottomRight = defaultBottomRight;
        double scaleStep = defaultScaleStep;
        bool isColorInverted = false;

        bool isDragging;
        Point dragStartPos;
        Complex dragStartLeftTop;
        Complex dragStartRightBottom;

        public MainPage()
        {
            InitializeComponent();

            bitmap = new WriteableBitmap(canvasImage, null);
            canvasImage.Source = bitmap;
        }

        #region Event Handlers
        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            // Apply the default values to the controls.
            thresholdSlider.Value = threshold;
            iterationSlider.Value = iterationLimit;
            scaleStepSlider.Value = scaleStep;

            CreateNewCanvas();

            Draw();
        }

        private void UserControl_KeyUp_1(object sender, KeyEventArgs e)
        {
            // Redraw
            if (e.Key == Key.R && !isDrawing)
                Draw();

            // Full redraw
            if (e.Key == Key.F && !isDrawing)
                FullRedraw();
            
            // Reset
            if (e.Key == Key.O && !isDrawing)
            {
                topLeft = defaultTopLeft;
                bottomRight = defaultBottomRight;
                iterationLimit = defaultIterationLimit;
                threshold = defaultThreshold;
                FullRedraw();
            }
        }

        private void canvasImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var p = e.GetPosition(canvasImage);
            // Do nothing when the cursor is not in the element.
            if (p.X < 0 || (int)canvasImage.ActualWidth < p.X || p.Y < 0 || (int)canvasImage.ActualHeight < p.Y)
                return;
            
            // Calculate the relative position from the left-top of canvasImage. (range: 0 - 1)
            var relativeX = p.X / canvasImage.ActualWidth;
            var relativeY = p.Y / canvasImage.ActualHeight;

            if (e.Delta < 0)    // Zoom out
            {
                topLeft += new Complex(-scaleStep * relativeX, scaleStep * relativeY);
                bottomRight += new Complex(scaleStep * (1 - relativeX), -scaleStep * (1 - relativeY));
            }
            else if (e.Delta > 0)   // Zoom in
            {
                topLeft += new Complex(scaleStep * relativeX, -scaleStep * relativeY);
                bottomRight += new Complex(-scaleStep * (1 - relativeX), scaleStep * (1 - relativeY));
            }

            wheelPositionIndicateText.Text = string.Format("({0:F2},{1:F2})", relativeX, relativeY);

            CheckDomain();
            FullRedraw();
        }

        private void settingsCloseButton_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            // Apply the values of the slider controls to the fields.
            iterationLimit = Convert.ToInt32(iterationSlider.Value);
            threshold = thresholdSlider.Value;
            topLeft = new Complex(reMinSlider.Value, imMaxSlider.Value);
            bottomRight = new Complex(reMaxSlider.Value, imMinSlider.Value);
            scaleStep = scaleStepSlider.Value;
            isColorInverted = invertColorCheckbox.IsChecked ?? false;
            FullRedraw();
        }

        private void settingsOpenButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the values of the fields to the sliders.
            iterationSlider.Value = iterationLimit;
            thresholdSlider.Value = threshold;
            reMinSlider.Value = topLeft.Real;
            reMaxSlider.Value = bottomRight.Real;
            imMinSlider.Value = bottomRight.Imaginary;
            imMaxSlider.Value = topLeft.Imaginary;
            invertColorCheckbox.IsChecked = isColorInverted;
        }
        #endregion

        void Draw()
        {
            isDrawing = true;

            canvas.Draw(iterationLimit, threshold);

            UpdateBoundText();

            isDrawing = false;
        }

        private void UpdateBoundText()
        {
            // Update bounds
            topBoundIndicator.Text = canvas.TopLeft.Imaginary.ToString("F2");
            leftBoundIndicator.Text = canvas.TopLeft.Real.ToString("F2");
            bottomBoundIndicator.Text = canvas.BottomRight.Imaginary.ToString("F2");
            rightBoundIndicator.Text = canvas.BottomRight.Real.ToString("F2");
        }

        // Recreate new canvas and draw. Call this method when the scale is changed.
        void FullRedraw()
        {
            CreateNewCanvas();
            Draw();
        }

        private void CreateNewCanvas()
        {
            canvas = new PlotCanvas(bitmap.PixelWidth, bitmap.PixelHeight, topLeft, bottomRight);
            canvas.DrawCompleted += (sender, e) => Dispatcher.BeginInvoke(() => UpdateBitmap());
        }

        private void UpdateBitmap()
        {
            // Iteration with index
            foreach (var it in canvas.Select((v, i) => new { Value = v, Index = i }))
                bitmap.Pixels[it.Index] = GetColor(it.Value);
            bitmap.Invalidate();
        }

        private void CheckDomain()
        {
            // Check the lower and higher bound value. If they are same, show the caution.
            if (Math.Abs(topLeft.Real - bottomRight.Real) < eps || Math.Abs(topLeft.Imaginary - bottomRight.Imaginary) < eps)
                domainCautionBorder.Visibility = System.Windows.Visibility.Visible;
            else
                domainCautionBorder.Visibility = System.Windows.Visibility.Collapsed;
        }

        private int GetColor(int diverge)
        {
            if (diverge < 0)
                if (isColorInverted)
                    return MakeArgb(255, 0, 0, 0);
                else
                    return MakeArgb(255, 255, 255, 255);

            // [0, iterationLimit] -> [0, 255]
            var v = (byte)(Map(diverge, 0, iterationLimit, 0, 255));
            if (isColorInverted)
                v = (byte)(255 - v);
            return MakeArgb(255, v, v, v);
        }

        private static int MakeArgb(byte a, byte r, byte g, byte b)
        {
            return (a << 24) + (r << 16) + (g << 8) + b;
        }

        private static int Map(int value, int fromMin, int fromMax, int toMin, int toMax)
        {
            return (value - fromMin) * (toMax - toMin) / (fromMax - fromMin) + toMin;
        }

        private void navigationPanel_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            navigationPanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void canvasImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDragging = true;
            var p = e.GetPosition(canvasImage);
            dragPositionIndicateBorder.Opacity = 1;
            canvasImage.CaptureMouse();

            // Calculate the relative position from the left-top of canvasImage. (range: 0 - 1)
            var relativeX = p.X / canvasImage.ActualWidth;
            var relativeY = p.Y / canvasImage.ActualHeight;

            dragStartPos = new Point(relativeX, relativeY);
            dragStartLeftTop = topLeft;
            dragStartRightBottom = bottomRight;
        }

        private void canvasImage_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDragging = false;
            canvasImage.ReleaseMouseCapture();

            FullRedraw();

            dragPositionIndicateBorder.Opacity = 0;
        }

        private void canvasImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isDragging)
                return;
            var current = e.GetPosition(canvasImage);

            // Calculate the relative position from the left-top of canvasImage. (range: 0 - 1)
            var currentRelativeX = current.X / canvasImage.ActualWidth;
            var currentRelativeY = current.Y / canvasImage.ActualHeight;

            var diffX = currentRelativeX - dragStartPos.X;
            var diffY = currentRelativeY - dragStartPos.Y;

            topLeft -= new Complex(diffX, -diffY);
            bottomRight -= new Complex(diffX, -diffY);

            dragPositionIndicateText.Text = string.Format("(({4:F2},{5:F2}),({6:F2},{7:F2}))",
                dragStartLeftTop.Real, dragStartLeftTop.Imaginary, dragStartRightBottom.Real, dragStartRightBottom.Imaginary,
                topLeft.Real, topLeft.Imaginary, bottomRight.Real, bottomRight.Imaginary);
            dragStartPos = new Point(currentRelativeX, currentRelativeY);
        }
    }
}
