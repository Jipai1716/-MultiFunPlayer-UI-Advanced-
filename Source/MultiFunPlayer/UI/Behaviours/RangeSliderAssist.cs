using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace MultiFunPlayer.UI.Behaviours;

/// <summary>
/// 让 MahApps 的 RangeSlider（上下限双柄）支持"相对拖动 + 灵敏度 + 边界吸附"，
/// 整个控件都可以按下，自动选择距离指针最近的柄来拖动；对 VR 控制器更友好。
/// </summary>
internal static class RangeSliderAssist
{
    public static readonly DependencyProperty RelativeDragProperty =
        DependencyProperty.RegisterAttached("RelativeDrag",
            typeof(bool), typeof(RangeSliderAssist),
                new PropertyMetadata(false, OnRelativeDragPropertyChanged));

    public static readonly DependencyProperty DragSensitivityProperty =
        DependencyProperty.RegisterAttached("DragSensitivity",
            typeof(double), typeof(RangeSliderAssist),
                new PropertyMetadata(0.5));

    public static readonly DependencyProperty SnapToBoundsProperty =
        DependencyProperty.RegisterAttached("SnapToBounds",
            typeof(bool), typeof(RangeSliderAssist),
                new PropertyMetadata(true));

    private static readonly DependencyProperty DragStateProperty =
        DependencyProperty.RegisterAttached("DragState",
            typeof(RangeSliderDragState), typeof(RangeSliderAssist),
                new PropertyMetadata(null));

    public static bool GetRelativeDrag(DependencyObject dp)
        => (bool)dp.GetValue(RelativeDragProperty);

    public static void SetRelativeDrag(DependencyObject dp, bool value)
        => dp.SetValue(RelativeDragProperty, value);

    public static double GetDragSensitivity(DependencyObject dp)
        => (double)dp.GetValue(DragSensitivityProperty);

    public static void SetDragSensitivity(DependencyObject dp, double value)
        => dp.SetValue(DragSensitivityProperty, value);

    public static bool GetSnapToBounds(DependencyObject dp)
        => (bool)dp.GetValue(SnapToBoundsProperty);

    public static void SetSnapToBounds(DependencyObject dp, bool value)
        => dp.SetValue(SnapToBoundsProperty, value);

    private static void OnRelativeDragPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
    {
        if (dp is not RangeSlider slider)
            return;

        if ((bool)e.NewValue)
            Attach(slider);
        else
            Detach(slider);
    }

    private static void Attach(RangeSlider slider)
    {
        if (slider.GetValue(DragStateProperty) is RangeSliderDragState)
            return;

        slider.SetValue(DragStateProperty, new RangeSliderDragState());
        slider.PreviewMouseLeftButtonDown += HandlePreviewMouseLeftButtonDown;
        slider.PreviewMouseMove += HandlePreviewMouseMove;
        slider.PreviewMouseLeftButtonUp += HandlePreviewMouseLeftButtonUp;
        slider.LostMouseCapture += HandleLostMouseCapture;
    }

    private static void Detach(RangeSlider slider)
    {
        slider.ClearValue(DragStateProperty);
        slider.PreviewMouseLeftButtonDown -= HandlePreviewMouseLeftButtonDown;
        slider.PreviewMouseMove -= HandlePreviewMouseMove;
        slider.PreviewMouseLeftButtonUp -= HandlePreviewMouseLeftButtonUp;
        slider.LostMouseCapture -= HandleLostMouseCapture;
    }

    private static RangeSliderDragState GetState(RangeSlider slider)
        => slider.GetValue(DragStateProperty) as RangeSliderDragState;

    private static double GetDragLength(RangeSlider slider)
        => Math.Max(1, slider.ActualWidth - 34);

    private static void HandlePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not RangeSlider slider || !slider.IsEnabled)
            return;
        if (GetState(slider) is not { } state)
            return;

        var range = slider.Maximum - slider.Minimum;
        var length = GetDragLength(slider);
        var x = e.GetPosition(slider).X;

        if (range <= 0)
            return;

        var lowerX = (slider.LowerValue - slider.Minimum) / range * length;
        var upperX = (slider.UpperValue - slider.Minimum) / range * length;

        state.IsDragging = true;
        state.IsLower = Math.Abs(x - lowerX) <= Math.Abs(x - upperX);
        state.StartPosition = e.GetPosition(slider);
        state.StartValue = state.IsLower ? slider.LowerValue : slider.UpperValue;
        state.DragLength = length;
        state.SnappedValue = double.NaN;

        slider.CaptureMouse();
        e.Handled = true;
    }

    private static void HandlePreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not RangeSlider slider)
            return;
        if (GetState(slider) is not { IsDragging: true } state)
            return;
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            state.IsDragging = false;
            return;
        }

        var range = slider.Maximum - slider.Minimum;
        if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range))
            return;

        var delta = e.GetPosition(slider).X - state.StartPosition.X;
        var value = state.StartValue + delta / state.DragLength * range * GetDragSensitivity(slider);
        value = SnapToBounds(slider, state, value);

        if (state.IsLower)
            slider.LowerValue = Math.Clamp(value, slider.Minimum, slider.UpperValue);
        else
            slider.UpperValue = Math.Clamp(value, slider.LowerValue, slider.Maximum);

        e.Handled = true;
    }

    private static void HandlePreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not RangeSlider slider)
            return;
        if (GetState(slider) is not { } state || !state.IsDragging)
            return;

        state.IsDragging = false;
        slider.ReleaseMouseCapture();
        e.Handled = true;
    }

    private static void HandleLostMouseCapture(object sender, MouseEventArgs e)
    {
        if (sender is not RangeSlider slider)
            return;
        if (GetState(slider) is { } state)
            state.IsDragging = false;
    }

    private static double SnapToBounds(RangeSlider slider, RangeSliderDragState state, double value)
    {
        if (!GetSnapToBounds(slider))
            return value;

        var range = slider.Maximum - slider.Minimum;
        if (range <= 0)
            return value;

        var threshold = range * 0.01;
        if (!double.IsNaN(state.SnappedValue))
        {
            if (Math.Abs(value - state.SnappedValue) < threshold * 2.2)
                return state.SnappedValue;

            state.SnappedValue = double.NaN;
        }

        var midpoint = slider.Minimum + range / 2;

        if (Math.Abs(value - slider.Minimum) <= threshold)
        {
            state.SnappedValue = slider.Minimum;
            return slider.Minimum;
        }

        if (Math.Abs(value - slider.Maximum) <= threshold)
        {
            state.SnappedValue = slider.Maximum;
            return slider.Maximum;
        }

        if (Math.Abs(value - midpoint) <= threshold)
        {
            state.SnappedValue = midpoint;
            return midpoint;
        }

        if (slider.Minimum < 0 && slider.Maximum > 0 && Math.Abs(value) <= threshold)
        {
            state.SnappedValue = 0;
            return 0;
        }

        return value;
    }

    private sealed class RangeSliderDragState
    {
        public bool IsDragging { get; set; }
        public bool IsLower { get; set; }
        public Point StartPosition { get; set; }
        public double StartValue { get; set; }
        public double DragLength { get; set; } = 1;
        public double SnappedValue { get; set; } = double.NaN;
    }
}
