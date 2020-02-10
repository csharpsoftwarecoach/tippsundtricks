using System.Windows;

namespace FilteredDataGridProject
{
    /// <summary>
    /// Interaktionslogik für MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();

            // HINWEIS: Diese Zeile dient zur Vereinfachung des Beispiels.
            //          Der saubere Weg ist es, Views und ViewModel in eigene Assemblies zu packen, die keine Referenz zueinander haben und die Verbindung
            //          über einen IoC-Container herzustellen.
            DataContext = new MainWindowViewModel();
        }
    }
}
