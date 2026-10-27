using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiFunPlayer.UI.Behaviours;

internal static class SliderAssist
{
    public static readonly DependencyProperty DefaultValueOnDoubleClickProperty =
        DependencyProperty.RegisterAttached("DefaultValueOnDoubleClick",
            typeof(double), typeof(SliderAssist),
                new PropertyMetadata(double.NaN, OnDefaultValueOnDoubleClickPropertyChanged));

    public static readonly DependencyProperty RelativeDragProperty =
        DependencyProperty.RegisterAttached("RelativeDrag",
            typeof(bool), typeof(SliderAssist),
                new PropertyMetadata(false, OnRelativeDragPropertyChanged));

    public static readonly DependencyProperty DragSensitivityProperty =
        DependencyProperty.RegisterAttached("DragSensitivity",
            typeof(double), typeof(SliderAssist),
                new PropertyMetadata(0.5));

    public static readonly DependencyProperty SnapToBoundsProperty =
        DependencyProperty.RegisterAttached("SnapToBounds",
            typeof(bool), typeof(SliderAssist),
                new PropertyMetadata(true));

    private static readonly DependencyProperty DragStateProperty =
        DependencyProperty.RegisterAttached("DragState",
            typeof(SliderDragState), typeof(SliderAssist),
                new PropertyMetadata(null));

    public static double GetDefaultValueOnDoubleClick(DependencyObject dp)
        => (double)dp.GetValue(DefaultValueOnDoubleClickProperty);

    public static void SetDefaultValueOnDoubleClick(DependencyObject dp, double value)
        => dp.SetValue(DefaultValueOnDoubleClickProperty, value);

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

    private static void OnDefaultValueOnDoubleClickPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
    {
        if (dp is not Slider slider)
            return;

        slider.MouseDoubleClick -= HandleMouseDoubleClick;
        slider.MouseDoubleClick += HandleMouseDoubleClick;
    }

    private static void HandleMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Slider slider)
            return;

        slider.Value = GetDefaultValueOnDoubleClick(slider);
    }

    private static void OnRelativeDragPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
    {
        if (dp is not Slider slider)
            return;

        if ((bool)e.NewValue)
            AttachRelativeDrag(slider);
        else
            DetachRelativeDrag(slider);
    }

    private static void AttachRelativeDrag(Slider slider)
    {
        if (slider.GetValue(DragStateProperty) is SliderDragState)
            return;

        slider.SetValue(DragStateProperty, new SliderDragState());
        slider.PreviewMouseLeftButtonDown += HandlePreviewMouseLeftButtonDown;
        slider.PreviewMouseMove += HandlePreviewMouseMove;
        slider.PreviewMouseLeftButtonUp += HandlePreviewMouseLeftButtonUp;
        slider.LostMouseCapture += HandleLostMouseCapture;
    }

    private static void DetachRelativeDrag(Slider slider)
    {
        slider.ClearValue(DragStateProperty);
        slider.PreviewMouseLeftButtonDown -= HandlePreviewMouseLeftButtonDown;
        slider.PreviewMouseMove -= HandlePreviewMouseMove;
        slider.PreviewMouseLeftButtonUp -= HandlePreviewMouseLeftButtonUp;
        slider.LostMouseCapture -= HandleLostMouseCapture;
    }

    private static SliderDragState GetState(Slider slider)
        => slider.GetValue(DragStateProperty) as SliderDragState;

    private static double GetDragLength(Slider slider)
    {
        var length = slider.Orientation == Orientation.Horizontal ? slider.ActualWidth : slider.ActualHeight;
        return Math.Max(1, length - 32);
    }

    private static double GetDragDelta(Slider slider, Point start, Point current)
        => slider.Orientation == Orientation.Horizontal ? current.X - start.X : start.Y - current.Y;

    private static void HandlePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Slider slider || !slider.IsEnabled)
            return;
        if (GetState(slider) is not { } state)
            return;

        state.IsDragging = true;
        state.StartPosition = e.GetPosition(slider);
        state.StartValue = slider.Value;
        state.DragLength = GetDragLength(slider);
        state.SnappedValue = double.NaN;

        slider.CaptureMouse();
        e.Handled = true;
    }

    private static void HandlePreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not Slider slider)
            return;
        if (GetState(slider) is not { IsDragging: true } state)
            return;
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            state.IsDragging = false;
            return;
        }

        var delta = GetDragDelta(slider, state.StartPosition, e.GetPosition(slider));
        var range = slider.Maximum - slider.Minimum;
        if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range))
            return;

        var value = state.StartValue + delta / state.DragLength * range * GetDragSensitivity(slider);
        value = SnapToBounds(slider, state, value);
        value = Math.Clamp(value, slider.Minimum, slider.Maximum);

        if (Math.Abs(value - slider.Value) > double.Epsilon)
            slider.Value = value;

        e.Handled = true;
    }

    private static void HandlePreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Slider slider)
            return;
        if (GetState(slider) is not { } state || !state.IsDragging)
            return;

        state.IsDragging = false;
        slider.ReleaseMouseCapture();
        e.Handled = true;
    }

    private static void HandleLostMouseCapture(object sender, MouseEventArgs e)
    {
        if (sender is not Slider slider)
            return;
        if (GetState(slider) is { } state)
            state.IsDragging = false;
    }

    private static double SnapToBounds(Slider slider, SliderDragState state, double value)
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

        if (slider.Minimum < 0 && slider.Maximum > 0 && Math.Abs(value) <= threshold)
        {
            state.SnappedValue = 0;
            return 0;
        }

        return value;
    }

    private sealed class SliderDragState
    {
        public bool IsDragging { get; set; }
        public Point StartPosition { get; set; }
        public double StartValue { get; set; }
        public double DragLength { get; set; } = 1;
        public double SnappedValue { get; set; } = double.NaN;
    }
}
