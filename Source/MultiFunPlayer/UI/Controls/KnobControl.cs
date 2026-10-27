using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiFunPlayer.UI.Controls;

/// <summary>
/// 旋钮控件（VR 优化）：整个圆盘都可以按下上下拖动来旋转，
/// 继承自 Slider，因此 Value/ValueChanged 与普通滑条完全兼容。
/// </summary>
internal sealed class KnobControl : Slider
{
    public static readonly DependencyProperty SensitivityProperty =
        DependencyProperty.Register(nameof(Sensitivity), typeof(double), typeof(KnobControl),
            new PropertyMetadata(0.25));

    public static readonly DependencyProperty SnapValueProperty =
        DependencyProperty.Register(nameof(SnapValue), typeof(double), typeof(KnobControl),
            new PropertyMetadata(double.NaN));

    private const double StartAngle = 135;
    private const double SweepAngle = 270;

    private bool _isDragging;
    private Point _startPosition;
    private double _startValue;
    private double _snappedValue = double.NaN;

    /// <summary>拖动 1 像素改变的范围比例（0.25 = 拖过整个控件高度改变 25% 量程）</summary>
    public double Sensitivity
    {
        get => (double)GetValue(SensitivityProperty);
        set => SetValue(SensitivityProperty, value);
    }

    /// <summary>拖动时靠近该值会自动吸附（例如缩放复位到 1.0）</summary>
    public double SnapValue
    {
        get => (double)GetValue(SnapValueProperty);
        set => SetValue(SnapValueProperty, value);
    }

    public KnobControl()
    {
        Orientation = Orientation.Vertical;
        SmallChange = 0.01;
        IsMoveToPointEnabled = false;

        PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        PreviewMouseMove += OnPreviewMouseMove;
        PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
        LostMouseCapture += (_, _) => _isDragging = false;
        MouseWheel += OnMouseWheel;
        MouseDoubleClick += OnMouseDoubleClick;
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (double.IsNaN(SnapValue))
            return;

        Value = Math.Clamp(SnapValue, Minimum, Maximum);
        e.Handled = true;
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        var width = ActualWidth;
        var height = ActualHeight;
        if (width <= 0 || height <= 0)
            return;

        var size = Math.Min(width, height);
        var thickness = Math.Max(8, size * 0.09);
        var radius = size / 2 - thickness / 2 - 2;
        if (radius <= 2)
            return;

        var center = new Point(width / 2, height / 2);
        var range = Maximum - Minimum;
        var progress = range > 0 ? (Value - Minimum) / range : 0;
        progress = Math.Clamp(progress, 0, 1);

        var trackBrush = TryFindResource("VrSliderInactiveTrackBrush") as Brush ?? Brushes.Gray;
        var valueBrush = TryFindResource("PrimaryHueMidBrush") as Brush ?? Brushes.DeepSkyBlue;

        var trackPen = new Pen(trackBrush, thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var valuePen = new Pen(valueBrush, thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        trackPen.Freeze();
        valuePen.Freeze();

        drawingContext.DrawGeometry(null, trackPen, CreateArcGeometry(center, radius, StartAngle, StartAngle + SweepAngle));

        if (progress > 0.001)
            drawingContext.DrawGeometry(null, valuePen, CreateArcGeometry(center, radius, StartAngle, StartAngle + SweepAngle * progress));

        // 旋钮盘面
        var capRadius = radius - thickness / 2 - 4;
        var capBrush = new RadialGradientBrush(
            Color.FromRgb(0x3D, 0x4A, 0x58),
            Color.FromRgb(0x1D, 0x23, 0x2B))
        {
            GradientOrigin = new Point(0.35, 0.28),
            Center = new Point(0.5, 0.5),
            RadiusX = 0.75,
            RadiusY = 0.75
        };
        capBrush.Freeze();
        drawingContext.DrawEllipse(capBrush, new Pen(new SolidColorBrush(Color.FromRgb(0x46, 0x54, 0x5F)), 2), center, capRadius, capRadius);

        // 指针
        var angle = (StartAngle + SweepAngle * progress) * Math.PI / 180;
        var inner = new Point(center.X + Math.Cos(angle) * capRadius * 0.35, center.Y + Math.Sin(angle) * capRadius * 0.35);
        var outer = new Point(center.X + Math.Cos(angle) * capRadius * 0.82, center.Y + Math.Sin(angle) * capRadius * 0.82);
        var indicatorPen = new Pen(valueBrush, Math.Max(3, thickness * 0.42)) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        indicatorPen.Freeze();
        drawingContext.DrawLine(indicatorPen, inner, outer);
    }

    private static Geometry CreateArcGeometry(Point center, double radius, double startAngle, double endAngle)
    {
        var start = PointOnCircle(center, radius, startAngle);
        var end = PointOnCircle(center, radius, endAngle);

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(start, false, false);
            context.ArcTo(end, new Size(radius, radius), 0,
                endAngle - startAngle > 180, SweepDirection.Clockwise, true, false);
        }

        geometry.Freeze();
        return geometry;
    }

    private static Point PointOnCircle(Point center, double radius, double angle)
    {
        var radians = angle * Math.PI / 180;
        return new Point(center.X + Math.Cos(radians) * radius, center.Y + Math.Sin(radians) * radius);
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled)
            return;

        _isDragging = true;
        _startPosition = e.GetPosition(this);
        _startValue = Value;
        _snappedValue = double.NaN;

        CaptureMouse();
        e.Handled = true;
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging)
            return;
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            _isDragging = false;
            return;
        }

        var range = Maximum - Minimum;
        if (range <= 0)
            return;

        var delta = _startPosition.Y - e.GetPosition(this).Y;
        var value = _startValue + delta / Math.Max(1, ActualHeight) * range * Sensitivity;
        value = Snap(value);
        Value = Math.Clamp(value, Minimum, Maximum);

        e.Handled = true;
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging)
            return;

        _isDragging = false;
        ReleaseMouseCapture();
        e.Handled = true;
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var step = SmallChange <= 0 ? (Maximum - Minimum) / 100 : SmallChange;
        Value = Math.Clamp(Value + Math.Sign(e.Delta) * step, Minimum, Maximum);
        e.Handled = true;
    }

    private double Snap(double value)
    {
        var range = Maximum - Minimum;
        if (range <= 0)
            return value;

        var threshold = range * 0.015;
        if (!double.IsNaN(_snappedValue))
        {
            if (Math.Abs(value - _snappedValue) < threshold * 2.2)
                return _snappedValue;

            _snappedValue = double.NaN;
        }

        foreach (var target in new[] { Minimum, Maximum, SnapValue })
        {
            if (double.IsNaN(target))
                continue;
            if (target < Minimum || target > Maximum)
                continue;
            if (Math.Abs(value - target) <= threshold)
            {
                _snappedValue = target;
                return target;
            }
        }

        return value;
    }
}
