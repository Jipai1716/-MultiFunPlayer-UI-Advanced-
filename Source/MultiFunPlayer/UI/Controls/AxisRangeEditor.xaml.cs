using System.Windows;
using System.Windows.Controls;

namespace MultiFunPlayer.UI.Controls;

/// <summary>
/// Interaction logic for AxisRangeEditor.xaml
/// </summary>
internal sealed partial class AxisRangeEditor : UserControl
{
    // 输出范围的刻度是 0~1，1% 即 0.01
    private const double Step = 0.01;
    private const double MinRange = 0.01;

    public AxisRangeEditor()
    {
        InitializeComponent();
    }

    private void OnLowerDecreaseClick(object sender, RoutedEventArgs e) => NudgeLower(-Step);
    private void OnLowerIncreaseClick(object sender, RoutedEventArgs e) => NudgeLower(Step);
    private void OnUpperDecreaseClick(object sender, RoutedEventArgs e) => NudgeUpper(-Step);
    private void OnUpperIncreaseClick(object sender, RoutedEventArgs e) => NudgeUpper(Step);

    private void NudgeLower(double delta)
    {
        var range = Range;
        if (range == null)
            return;

        var value = Math.Round(range.LowerValue + delta, 4);
        range.LowerValue = Math.Clamp(value, range.Minimum, Math.Max(range.Minimum, range.UpperValue - MinRange));
    }

    private void NudgeUpper(double delta)
    {
        var range = Range;
        if (range == null)
            return;

        var value = Math.Round(range.UpperValue + delta, 4);
        range.UpperValue = Math.Clamp(value, Math.Min(range.Maximum, range.LowerValue + MinRange), range.Maximum);
    }
}
