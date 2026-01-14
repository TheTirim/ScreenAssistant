using System.Globalization;
using System.Windows.Data;
using TabZeroAssistant.Wpf.Resources;

namespace TabZeroAssistant.Wpf;

public sealed class ActionLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var type = value as string;
        return type switch
        {
            "start_timer" => Strings.ActionStartTimerLabel,
            "open_app" => Strings.ActionOpenAppLabel,
            "set_mode" => Strings.ActionSetModeLabel,
            _ => type ?? string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
