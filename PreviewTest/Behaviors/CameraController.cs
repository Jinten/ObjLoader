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
        bool _IsCameraNeckRotating = false;
        bool _IsUpdatingMatrix = false;

        Point _CapturePosOnElement = ZeroPoint;
        Vector _MouseMovingVector = ZeroVector;
        Vector _MouseRotationVector = ZeroVector;
        Vector _MouseNeckRotationVector = ZeroVector;

        static readonly Point ZeroPoint = new Point(0, 0);
        static readonly Vector ZeroVector = new Vector(0, 0);
        static readonly Point3D ZeroPoint3D = new Point3D(0, 0, 0);
        static readonly Vector3D UpVector3D = new Vector3D(0, 1, 0);
        static readonly Vector3D FowardVector3D = new Vector3D(0, 0, 1);

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
            if (_IsCameraMoving == false && _IsCameraRotating == false && _IsCameraNeckRotating == false)
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

            if (_IsCameraNeckRotating)
            {
                _MouseNeckRotationVector = diffMoveMouseAmount;
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
                case MouseButton.Right:
                    _IsCameraNeckRotating = true;
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
                case MouseButton.Right:
                    _IsCameraNeckRotating = false;
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
            if (AssociatedObject == null || AssociatedObject.IsInitialized == false)
            {
                return;
            }

            if (_IsUpdatingMatrix)
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

            var currLookDir = (LookAt - Position).ToVector3D();

            if (_MouseNeckRotationVector.Length > 0)
            {
                var v = new Vector(0 - _MouseNeckRotationVector.X, 0 - _MouseNeckRotationVector.Y);
                var length = v.Length;

                v.NormalizeTo();

                var k = Math.Sign(Vector3D.DotProduct(FowardVector3D, currLookDir));
                var rotAxis = Vector3D.CrossProduct(FowardVector3D, new Vector3D(v.X, v.Y * k, 0));
                var lookDirRotation = FowardVector3D.MakeQuaternionBetweenVectors(currLookDir * k);

                var neckRotation = new Quaternion(rotAxis.TransfromNormal(lookDirRotation), length);
                currLookDir = currLookDir.TransfromNormal(neckRotation);

                LookAt = Position.ToVector3D() + currLookDir;

                _MouseNeckRotationVector = ZeroVector;
            }

            var xRotAxis = Vector3D.CrossProduct(UpVector3D, currLookDir);
            var yRotAxis = Vector3D.CrossProduct(xRotAxis, currLookDir);
            var xAxisRotation = new Quaternion(xRotAxis, _MouseRotationVector.Y);
            var yAxisRotation = new Quaternion(yRotAxis, _MouseRotationVector.X);
            _MouseRotationVector = ZeroVector;

            var rotation = xAxisRotation * yAxisRotation;
            var position = (-currLookDir.NormalizeTo() * Distance).TransfromNormal(rotation);
            Position = LookAt + position.ToPoint3D();

            UpDirection = UpVector3D.TransfromNormal(rotation);

            var zAxis = (LookAt - Position).NormalizeToVector3D();
            var xAxis = Vector3D.CrossProduct(UpVector3D, zAxis);
            var yAxis = Vector3D.CrossProduct(zAxis, xAxis);

            Matrix = new Matrix3D(xAxis.X, xAxis.Y, xAxis.Z, 0, yAxis.X, yAxis.Y, yAxis.Z, 0, zAxis.X, zAxis.Y, zAxis.Z, 0, Position.X, Position.Y, Position.Z, 1.0);

            _IsUpdatingMatrix = false;
        }
    }
}
