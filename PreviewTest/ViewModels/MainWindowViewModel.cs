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
using System.Windows;
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

        public Model3DGroup PreviewModel
        {
            get => _PreviewModel;
            set => RaisePropertyChangedIfSet(ref _PreviewModel, value);
        }
        Model3DGroup _PreviewModel = new Model3DGroup();

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

        public int VertexCount => _ObjHandle != null ? _ObjHandle.Vertices.Length : 0;
        public int IndexCount => _ObjHandle != null ? _ObjHandle.Indices.Length : 0;
        public int MasterMaterialCount => _ObjHandle != null ? _ObjHandle.MasterMaterials.Length : 0;

        public IEnumerable<string> MasterMaterialNames => _ObjHandle?.MasterMaterials.Select(arg => arg.Name);

        public int SelectedMaterialIndex
        {
            get => _SelectedMaterialIndex;
            set => UpdateSelectedMaterialIndex(value);
        }
        int _SelectedMaterialIndex = -1;

        public ViewModelCommand LoadedCommand => _LoadedCommand.Get(Loaded);
        ViewModelCommandHandler _LoadedCommand = new ViewModelCommandHandler();

        public ViewModelCommand ExplorerCommand => _ExplorerCommand.Get(Explorer);
        ViewModelCommandHandler _ExplorerCommand = new ViewModelCommandHandler();

        public ViewModelCommand ResetCameraCommand => _ResetCameraCommand.Get(ResetCamera);
        ViewModelCommandHandler _ResetCameraCommand = new ViewModelCommandHandler();

        public ViewModelCommand RecalculateNormalsCommand => _RecalculateNormalsCommand.Get(RecalculateNormals);
        ViewModelCommandHandler _RecalculateNormalsCommand = new ViewModelCommandHandler();

        Point3D _ResetPosition = new Point3D();
        Vector3D _ResetLookAt = new Vector3D();
        Vector3D _ResetUpDirection = new Vector3D();
        double _ResetDistance = 0.0;

        Transform3DGroup _PreviewModelTransfrom = new Transform3DGroup();
        ScaleTransform3D _PreviewModelScale = new ScaleTransform3D();
        RotateTransform3D _PreviewModelRotation = new RotateTransform3D();
        TranslateTransform3D _PreviewModelTranslation = new TranslateTransform3D();

        class MeshMaterial
        {
            public MaterialGroup Material { get; } = new MaterialGroup();
            public DiffuseMaterial Diffuse { get; set; } = null;
            public EmissiveMaterial Emissive { get; set; } = null;

            public MeshMaterial(Brush diffuse, Brush emissive)
            {
                Diffuse = new DiffuseMaterial(diffuse);
                Emissive = new EmissiveMaterial(emissive);

                Material.Children.Add(Diffuse);
                Material.Children.Add(Emissive);
            }
        };

        IObjHandle _ObjHandle = null;
        MeshMaterial[] _PreviewMeshMaterials = null;

        public MainWindowViewModel()
        {

        }

        void Loaded()
        {
            // developing test.

            PreviewModel.Children.Add(GeometryMaker.MakeCube3D());

            _ResetPosition = CameraPosition;
            _ResetLookAt = CameraLookAt;
            _ResetUpDirection = CameraUpDirection;
            _ResetDistance = CameraDistance;

            PreviewModel.Transform = _PreviewModelTransfrom;

            _PreviewModelTransfrom.Children.Add(_PreviewModelScale);
            _PreviewModelTransfrom.Children.Add(_PreviewModelRotation);
            _PreviewModelTransfrom.Children.Add(_PreviewModelTranslation);

            _PreviewModelScale.ScaleX = 1.0;
            _PreviewModelScale.ScaleY = 1.0;
            _PreviewModelScale.ScaleZ = 1.0;
            _PreviewModelTranslation.OffsetY = 1;

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

            _ObjHandle = ObjLoader.CreateHandle();
            var result = ObjLoader.LoadAsync(_ObjHandle, PreviewModelPath, loadedHandle =>
            {
                _ObjHandle = loadedHandle;

                InvokeOnUIDispatcher(() =>
                {
                    UpdatePreviewModel(_ObjHandle.Vertices.Select(arg => arg.Normal).ToArray());

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
            CameraDistance = _ResetDistance;
            CameraLookAt = _ResetLookAt;
            CameraUpDirection = _ResetUpDirection;
            CameraPosition = _ResetPosition;
        }

        void RecalculateNormals()
        {
            if (_ObjHandle == null)
            {
                return;
            }

            var normals = new Vector3D[_ObjHandle.Vertices.Length];

            for (int i = 0; i < _ObjHandle.Indices.Length; i += 3)
            {
                int triIndex0 = _ObjHandle.Indices[i + 0];
                int triIndex1 = _ObjHandle.Indices[i + 1];
                int triIndex2 = _ObjHandle.Indices[i + 2];

                Vector3D v1 = _ObjHandle.Vertices[triIndex1].Position - _ObjHandle.Vertices[triIndex0].Position;
                Vector3D v2 = _ObjHandle.Vertices[triIndex2].Position - _ObjHandle.Vertices[triIndex0].Position;
                var normal = Vector3D.CrossProduct(v1, v2);

                normals[triIndex0] = normal;
                normals[triIndex1] = normal;
                normals[triIndex2] = normal;
            }

            UpdatePreviewModel(normals);

            // recolor selected material.
            UpdateSelectedMaterialIndex(SelectedMaterialIndex);
        }

        void UpdatePreviewModel(in Vector3D[] normals)
        {
            PreviewModel.Children.Clear();

            _PreviewMeshMaterials = new MeshMaterial[_ObjHandle.MasterMaterials.Length];

            for (int i = 0; i < _PreviewMeshMaterials.Length; ++i)
            {
                _PreviewMeshMaterials[i] = new MeshMaterial(Brushes.Gray, Brushes.Black);
            }

            var positions = _ObjHandle.Vertices.Select(arg => arg.Position.ToPoint3D()).ToArray();
            var textureCoordinates = _ObjHandle.Vertices.Select(arg => arg.UV.ToPoint()).ToArray();

            if (_ObjHandle.Materials.Length > 0)
            {
                for (int i = 0; i < _ObjHandle.Materials.Length; ++i)
                {
                    var material = _ObjHandle.Materials[i];
                    var masterMaterial = _ObjHandle.MasterMaterials[material.MasterIndex];
                    var indexSegment = new ArraySegment<int>(_ObjHandle.Indices, material.StartIndex, material.IndexCount);
                    var model = MakePreviewModel(positions, normals, textureCoordinates, indexSegment.ToArray(), _PreviewMeshMaterials[material.MasterIndex].Material);
                    PreviewModel.Children.Add(model);
                }
            }
            else
            {
                // default material
                _PreviewMeshMaterials = new MeshMaterial[1];
                _PreviewMeshMaterials[0] = new MeshMaterial(Brushes.Gray, Brushes.Black);

                var model = MakePreviewModel(positions, normals, textureCoordinates, _ObjHandle.Indices, _PreviewMeshMaterials[0].Material);
                PreviewModel.Children.Add(model);
            }

            RaisePropertyChanged(nameof(VertexCount));
            RaisePropertyChanged(nameof(IndexCount));
            RaisePropertyChanged(nameof(MasterMaterialCount));
            RaisePropertyChanged(nameof(MasterMaterialNames));
        }

        GeometryModel3D MakePreviewModel(in Point3D[] positions, in Vector3D[] normals, in Point[] textureCoordinates, int[] indices, Material material)
        {
            Point3D[] posCollection = new Point3D[indices.Length];
            Vector3D[] normalCollection = new Vector3D[indices.Length];
            Point[] textureCoordinateCollection = new Point[indices.Length];

            for (int i = 0; i < indices.Length; ++i)
            {
                int index = indices[i];
                posCollection[i] = positions[index];
                normalCollection[i] = normals[index];
                textureCoordinateCollection[i] = textureCoordinates[index];
            }

            var objMesh = new MeshGeometry3D()
            {
                Positions = new Point3DCollection(posCollection),
                Normals = new Vector3DCollection(normalCollection),
                TextureCoordinates = new PointCollection(textureCoordinateCollection),
            };

            return new GeometryModel3D()
            {
                Geometry = objMesh,
                Material = material,
            };
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

        void UpdateSelectedMaterialIndex(int value)
        {
            _SelectedMaterialIndex = value;
            RaisePropertyChanged(nameof(UpdateSelectedMaterialIndex));

            if(value == -1)
            {
                return;
            }

            var material = _PreviewMeshMaterials[value];
            material.Emissive.Brush = Brushes.Blue;

            foreach (var otherMaterial in _PreviewMeshMaterials.Where(arg => arg != material))
            {
                otherMaterial.Emissive.Brush = Brushes.Black;
            }
        }
    }
}
