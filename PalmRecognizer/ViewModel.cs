using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PalmRecognizer
{
    class ViewModel : ViewModelBase
    {
        private string _palmFilename;
        private ImageSource _palmImage;
        public ImageSource PalmLoadedImage
        {
            get { return _palmImage; }
            set
            {
                _palmImage = value;
                OnPropertyChanged("PalmLoadedImage");
            }
        }

        #region Commands
        private ICommand _loadFileCommand, _recognizePalmCommand, _searchPalmCommand, _logInCommand, _logOutCommand;
        public ICommand LoadFileCommand
        {
            get { return _loadFileCommand ?? (_loadFileCommand = new DelegateCommand(LoadFileCommandExecuted)); }
        }
        public ICommand RecognizePalmCommand
        {
            get { return _recognizePalmCommand ?? (_recognizePalmCommand = new DelegateCommand(RecognizePalmCommandExecuted)); }
        }
        public ICommand SearchPalmCommand
        {
            get { return _searchPalmCommand ?? (_searchPalmCommand = new DelegateCommand(SearchPalmCommandExecuted)); }
        }
        public ICommand LogInCommand
        {
            get { return _logInCommand ?? (_logInCommand = new DelegateCommand(LogInCommandExecuted)); }
        }
        public ICommand LogOutCommand
        {
            get { return _logOutCommand ?? (_logOutCommand = new DelegateCommand(LogOutCommandExecuted)); }
        }

        private void LoadFileCommandExecuted()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
            if (fileDialog.ShowDialog() != true) return;
            _palmFilename = fileDialog.FileName;
            PalmLoadedImage = new BitmapImage(new Uri(fileDialog.FileName));
        }

        private void RecognizePalmCommandExecuted()
        {
            PalmTool tool = new PalmTool(_palmFilename);
        }

        private void SearchPalmCommandExecuted()
        {
            throw new NotImplementedException();
        }

        private void LogInCommandExecuted()
        {
            throw new NotImplementedException();
        }

        private void LogOutCommandExecuted()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
