using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TabZeroAssistant.Wpf.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ChatMessageViewModel> ChatMessages { get; } = [];
    public ObservableCollection<SuggestionViewModel> Suggestions { get; } = [];
    public ObservableCollection<MemoryCandidateViewModel> MemoryCandidates { get; } = [];

    private string _inputText = string.Empty;
    private string _statusMessage = string.Empty;
    private Visibility _memoryCandidatesVisible = Visibility.Collapsed;

    public string InputText
    {
        get => _inputText;
        set => SetField(ref _inputText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public Visibility MemoryCandidatesVisible
    {
        get => _memoryCandidatesVisible;
        set => SetField(ref _memoryCandidatesVisible, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
