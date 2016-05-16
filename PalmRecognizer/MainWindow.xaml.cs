using System.Windows;

namespace PalmRecognizer
{
	using PalmRecognizer.ViewModels;

	/// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }
    }
}
