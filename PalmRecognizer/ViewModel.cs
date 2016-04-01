using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PalmRecognizer
{
    class ViewModel : ViewModelBase
    {
        private string _palmFilename;
        private Visibility _isEdgesDetected;
        private int _cannyParamLow, _cannyParamHigh;
        private bool _isFileLoaded;
        private ImageSource _palmImage, _palmEdgesImage, _palmBlurImage, _palmGrayImage;

        #region Properties
        public ImageSource PalmLoadedImage
        {
            get { return _palmImage; }
            set
            {
                if (_palmImage != value)
                    _palmImage = value;
                OnPropertyChanged("PalmLoadedImage");
            }
        }

        public ImageSource PalmEdgesImage
        {
            get { return _palmEdgesImage; }
            set
            {
                if (_palmEdgesImage != value)
                    _palmEdgesImage = value;
                OnPropertyChanged("PalmEdgesImage");
            }
        }

        public ImageSource PalmBlurImage
        {
            get { return _palmBlurImage; }
            set
            {
                if (_palmBlurImage != value)
                    _palmBlurImage = value;
                OnPropertyChanged("PalmBlurImage");
            }
        }

        public ImageSource PalmGrayImage
        {
            get { return _palmGrayImage; }
            set
            {
                if (_palmGrayImage != value)
                    _palmGrayImage = value;
                OnPropertyChanged("PalmGrayImage");
            }
        }

        public Visibility IsEdgesDetected
        {
            get { return _isEdgesDetected; }
            set
            {
                if (_isEdgesDetected != value)
                    _isEdgesDetected = value;
                OnPropertyChanged("IsEdgesDetected");
                OnPropertyChanged("IsEdgesDetectedToSelection");
            }
        }

        public bool IsEdgesDetectedToSelection
        {
            get
            {
                return IsEdgesDetected == Visibility.Collapsed ? true : false;
            }
        }
        public bool IsFileLoaded
        {
            get { return _isFileLoaded; }
            set
            {
                if (_isFileLoaded != value)
                    _isFileLoaded = value;
                OnPropertyChanged("IsFileLoaded");
            }
        }

        public string CannyParamLow
        {
            get { return _cannyParamLow.ToString(); }
            set
            {
                if (_cannyParamLow.ToString() != value)
                {
                    if (value.Length != 0)
                        _cannyParamLow = int.Parse(value);
                    else
                        _cannyParamLow = 0;
                }
                OnPropertyChanged("CannyParamLow");
            }
        }

        public string CannyParamHigh
        {
            get { return _cannyParamHigh.ToString(); }
            set
            {
                if (_cannyParamHigh.ToString() != value)
                {
                    if (value.Length != 0)
                        _cannyParamHigh = int.Parse(value);
                    else
                        _cannyParamHigh = 0;
                }
                OnPropertyChanged("CannyParamHigh");
            }
        }
        #endregion

        #region Commands
        private ICommand _loadFileCommand, _measurePalmCommand, _recognizePalmCommand, _searchPalmCommand, _logInCommand, _logOutCommand;

        public ICommand LoadFileCommand
        {
            get { return _loadFileCommand ?? (_loadFileCommand = new DelegateCommand(LoadFileCommandExecuted)); }
        }

        public ICommand MeasurePalmCommand
        {
            get { return _measurePalmCommand ?? (_measurePalmCommand = new DelegateCommand(MeasurePalmCommandExecuted)); }
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
            IsEdgesDetected = Visibility.Collapsed;
            IsFileLoaded = true;
            _palmFilename = fileDialog.FileName;
            PalmLoadedImage = new BitmapImage(new Uri(fileDialog.FileName));
        }

        private void RecognizePalmCommandExecuted()
        {
            PalmTool tool = new PalmTool(_palmFilename, _cannyParamLow, _cannyParamHigh);
            IsEdgesDetected = Visibility.Visible;
            PalmEdgesImage = ConvertFromBitmapToBitmapSource(tool.GetEdgesPalmBitmap);
            PalmGrayImage = ConvertFromBitmapToBitmapSource(tool.GetGrayPalmBitmap);
            PalmBlurImage = ConvertFromBitmapToBitmapSource(tool.GetBlurPalmBitmap);
        }

        private void MeasurePalmCommandExecuted()
        {
            throw new NotImplementedException();
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

        public ViewModel()
        {
            _isEdgesDetected = Visibility.Collapsed;
            _isFileLoaded = false;
            _cannyParamHigh = 250;
            _cannyParamLow = 100;
        }

        private BitmapSource ConvertFromBitmapToBitmapSource(Bitmap bmp)
        {
            MemoryStream stream = new MemoryStream();
            BitmapSource image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap
                (
                bmp.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
                );
            return image;
        }

        private Bitmap ConvertFromBitmapSourceToBitmap(BitmapSource bmp)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            BitmapFrame frame = BitmapFrame.Create(PalmLoadedImage as BitmapSource);
            encoder.Frames.Add(frame);
            MemoryStream stream = new MemoryStream();
            encoder.Save(stream);
            return new Bitmap(stream);
        }
    }
}
