using Livet;
using Livet.Commands;
using Loader;
using Microsoft.Win32;
using PreviewTest.Extensions;
using PreviewTest.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PreviewTest.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public string PreviewModelPath
        {
            get => _PreviewModelPath;
            set => RaisePropertyChangedIfSet(ref _PreviewModelPath, value);
        }
        string _PreviewModelPath = string.Empty;

        public GeometryModel3D PreviewModel
        {
            get => _PreviewModel;
            set => RaisePropertyChangedIfSet(ref _PreviewModel, value);
        }
        GeometryModel3D _PreviewModel = null;

        public Vector3D LightDirection
        {
            get => _LightDirection;
            set => RaisePropertyChangedIfSet(ref _LightDirection, value);
        }
        Vector3D _LightDirection = new Vector3D(0, 1, 0);

        public PerspectiveCamera Camera
        {
            get => _Camera;
            set => RaisePropertyChangedIfSet(ref _Camera, value);
        }
        PerspectiveCamera _Camera = null;

        public Point3D CameraPosition
        {
            get => _CameraPosition;
            set => UpdateCameraPosition(value);
        }
        Point3D _CameraPosition = new Point3D(0, 5, -15);

        public Vector3D CameraLookAt
        {
            get => _CameraLookAt;
            set => UpdateCameraLookAt(value);
        }
        Vector3D _CameraLookAt = new Vector3D(0, 0, 0);

        public Vector3D CameraUpDirection
        {
            get => _CameraUpDirection;
            set => UpdateCameraUpDirection(value);
        }
        Vector3D _CameraUpDirection = new Vector3D(0, 1, 0);

        public double CameraDistance
        {
            get => _CameraDistance;
            set => UpdateCameraDistance(value);
        }
        double _CameraDistance = 10;

        public bool Loading
        {
            get => _Loading;
            set => RaisePropertyChangedIfSet(ref _Loading, value);
        }
        bool _Loading = false;

        public double LoadProgress
        {
            get => _LoadProgress;
            set => RaisePropertyChangedIfSet(ref _LoadProgress, value);
        }
        double _LoadProgress = 0.0;

        public string LoadingState
        {
            get => _LoadingState;
            set => RaisePropertyChangedIfSet(ref _LoadingState, value);
        }
        string _LoadingState = string.Empty;

        public bool IsIndeterminateLoadTime
        {
            get => _IsIndeterminateLoadTime;
            set => RaisePropertyChangedIfSet(ref _IsIndeterminateLoadTime, value);
        }
        bool _IsIndeterminateLoadTime = false;


        public ViewModelCommand LoadedCommand => _LoadedCommand.Get(Loaded);
        ViewModelCommandHandler _LoadedCommand = new ViewModelCommandHandler();

        public ViewModelCommand ExplorerCommand => _ExplorerCommand.Get(Explorer);
        ViewModelCommandHandler _ExplorerCommand = new ViewModelCommandHandler();

        public ViewModelCommand ResetCameraCommand => _ResetCameraCommand.Get(ResetCamera);
        ViewModelCommandHandler _ResetCameraCommand = new ViewModelCommandHandler();

        Point3D _ResetPosition = new Point3D();
        Vector3D _ResetLookAt = new Vector3D();
        Vector3D _ResetUpDirection = new Vector3D();
        Transform3DGroup _PreviewModelTransfrom = new Transform3DGroup();
        ScaleTransform3D _PreviewModelScale = new ScaleTransform3D();
        RotateTransform3D _PreviewModelRotation = new RotateTransform3D();
        TranslateTransform3D _PreviewModelTranslation = new TranslateTransform3D();

        public MainWindowViewModel()
        {

        }

        void Loaded()
        {
            // developing test.
            PreviewModel = GeometryMaker.MakeCube3D();

            _ResetPosition = CameraPosition;
            _ResetLookAt = CameraLookAt;
            _ResetUpDirection = CameraUpDirection;

            var nCameraLookDir = (CameraLookAt - CameraPosition).NormalizeToVector3D();
            Camera = new PerspectiveCamera(CameraPosition, nCameraLookDir, CameraUpDirection, 45);

            UpdateCamera();
        }

        void Explorer()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "OBJ file(*.obj)|*.obj";
            ofd.Title = "open to preview obj file.";
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == false)
            {
                return;
            }

            PreviewModelPath = ofd.FileName;

            Loading = true;
            IsIndeterminateLoadTime = true;
            LoadingState = "Reading obj file...";

            var handle = ObjLoader.CreateHandle();
            var result = ObjLoader.LoadAsync(handle, PreviewModelPath, loadedHandle =>
            {
                handle = loadedHandle;

                InvokeOnUIDispatcher(() =>
                {
                    _PreviewModelScale.ScaleX = 10.0;
                    _PreviewModelScale.ScaleY = 10.0;
                    _PreviewModelScale.ScaleZ = 10.0;
                    _PreviewModelTranslation.OffsetY = 1;

                    _PreviewModelTransfrom.Children.Add(_PreviewModelScale);
                    _PreviewModelTransfrom.Children.Add(_PreviewModelRotation);
                    _PreviewModelTransfrom.Children.Add(_PreviewModelTranslation);

                    var objMesh = new MeshGeometry3D()
                    {
                        Positions = new Point3DCollection(handle.Vertices.Select(arg => arg.Position.ToPoint3D()))
                    };

                    PreviewModel = new GeometryModel3D()
                    {
                        Geometry = objMesh,
                        Material = new DiffuseMaterial(Brushes.Gray),
                        Transform = _PreviewModelTransfrom
                    };

                    Loading = false;
                });
            },
            (maxParseCount, parsingCount) =>
            {
                if (parsingCount == 0)
                {
                    IsIndeterminateLoadTime = false;
                }

                // phase 1 progress
                LoadProgress = parsingCount / (double)maxParseCount;

                var percent = LoadProgress * 100;
                LoadingState = string.Format("Parsing {0:0.000} %", percent);
            },
            (maxConstructCount, constructingCount) =>
            {
                // phase 2 progress
                LoadProgress = constructingCount / (double)maxConstructCount;

                var percent = LoadProgress * 100;
                LoadingState = string.Format("Constructing {0:0.000} %", percent);
            });

            if (result == false)
            {
                // TODO: error handling.
                return;
            }
        }

        void ResetCamera()
        {
            CameraPosition = _ResetPosition;
            CameraLookAt = _ResetLookAt;
            CameraUpDirection = _ResetUpDirection;
        }

        void UpdateCameraPosition(Point3D value)
        {
            _CameraPosition = value;
            RaisePropertyChanged(nameof(CameraPosition));
            UpdateCamera();
        }

        void UpdateCameraLookAt(Vector3D value)
        {
            _CameraLookAt = value;
            RaisePropertyChanged(nameof(CameraLookAt));
            UpdateCamera();
        }

        void UpdateCameraUpDirection(Vector3D value)
        {
            _CameraUpDirection = value;
            RaisePropertyChanged(nameof(CameraUpDirection));
            UpdateCamera();
        }

        void UpdateCameraDistance(double value)
        {
            _CameraDistance = value;
            RaisePropertyChanged(nameof(CameraDistance));
            UpdateCamera();
        }

        void UpdateCamera()
        {
            if (Camera != null)
            {
                var nCameraLookDir = (CameraLookAt - CameraPosition).NormalizeToVector3D();
                Camera.Position = CameraPosition;
                Camera.LookDirection = nCameraLookDir;
                Camera.UpDirection = CameraUpDirection;

                LightDirection = nCameraLookDir;
            }
        }
    }
}
