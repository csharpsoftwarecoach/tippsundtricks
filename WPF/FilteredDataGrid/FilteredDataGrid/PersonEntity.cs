namespace FilteredDataGridProject
{
    public class PersonEntity : ModelBase
    {
        private string _nachname;

        public string Nachname
        {
            get { return _nachname; }
            set
            {
                if (_nachname != value)
                {
                    _nachname = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _vorname;

        public string Vorname
        {
            get { return _vorname; }
            set 
            {
                if (_vorname != value)
                {
                    _vorname = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
