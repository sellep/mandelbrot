using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using MB.Core;

namespace MB.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int _WIDTH = 840;
        private const int _HEIGHT = 680;
        private const int _PACKAGES_PER_FRAME = 500;

        private SolidColorBrush[] _Brushes;

        DoubleComplex _BoundsMin = new DoubleComplex(-2, -1.5);
        DoubleComplex _BoundsMax = new DoubleComplex(0.5, 1.5);

        private Point? _Start;
        private Rectangle _Rect = new Rectangle();

        private Project _Proj = null;
        private ComputationProvider _Provider;

        public MainWindow()
        {
            InitializeComponent();

            _Brushes = ColorInterpolator.CreatePalette(100, Colors.Black, Colors.Black, 1000).Select(c => new SolidColorBrush(c)).ToArray();
            _Proj = Project.Initialize(_WIDTH, _HEIGHT, (uint)_Brushes.Length - 1, _PACKAGES_PER_FRAME, _BoundsMin, _BoundsMax);
            _Proj.FrameChanged += _Project_FrameChanged;

            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            MouseMove += MainWindow_MouseMove;

            _Rect.Opacity = 0.4;
            _Rect.Fill = Brushes.Red;

            _Project_FrameChanged(null, new EventArgs());

            _Provider = new ComputationProvider(new Uri("net.tcp://localhost/mb/"), _Proj);
        }

        private void _Project_FrameFinished(object sender, int[,] iframe)
        {
            _Project_FrameChanged(sender, new EventArgs());

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)_Out.ImageSource));

            using (Stream s = File.OpenWrite(_Proj.GetCurrentBitmapPath()))
            {
                encoder.Save(s);
            }
        }

        private void _Project_FrameChanged(object sender, EventArgs e)
        {
            _Out.ImageSource = BitmapHelper.Create(_Proj.Width, _Proj.Height, _Proj.CurrentFrame, _Brushes, Brushes.WhiteSmoke);
        }

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_Start == null)
                return;

            Point current = e.GetPosition(_Cnvs);

            double widthRatio = _WIDTH / _Cnvs.ActualWidth;
            double heightRatio = _HEIGHT / _Cnvs.ActualHeight;

            // here must call project create frame
            int zoomWidth = (int)(widthRatio * (current.X - _Start.Value.X));
            int zoomHeight = (int)(heightRatio * (current.Y - _Start.Value.Y));
            int offsetX = (int)(widthRatio * _Start.Value.X);
            int offsetY = (int)(heightRatio * _Start.Value.Y);

            _Proj.SetZoomRequest(zoomWidth, zoomHeight, offsetX, offsetY);

            _Cnvs.Children.Clear();
            _Start = null;
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (_Start == null)
                return;

            Point current = e.GetPosition(_Cnvs);

            double width = current.X - _Start.Value.X;
            double height = _Cnvs.ActualHeight / _Cnvs.ActualWidth * width;

            if (width < 0)
            {
                _Cnvs.Children.Clear();
                _Start = null;
                return;
            }

            _Rect.Width = width;
            _Rect.Height = height;
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _Start = e.GetPosition(_Cnvs);

            Canvas.SetLeft(_Rect, _Start.Value.X);
            Canvas.SetTop(_Rect, _Start.Value.Y);

            _Cnvs.Children.Add(_Rect);
        }
    }
}
