using PreviewTest.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media.Media3D;

namespace PreviewTest.Behaviors
{
    internal class CameraController : Behavior<FrameworkElement>
    {
        public double FOV
        {
            get => (double)GetValue(FOVProperty);
            set => SetValue(FOVProperty, value);
        }
        public static readonly DependencyProperty FOVProperty =
            DependencyProperty.Register(nameof(FOV), typeof(double), typeof(CameraController), new FrameworkPropertyMetadata(45.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, FOVPropertyChanged));

        public Point3D Position
        {
            get => (Point3D)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(Point3D), typeof(CameraController), new FrameworkPropertyMetadata(new Point3D(0, 0, -5), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PositionPropertyChanged));

        public Vector3D LookAt
        {
            get => (Vector3D)GetValue(LookAtProperty);
            set => SetValue(LookAtProperty, value);
        }
        public static readonly DependencyProperty LookAtProperty =
            DependencyProperty.Register(nameof(LookAt), typeof(Vector3D), typeof(CameraController), new FrameworkPropertyMetadata(new Vector3D(0, 0, 0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, LookAtPropertyChanged));

        public Vector3D UpDirection
        {
            get => (Vector3D)GetValue(UpDirectionProperty);
            set => SetValue(UpDirectionProperty, value);
        }
        public static readonly DependencyProperty UpDirectionProperty =
            DependencyProperty.Register(nameof(UpDirection), typeof(Vector3D), typeof(CameraController), new FrameworkPropertyMetadata(new Vector3D(0, 1, 0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, UpDirectionPropertyChanged));

        public double Distance
        {
            get => (double)GetValue(DistanceProperty);
            set => SetValue(DistanceProperty, value);
        }
        public static readonly DependencyProperty DistanceProperty =
            DependencyProperty.Register(nameof(Distance), typeof(double), typeof(CameraController), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, DistancePropertyChanged));

        public double DistanceScaleRate
        {
            get => (double)GetValue(DistanceScaleRateProperty);
            set => SetValue(DistanceScaleRateProperty, value);
        }
        public static readonly DependencyProperty DistanceScaleRateProperty =
            DependencyProperty.Register(nameof(DistanceScaleRate), typeof(double), typeof(CameraController), new FrameworkPropertyMetadata(1.0));

        public Matrix3D Matrix
        {
            get => (Matrix3D)GetValue(MatrixProperty);
            set => SetValue(MatrixProperty, value);
        }
        public static readonly DependencyProperty MatrixProperty =
            DependencyProperty.Register(nameof(Matrix), typeof(Matrix3D), typeof(CameraController), new FrameworkPropertyMetadata(Matrix3D.Identity));

        bool _IsCameraMoving = false;
        bool _IsCameraRotating = false;
        bool _IsUpdatingMatrix = false;

        Point _CapturePosOnElement = new Point(0, 0);
        Vector _MouseMovingVector = new Vector(0, 0);
        Vector _MouseRotationVector = new Vector(0, 0);

        static readonly Vector ZeroVector = new Vector(0, 0);
        static readonly Point3D ZeroPoint = new Point3D(0, 0, 0);
        static readonly Vector3D UpVector3D = new Vector3D(0, 1, 0);

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            AssociatedObject.MouseUp += AssociatedObject_MouseUp;
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseWheel += AssociatedObject_MouseWheel;

            Distance = (Position - LookAt).Length();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseWheel -= AssociatedObject_MouseWheel;
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.MouseUp -= AssociatedObject_MouseUp;
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;

            base.OnDetaching();
        }


        void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var nDelta = e.Delta / 120.0;
            Distance = Math.Max(0.1, Distance - nDelta * DistanceScaleRate);
            UpdateMatrix();
        }

        void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if(_IsCameraMoving == false && _IsCameraRotating == false)
            {
                return;
            }

            var posOnElement = e.GetPosition(AssociatedObject);
            var diffMoveMouseAmount = posOnElement - _CapturePosOnElement;
            _CapturePosOnElement = posOnElement;

            if (_IsCameraMoving)
            {
                _MouseMovingVector = diffMoveMouseAmount * 0.1;
            }

            if (_IsCameraRotating)
            {
                _MouseRotationVector = diffMoveMouseAmount;
            }

            UpdateMatrix();
        }

        void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    _IsCameraRotating = true;
                    break;
                case MouseButton.Middle:
                    _IsCameraMoving = true;
                    break;
            }

            _CapturePosOnElement = e.GetPosition(AssociatedObject);
        }

        void AssociatedObject_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    _IsCameraRotating = false;
                    break;
                case MouseButton.Middle:
                    _IsCameraMoving = false;
                    break;
            }
        }

        static void FOVPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // must change matrix
            var controller = d as CameraController;
            controller.UpdateMatrix();
        }

        static void PositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // must change matrix
            var controller = d as CameraController;
            controller.UpdateMatrix();
        }

        static void LookAtPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // must change matrix
            var controller = d as CameraController;
            controller.UpdateMatrix();
        }

        static void UpDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // must change matrix
            var controller = d as CameraController;
            controller.UpdateMatrix();
        }

        static void DistancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // must change matrix
            var controller = d as CameraController;
            controller.UpdateMatrix();
        }

        void UpdateMatrix()
        {
            if(AssociatedObject == null || AssociatedObject.IsInitialized == false)
            {
                return;
            }

            if(_IsUpdatingMatrix)
            {
                return;
            }

            _IsUpdatingMatrix = true;

            if (_MouseMovingVector.Length > 0)
            {
                var prevMatrix = Matrix;
                prevMatrix.OffsetX = 0;
                prevMatrix.OffsetY = 0;
                prevMatrix.OffsetZ = 0;

                var move = prevMatrix.Transform(new Point3D(_MouseMovingVector.X, _MouseMovingVector.Y, 0));
                LookAt += move.ToVector3D();
                Position += move.ToVector3D();
                _MouseMovingVector = ZeroVector;
            }

            var currLookDir = (LookAt - Position).NormalizeToVector3D();
            var xRotAxis = Vector3D.CrossProduct(UpVector3D, currLookDir);
            var yRotAxis = Vector3D.CrossProduct(xRotAxis, currLookDir);
            var xAxisRotation = new Quaternion(xRotAxis, _MouseRotationVector.Y);
            var yAxisRotation = new Quaternion(yRotAxis, _MouseRotationVector.X);
            _MouseRotationVector = ZeroVector;

            var rotation = xAxisRotation * yAxisRotation;
            var transform = new RotateTransform3D(new QuaternionRotation3D(rotation), ZeroPoint);

            var pos = transform.Transform(Vector3D.Multiply(Distance, -currLookDir));
            Position = LookAt + pos.ToPoint3D();

            var up = transform.Transform(Point3D.Add(ZeroPoint, new Vector3D(0, 1, 0)));
            UpDirection = new Vector3D(up.X, up.Y, up.Z);

            var zAxis = (LookAt - Position).NormalizeToVector3D();
            var xAxis = Vector3D.CrossProduct(UpVector3D, zAxis);
            var yAxis = Vector3D.CrossProduct(zAxis, xAxis);

            Matrix = new Matrix3D(xAxis.X, xAxis.Y, xAxis.Z, 0, yAxis.X, yAxis.Y, yAxis.Z, 0, zAxis.X, zAxis.Y, zAxis.Z, 0, Position.X, Position.Y, Position.Z, 1.0);
            
            _IsUpdatingMatrix = false;
        }
    }
}
