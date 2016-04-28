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
        #region Variables
        private Visibility _isEdgesDetected;
        private int _cannyParamLow, _cannyParamHigh;
        private double angle = 0.0;
        private bool _isFileLoaded, _isImageReadyForRotation;
        private string _palmFilename;
        private ImageSource _palmImage, _palmEdgesImage, _palmBlurImage, _palmGrayImage;
        private Bitmap _palmImageBitmap, _palmRotatedImageBitmap;
        #endregion 

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
        private ICommand _mouseWheelCommand, _loadFileCommand, _measurePalmCommand, _recognizePalmCommand, _searchPalmCommand, _addPalmToBaseCommand, _logInCommand, _logOutCommand;

        public ICommand MouseWheelCommand
        {
            get { return _mouseWheelCommand ?? (_mouseWheelCommand = new DelegateCommand(MouseWheelCommandExecuted)); }
        }

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

        public ICommand AddPalmToBaseCommand
        {
            get { return _addPalmToBaseCommand ?? (_addPalmToBaseCommand = new DelegateCommand(AddPalmToBaseCommandExecuted)); }
        }

        public ICommand LogInCommand
        {
            get { return _logInCommand ?? (_logInCommand = new DelegateCommand(LogInCommandExecuted)); }
        }

        public ICommand LogOutCommand
        {
            get { return _logOutCommand ?? (_logOutCommand = new DelegateCommand(LogOutCommandExecuted)); }
        }
        
        private void MouseWheelCommandExecuted()
        {
            if (_isImageReadyForRotation == false) return;

            int a = Mouse.MouseWheelDeltaForOneLine / 5;
            float numSteps = Mouse.RightButton == MouseButtonState.Pressed ? -a / 15 : a / 15;
            angle += numSteps / 10.0;

            _palmRotatedImageBitmap = new Bitmap(_palmImageBitmap);
            RotateImage(_palmRotatedImageBitmap, (float)angle);
            PalmLoadedImage = ConvertFromBitmapToBitmapSource(_palmRotatedImageBitmap);
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
            _palmImageBitmap = ConvertFromBitmapSourceToBitmap(PalmLoadedImage as BitmapSource);

            if (MessageBox.Show("Image rotated properly?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                _isImageReadyForRotation = true;
        }

        public void RotateImage(Bitmap bitmap, float angle)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.TranslateTransform((float)bitmap.Width / 2, (float)bitmap.Height / 2);
                graphics.RotateTransform(angle);
                graphics.TranslateTransform(-(float)bitmap.Width / 2, -(float)bitmap.Height / 2);
                graphics.DrawImage(bitmap, new PointF(0, 0));
            }
        }

        private void RecognizePalmCommandExecuted()
        {
            if (_isImageReadyForRotation)
            {
                _isImageReadyForRotation = false;
                _palmFilename = _palmFilename.Replace(".jpg", "ROTATED.jpg");
                _palmRotatedImageBitmap.Save(_palmFilename);
            }
            PalmTool _tool = new PalmTool(_palmFilename, _cannyParamLow, _cannyParamHigh);
            IsEdgesDetected = Visibility.Visible;
            PalmEdgesImage = ConvertFromBitmapToBitmapSource(_tool.GetEdgesPalmBitmap);
            PalmGrayImage = ConvertFromBitmapToBitmapSource(_tool.GetGrayPalmBitmap);
            PalmBlurImage = ConvertFromBitmapToBitmapSource(_tool.GetBlurPalmBitmap);
            if (MessageBox.Show("Edges detected properly?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            { }
        }

        private void MeasurePalmCommandExecuted()
        {
            throw new NotImplementedException();
        }

        private void SearchPalmCommandExecuted()
        {
            throw new NotImplementedException();
        }

        private void AddPalmToBaseCommandExecuted()
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
            BitmapFrame frame = BitmapFrame.Create(bmp);
            encoder.Frames.Add(frame);
            MemoryStream stream = new MemoryStream();
            encoder.Save(stream);
            return new Bitmap(stream);
        }

    }
}
