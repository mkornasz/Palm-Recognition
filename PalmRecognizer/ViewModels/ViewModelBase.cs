namespace PalmRecognizer.ViewModels
{
	using System.ComponentModel;

	public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
