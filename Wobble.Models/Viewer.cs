using System.ComponentModel;

namespace Wobble.Models
{
    public class Viewer :INotifyPropertyChanged
    {
        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}