using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using LagoVista.Core.Models.Drawing;
using LagoVista.GCode.Sender.Models;
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

namespace ShotTtracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        VisionProfile Profile { get; set; }


        FloatMedianFilter _cornerMedianFilter = new FloatMedianFilter(12, 3);
        FloatMedianFilter _circleMedianFilter = new FloatMedianFilter(12, 3);
        FloatMedianFilter _circleRadiusMedianFilter = new FloatMedianFilter(12, 3);


        protected void Circle(IInputOutputArray img, int x, int y, int radius, System.Drawing.Color color, int thickness = 1)
        {

            CvInvoke.Circle(img,
            new System.Drawing.Point(x, y), radius,
            new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);

        }

        protected void Line(IInputOutputArray img, int x1, int y1, int x2, int y2, System.Drawing.Color color, int thickness = 1)
        {

            CvInvoke.Line(img, new System.Drawing.Point(x1, y1),
                new System.Drawing.Point(x2, y2),
                new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);
        }

        private void FindCircles(IInputOutputArray input, IInputOutputArray output, System.Drawing.Size size)
        {

            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            var circles = CvInvoke.HoughCircles(input, HoughType.Gradient, Profile.HoughCirclesDP, Profile.HoughCirclesMinDistance, Profile.HoughCirclesParam1, Profile.HoughCirclesParam2, Profile.HoughCirclesMinRadius, Profile.HoughCirclesMaxRadius);

            var foundCircle = false;
            /* Above will return ALL maching circles, we only want the first one that is in the target image radius in the middle of the screen */
            foreach (var circle in circles)
            {
                if (circle.Center.X > ((size.Width / 2) - Profile.TargetImageRadius) && circle.Center.X < ((size.Width / 2) + Profile.TargetImageRadius) &&
                   circle.Center.Y > ((size.Height / 2) - Profile.TargetImageRadius) && circle.Center.Y < ((size.Height / 2) + Profile.TargetImageRadius))
                {
                    _circleMedianFilter.Add(circle.Center.X, circle.Center.Y);
                    _circleRadiusMedianFilter.Add(circle.Radius, 0);
                    foundCircle = true;
                    break;
                }
            }

            if (!foundCircle)
            {
                _circleMedianFilter.Add(null);
                _circleRadiusMedianFilter.Add(null);
            }

            var avg = _circleMedianFilter.Filtered;
            if (avg != null)
            {


                
                    Line(output, 0, (int)avg.Y, size.Width, (int)avg.Y, System.Drawing.Color.Red);
                    Line(output, (int)avg.X, 0, (int)avg.X, size.Height, System.Drawing.Color.Red);
                    Circle(output, (int)avg.X, (int)avg.Y, (int)_circleRadiusMedianFilter.Filtered.X, System.Drawing.Color.Red);
                
            }
            else
            {

            }
        }


        public IImage PerformShapeDetection(Mat img)
        {
            if (img == null)
            {
                return null;
            }

            try
            {
                using (var gray = new Image<Gray, byte>(img.Bitmap))
                using (var blurredGray = new Image<Gray, float>(gray.Size))
                {
                    var output = img ;

                    var input = (IImage)gray;
                    if (false)
                    {
                        CvInvoke.GaussianBlur(gray, blurredGray, new System.Drawing.Size(5, 5), Profile.GaussianSigmaX);
                        input = blurredGray;
                    }

                    FindCircles(input, output, img.Size);
                    return img;

                    return gray.Clone();
                }
            }
            catch (Exception)
            {
                /*NOP, sometimes OpenCV acts a little funny. */
                return null;
            }

        }

        VideoCapture _topCameraCapture;
        VideoCapture _bottomCameraCapture;

        Object _videoCaptureLocker = new object();

        private VideoCapture InitCapture(int cameraIndex)
        {
            try
            {
                return new VideoCapture(cameraIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open camera: " + ex.Message);
                return null;
            }
        }

        private bool _running;

        private double _lastTopBrightness = -9999;
        private double _lastBottomBrightness = -9999;


        private double _lastTopFocus = -9999;
        private double _lastBottomFocus = -9999;

        private double _lastTopExposure = -9999;
        private double _lastBottomExposure = -9999;

        private double _lastTopContrast = -9999;
        private double _lastBottomContrast = -9999;

        public virtual bool UseTopCamera { get; set; } = true;
        public virtual bool UseBottomCamera { get; set; } = false;

        private async void StartImageRecognization()
        {
            _running = true;

            while (_running)
            {
                
                if (_bottomCameraCapture != null)
                {
                    _bottomCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

                    if (_lastBottomBrightness != Profile.Brightness)
                    {
                        _bottomCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, Profile.Brightness);
                        _lastBottomBrightness = Profile.Brightness;
                    }

                    if (_lastBottomFocus != Profile.Focus)
                    {
                        _bottomCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, Profile.Focus);
                        _lastBottomFocus = Profile.Focus;
                    }

                    if (_lastBottomContrast != Profile.Contrast)
                    {
                        _bottomCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, Profile.Contrast);
                        _lastBottomContrast = Profile.Contrast;
                    }

                    if (_lastBottomExposure != Profile.Exposure)
                    {
                        _bottomCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, Profile.Exposure);
                        _lastBottomExposure = Profile.Exposure;
                    }

                    if (UseBottomCamera)
                    {
                        using (var originalFrame = _bottomCameraCapture.QueryFrame())
                        using (var results = PerformShapeDetection(originalFrame))
                        {
                            if (ShowBottomCamera)
                            {
                                PrimaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
                            }
                            else if (PictureInPicture)
                            {
                                SecondaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
                            }
                        }
                    }

                    HasFrame = true;

                    if (LoadingMask)
                    {
                        LoadingMask = false;
                    }

                    if (!ShowTopCamera && !ShowBottomCamera)
                    {
                        PrimaryCapturedImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));
                        SecondaryCapturedImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));
                    }
                }

                await Task.Delay(100);
            }


            HasFrame = false;
        }

        public void StartCapture()
        {
            if (_topCameraCapture != null || _bottomCameraCapture != null)
            {
                return;
            }

            if (Machine.Settings.PositioningCamera == null && Machine.Settings.PartInspectionCamera == null)
            {
                MessageBox.Show("Please Select a Camera");
                new SettingsWindow(Machine, Machine.Settings, 2).ShowDialog();
                return;
            }

            try
            {
                LoadingMask = true;

                var positionCameraIndex = Machine.Settings.PositioningCamera == null ? null : (int?)Machine.Settings.PositioningCamera.CameraIndex;
                var inspectionCameraIndex = Machine.Settings.PartInspectionCamera == null ? null : (int?)Machine.Settings.PartInspectionCamera.CameraIndex;

                if (positionCameraIndex.HasValue && inspectionCameraIndex.HasValue)
                {
                    if (positionCameraIndex.Value < inspectionCameraIndex.Value)
                    {
                        _topCameraCapture = InitCapture(Machine.Settings.PositioningCamera.CameraIndex);
                        _bottomCameraCapture = InitCapture(Machine.Settings.PartInspectionCamera.CameraIndex);
                    }
                    else
                    {
                        _bottomCameraCapture = InitCapture(Machine.Settings.PartInspectionCamera.CameraIndex);
                        _topCameraCapture = InitCapture(Machine.Settings.PositioningCamera.CameraIndex);
                    }
                    StartImageRecognization();
                }
                else if (positionCameraIndex.HasValue)
                {
                    _topCameraCapture = InitCapture(Machine.Settings.PositioningCamera.CameraIndex);
                    StartImageRecognization();
                }
                else if (inspectionCameraIndex.HasValue)
                {
                    _bottomCameraCapture = InitCapture(Machine.Settings.PartInspectionCamera.CameraIndex);
                    StartImageRecognization();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start video, please restart your application: " + ex.Message);
            }

        }

        public void StopCapture()
        {
            try
            {
                _running = false;

                lock (_videoCaptureLocker)
                {
                    if (_topCameraCapture != null)
                    {
                        _topCameraCapture.Stop();
                        _topCameraCapture.Dispose();
                        _topCameraCapture = null;
                    }

                    if (_bottomCameraCapture != null)
                    {
                        _bottomCameraCapture.Stop();
                        _bottomCameraCapture.Dispose();
                        _bottomCameraCapture = null;
                    }
                }

                PrimaryCapturedImage = new BitmapImage(new Uri("pack://application:,,/Imgs/TestPattern.jpg"));
                SecondaryCapturedImage = new BitmapImage(new Uri("pack://application:,,/Imgs/TestPattern.jpg"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Shutting Down Video, please restart the application." + ex.Message);
            }
            finally
            {
            }
        }

    }


    public class FloatMedianFilter
    {
        int _filterSize = 12;
        int _throwAwaySize = 3;

        public Point2D<float>[] _points;
        int _head;
        int _arraySize;

        public FloatMedianFilter(int size = 12, int throwAway = 3)
        {
            _filterSize = size;
            _throwAwaySize = throwAway;
            _points = new Point2D<float>[size];
        }

        public void Add(Point2D<float> point)
        {
            _points[_head++] = point;
            if (_head == _filterSize)
            {
                _head = 0;
            }

            _arraySize++;
            _arraySize = Math.Min(_filterSize, _arraySize);
        }

        public void Add(float x, float y)
        {
            Add(new Point2D<float>(x, y));
        }

        public void Reset()
        {
            for (var idx = 0; idx < _points.Length; ++idx)
            {
                _points[idx] = null;
            }
        }

        public Point2D<double> StandardDeviation
        {
            get
            {
                var nonNullPoints = _points.Where(pt => pt != null);

                if (nonNullPoints.Count() == _filterSize / 2)
                    return new Point2D<double>(999.9, 999.9);


                var avgX = nonNullPoints.Select(pt => pt.X).Average();
                var avgY = nonNullPoints.Select(pt => pt.Y).Average();

                var sigmaDeltaX = 0.0;
                var sigmaDeltaY = 0.0;

                foreach (var point in nonNullPoints)
                {
                    sigmaDeltaX += Math.Pow(point.X - avgX, 2);
                    sigmaDeltaY += Math.Pow(point.Y - avgY, 2);
                }

                return new Point2D<double>()
                {
                    X = Math.Round(Math.Sqrt(sigmaDeltaX / nonNullPoints.Count()), 2),
                    Y = Math.Round(Math.Sqrt(sigmaDeltaY / nonNullPoints.Count()), 2),
                };
            }
        }

        public Point2D<double> Filtered
        {
            get
            {
                var nonNullPoints = _points.Where(pt => pt != null);
                if (nonNullPoints.Count() < 5)
                    return null;

                var sortedX = nonNullPoints.Select(pt => pt.X).OrderBy(pt => pt);
                var sortedY = nonNullPoints.Select(pt => pt.Y).OrderBy(pt => pt);

                if (sortedX.Count() == _filterSize)
                {
                    var subsetX = sortedX.Skip(_throwAwaySize).Take(_filterSize - _throwAwaySize * 2);
                    var subsetY = sortedY.Skip(_throwAwaySize).Take(_filterSize - _throwAwaySize * 2);
                    return new Point2D<double>(Math.Round(subsetX.Average(), 4), Math.Round(subsetY.Average(), 4));
                }
                else
                {
                    var avgX = nonNullPoints.Select(pt => pt.X).Average();
                    var avgY = nonNullPoints.Select(pt => pt.Y).Average();
                    return new Point2D<double>(Math.Round(avgX, 4), Math.Round(avgY, 4));
                }
            }
        }
    }

}

