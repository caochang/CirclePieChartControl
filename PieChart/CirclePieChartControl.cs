using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace PieChart
{
    /// <summary>
    /// 圆形饼图控件
    /// </summary>
    [ContentProperty("Values")]
    public class CirclePieChartControl : Control, IPieChartControl
    {
        /// <summary>
        /// 空白半径
        /// </summary>
        public double EmptyRadius
        {
            get { return (double)GetValue(EmptyRadiusProperty); }
            set { SetValue(EmptyRadiusProperty, value); }
        }

        /// <summary>
        /// 标识EmptyRadius的依赖项属性
        /// </summary>
        public static readonly DependencyProperty EmptyRadiusProperty =
            DependencyProperty.Register("EmptyRadius", typeof(double), typeof(CirclePieChartControl), new PropertyMetadata(ReRenderPropertyChanged));

        /// <summary>
        /// 是否显示比例
        /// </summary>
        public bool ShowRatio
        {
            get { return (bool)GetValue(ShowRatioProperty); }
            set { SetValue(ShowRatioProperty, value); }
        }

        /// <summary>
        /// 标识ShowRatio的依赖项属性
        /// </summary>
        public static readonly DependencyProperty ShowRatioProperty =
            DependencyProperty.Register("ShowRatio", typeof(bool), typeof(CirclePieChartControl), new PropertyMetadata(true, ReRenderPropertyChanged));

        private static void ReRenderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CirclePieChartControl)?.InvalidateVisual();
        }

        /// <summary>
        /// 子项
        /// </summary>
        public ObservableCollection<PieChartItem> Values { get; } = new ObservableCollection<PieChartItem>();

        public void OnValueItemUpdated()
        {
            InvalidateVisual();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Values.CollectionChanged += (o, a) => InvalidateVisual();
        }

        private void GenerateRatios()
        {
            var sum = Values.Sum((item) => item.Value);
            foreach (var item in Values)
            {
                item.Ratio = item.Value / sum;
                item.Owner = this;
            }
        }

        private void GenerateGeometrys()
        {
            if (ActualWidth == 0 || ActualHeight == 0)
                return;

            var rx = (ActualWidth - Padding.Left - Padding.Right) / 2;
            var ry = (ActualHeight - Padding.Top - Padding.Bottom) / 2;
            var cp = new Point() { X = Padding.Left + rx, Y = Padding.Top + ry };
            var r = Math.Min(rx, ry);

            var zeroPoint = new Point(0, 0 - r);
            var curAngle = 0d;
            var curPoint = zeroPoint;
            var emptyRate = double.NaN;
            if (EmptyRadius != double.NaN && EmptyRadius < r)
                emptyRate = EmptyRadius / r;
            foreach (var item in Values)
            {
                var ps = curPoint;
                var angle = item.Ratio * 360;
                var half = angle / 2;
                var matrix = new Matrix();
                curAngle += half;
                matrix.Rotate(curAngle);
                curAngle += half;
                var pm = zeroPoint * matrix;
                matrix.Rotate(half);
                var pe = zeroPoint * matrix;
                curPoint = pe;
                ps = new Point(ps.X + cp.X, ps.Y + cp.Y);
                pm = new Point(pm.X + cp.X, pm.Y + cp.Y);
                pe = new Point(pe.X + cp.X, pe.Y + cp.Y);
                if (double.IsNaN(emptyRate))
                    item.Geometry = Geometry.Parse($"M{ps.X},{ps.Y} A{r},{r} {half} 0 1 {pm.X},{pm.Y} A{r},{r} {half} 0 1 {pe.X},{pe.Y} L{cp.X},{cp.Y} L{ps.X},{ps.Y}");
                else
                {
                    var es = new Point(cp.X + (pe.X - cp.X) * emptyRate, cp.Y + (pe.Y - cp.Y) * emptyRate);
                    var em = new Point(cp.X + (pm.X - cp.X) * emptyRate, cp.Y + (pm.Y - cp.Y) * emptyRate);
                    var ee = new Point(cp.X + (ps.X - cp.X) * emptyRate, cp.Y + (ps.Y - cp.Y) * emptyRate);
                    item.Geometry = Geometry.Parse($"M{ps.X},{ps.Y} A{r},{r} {half} 0 1 {pm.X},{pm.Y} A{r},{r} {half} 0 1 {pe.X},{pe.Y} L{es.X},{es.Y} A{EmptyRadius},{EmptyRadius} {half} 0 0 {em.X},{em.Y} A{EmptyRadius},{EmptyRadius} {half} 0 0 {ee.X},{ee.Y} L{ps.X},{ps.Y}");
                }

                if (ShowRatio)
                    item.CenterPoint = new Point(cp.X + (pm.X - cp.X) * 2 / 3, cp.Y + (pm.Y - cp.Y) * 2 / 3);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            GenerateRatios();
            GenerateGeometrys();

            foreach (var item in Values)
            {
                if (item.Ratio == 0)
                    continue;

                drawingContext.DrawGeometry(item.Color, null, item.Geometry);
                if (ShowRatio)
                {
                    var txt = $"{item.Ratio * 100:F0}%";
                    var ft = new FormattedText(txt, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("黑体"), 20, Brushes.Black);
                    var vector = new Vector((0 - ft.Width) / 2, (0 - ft.Height) / 2);
                    drawingContext.DrawText(ft, item.CenterPoint + vector);
                }
            }
        }
    }
}
