using System.Windows;

namespace PalmRecognizer
{
    /// <summary>
    /// Interaction logic for NewUserWindow.xaml
    /// </summary>
    public partial class NewUserWindow : Window
    {
        public string newUserName { get; private set; }
        public string newUserPassword { get; private set; }

        public NewUserWindow()
        {
            InitializeComponent();
            UserName.Focus();
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            newUserName = UserName.Text;
            newUserPassword = UserPass.Password;
            this.DialogResult = true;
            this.Close();
        }
    }
}
