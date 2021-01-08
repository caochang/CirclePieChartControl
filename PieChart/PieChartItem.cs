using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace PieChart
{
    /// <summary>
    /// 饼图子项
    /// </summary>
    public class PieChartItem : DependencyObject
    {
        /// <summary>
        /// 值
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// 标识Value的依赖项属性
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(PieChartItem), new PropertyMetadata(0d, ValueOrColorChanged));

        /// <summary>
        /// 颜色
        /// </summary>
        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// 标识Color的依赖项属性
        /// </summary>
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(PieChartItem), new PropertyMetadata(null, ValueOrColorChanged));

        private static void ValueOrColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pci = d as PieChartItem;
            pci.Owner?.OnValueItemUpdated();
        }

        internal IPieChartControl Owner { get; set; }

        internal Geometry Geometry { get; set; }
        internal double Ratio { get; set; }
        internal Point CenterPoint { get; set; }

        /// <summary>
        /// 获取指定类型父级
        /// </summary>
        public static T GetParent<T>(DependencyObject item) where T : class
        {
            var p = LogicalTreeHelper.GetParent(item);
            while (p != null)
            {
                if (p is T)
                    return p as T;
                p = LogicalTreeHelper.GetParent(item);
            }
            return null;
        }
    }

    /// <summary>
    /// 饼图控件接口
    /// </summary>
    public interface IPieChartControl
    {
        /// <summary>
        /// 触发子项更新后动作
        /// </summary>
        void OnValueItemUpdated();
    }
}
