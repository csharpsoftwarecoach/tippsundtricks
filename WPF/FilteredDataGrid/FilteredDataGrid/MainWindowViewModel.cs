using System.Collections.ObjectModel;

namespace FilteredDataGridProject
{
    public class MainWindowViewModel : ModelBase
    {

        private PersonEntity _selectedItemPerson;

        public PersonEntity SelectedItemPerson
        {
            get { return _selectedItemPerson; }
            set
            {
                if (_selectedItemPerson != value)
                {
                    _selectedItemPerson = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<PersonEntity> _personen = new ObservableCollection<PersonEntity>();

        public ObservableCollection<PersonEntity> Personen
        {
            get { return _personen; }
            set
            {
                if (_personen != value)
                {
                    _personen = value;
                    OnPropertyChanged();
                }
            }
        }


        public MainWindowViewModel()
        {
            Personen.Add(new PersonEntity() { Vorname = "Anton", Nachname = "Huber" });
            Personen.Add(new PersonEntity() { Vorname = "Anna", Nachname = "Bärehns" });
        }
    }
}
