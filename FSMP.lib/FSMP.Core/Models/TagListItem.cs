using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FSMP.Core.Models;

public class TagListItem : INotifyPropertyChanged
{
    private bool _isPendingChange;

    public Tags Tag { get; set; } = null!;
    public bool IsSaved { get; set; }

    public bool IsPendingChange
    {
        get => _isPendingChange;
        set
        {
            if (_isPendingChange != value)
            {
                _isPendingChange = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
