using Presentation.Interfaces;
using System.ComponentModel;

namespace ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged, IViewModel
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;

        public string DisplayName { get; set; } = "Die Verbindung zwischen View und ViewModel hat geklappt.";
    }
}
