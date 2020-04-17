using PreviewTest.Utilities;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PreviewTest.Controls
{
    /// <summary>
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:PreviewTest.Controls"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:PreviewTest.Controls;assembly=PreviewTest.Controls"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:ObjViewer/>
    ///
    /// </summary>
    public class ObjViewer : ContentControl
    {
        public GeometryModel3D PreviewModel
        {
            get => (GeometryModel3D)GetValue(PreviewModelProperty);
            set => SetValue(PreviewModelProperty, value);
        }
        public static readonly DependencyProperty PreviewModelProperty =
            DependencyProperty.Register(nameof(PreviewModel), typeof(GeometryModel3D), typeof(ObjViewer), new FrameworkPropertyMetadata(null, PreviewModelPropertyChanged));

        public PerspectiveCamera Camera
        {
            get => (PerspectiveCamera)GetValue(CameraProperty);
            set => SetValue(CameraProperty, value);
        }
        public static readonly DependencyProperty CameraProperty =
            DependencyProperty.Register(nameof(Camera), typeof(PerspectiveCamera), typeof(ObjViewer), new FrameworkPropertyMetadata(null, CameraPropertyChanged));

        public Vector3D LookAt
        {
            get => (Vector3D)GetValue(LookAtProperty);
            set => SetValue(LookAtProperty, value);
        }
        public static readonly DependencyProperty LookAtProperty =
            DependencyProperty.Register(nameof(LookAt), typeof(Vector3D), typeof(ObjViewer), new FrameworkPropertyMetadata(new Vector3D(0, 0, 0), LookAtPropertyChanged));

        static ObjViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ObjViewer), new FrameworkPropertyMetadata(typeof(ObjViewer)));
        }

        Viewport3D _Viewport = null;
        DirectionalLight _Light = null;
        PerspectiveCamera _Camera = null;
        TranslateTransform3D _LookAtTransform = null;

        public ObjViewer()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _Viewport = GetTemplateChild("__Viewport3D__") as Viewport3D;

            _Light = new DirectionalLight(Colors.White, new Vector3D(0, 0, 1));
            _Camera = new PerspectiveCamera(new Point3D(0, 0, 5), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 45);

            CreateGridLines();
            CreateLookAtSymbol();

            _Viewport.Camera = _Camera;
            _Viewport.Children.Add(new ModelVisual3D() { Content = _Light });
        }

        void CreateLookAtSymbol()
        {
            var xAxis = GeometryMaker.MakeLine3D(new Vector3D(0, 0, 0), new Vector3D(1, 0, 0), Brushes.Red, 0.02);
            var yAxis = GeometryMaker.MakeLine3D(new Vector3D(0, 0, 0), new Vector3D(0, 1, 0), Brushes.Green, 0.02);
            var zAxis = GeometryMaker.MakeLine3D(new Vector3D(0, 0, 0), new Vector3D(0, 0, 1), Brushes.Black, 0.02);

            _LookAtTransform = new TranslateTransform3D();

            _Viewport.Children.Add(new ModelVisual3D() { Content = xAxis, Transform = _LookAtTransform });
            _Viewport.Children.Add(new ModelVisual3D() { Content = yAxis, Transform = _LookAtTransform });
            _Viewport.Children.Add(new ModelVisual3D() { Content = zAxis, Transform = _LookAtTransform });
        }

        void CreateGridLines()
        {
            for (int i = -50; i < 50; ++i)
            {
                double x = i;
                var line = GeometryMaker.MakeLine3D(new Vector3D(x, 0, -50), new Vector3D(x, 0, 50), Brushes.Gray);
                _Viewport.Children.Add(new ModelVisual3D() { Content = line });
            }

            for (int i = -50; i < 50; ++i)
            {
                double z = i;
                var line = GeometryMaker.MakeLine3D(new Vector3D(-50, 0, z), new Vector3D(50, 0, z), Brushes.Gray);
                _Viewport.Children.Add(new ModelVisual3D() { Content = line });
            }

            // squre edge            

            {   //left
                var line = GeometryMaker.MakeLine3D(new Vector3D(50, 0, -50), new Vector3D(50, 0, 50), Brushes.Gray);
                _Viewport.Children.Add(new ModelVisual3D() { Content = line });
            }

            {   //top
                var line = GeometryMaker.MakeLine3D(new Vector3D(-50, 0, 50), new Vector3D(50, 0, 50), Brushes.Gray);
                _Viewport.Children.Add(new ModelVisual3D() { Content = line });
            }
        }

        static void PreviewModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ObjViewer;

            if (e.OldValue != null)
            {
                var model = viewer._Viewport.Children.OfType<ModelVisual3D>().First(arg => arg.Content == e.OldValue);
                viewer._Viewport.Children.Remove(model);
            }
            if (e.NewValue != null)
            {
                viewer._Viewport.Children.Add(new ModelVisual3D() { Content = e.NewValue as Model3D });
            }
        }

        static void CameraPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ObjViewer;

            viewer._Viewport.Camera = e.NewValue as PerspectiveCamera;
        }

        static void LookAtPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = d as ObjViewer;
            if (viewer._LookAtTransform != null)
            {
                viewer._LookAtTransform.OffsetX = viewer.LookAt.X;
                viewer._LookAtTransform.OffsetY = viewer.LookAt.Y;
                viewer._LookAtTransform.OffsetZ = viewer.LookAt.Z;
            }
        }
    }
}
