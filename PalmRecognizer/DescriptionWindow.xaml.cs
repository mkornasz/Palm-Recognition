using System.Windows;

namespace PalmRecognizer
{
    /// <summary>
    /// Interaction logic for DescriptionWindow.xaml
    /// </summary>
    public partial class DescriptionWindow : Window
    {
        public string Description { get; private set; }

        public DescriptionWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Description = DescriptionTextBox.Text;
            this.DialogResult = true;
            this.Close();
        }
    }
}
