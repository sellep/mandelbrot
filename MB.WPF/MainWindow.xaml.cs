using System;
using System.Collections.Generic;
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
        private const int _WIDTH = 1920;
        private const int _HEIGHT = 1080;
        private const int _PACKAGES_PER_FRAME = 10000;

        private SolidColorBrush[] _Brushes;

        DoubleComplex _BoundsMin = new DoubleComplex(-2, -1.5);
        DoubleComplex _BoundsMax = new DoubleComplex(0.5, 1.5);

        private Point? _Start;
        private Rectangle _Rect = new Rectangle();

        private ComputationProvider _Provider;
        private ComputationPackage _Package = null;

        public MainWindow()
        {
            InitializeComponent();

            _Brushes = ColorInterpolator.CreatePalette(100, Colors.Black, Colors.Black, 1000).Select(c => new SolidColorBrush(c)).ToArray();

            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseLeftButtonUp += MainWindow_MouseLeftButtonUp;
            MouseMove += MainWindow_MouseMove;

            _Rect.Opacity = 0.4;
            _Rect.Fill = Brushes.Red;

            _Provider = new ComputationProvider(new Uri("net.tcp://localhost/mb/"));

            ApplyPackage();
        }

        private void ApplyPackage()
        {
            if (_Package != null)
            {
                _Package.FrameChanged += Package_FrameChanged;
            }

            IEnumerable<ComputationRequest> requests = DoubleComputationFactory.CreateRequests(_BoundsMin, _BoundsMax, _WIDTH, _HEIGHT, (uint)(_Brushes.Length - 1), _PACKAGES_PER_FRAME);

            _Package = new ComputationPackage(requests, _WIDTH, _HEIGHT);
            _Package.FrameChanged += Package_FrameChanged;

            _Provider.Apply(_Package);

            _Out.ImageSource = BitmapHelper.Create(_Package.Width, _Package.Height, _Package.Frame, _Brushes, Brushes.WhiteSmoke);
        }

        private void Package_FrameChanged(object sender, EventArgs e)
        {
            _Out.ImageSource = BitmapHelper.Create(_Package.Width, _Package.Height, _Package.Frame, _Brushes, Brushes.WhiteSmoke);
        }

        private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_Start == null)
                return;

            Point current = e.GetPosition(_Cnvs);

            double x_min_ratio = _Start.Value.X / _Cnvs.ActualWidth;
            double y_min_ratio = _Start.Value.Y / _Cnvs.ActualHeight;

            double width_ratio = (current.X - _Start.Value.X) / _Cnvs.ActualWidth;
            double height_ratio = (current.Y - _Start.Value.Y) / _Cnvs.ActualHeight;

            DoubleComplex logical_dim = _BoundsMax - _BoundsMin;

            _BoundsMin = new DoubleComplex(
                _BoundsMin.R + x_min_ratio * logical_dim.R,
                _BoundsMin.I + y_min_ratio * logical_dim.I);

            _BoundsMax = new DoubleComplex(
                _BoundsMin.R + logical_dim.R * width_ratio,
                _BoundsMin.I + logical_dim.I * height_ratio);

            ApplyPackage();

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
