using Microsoft.Win32;
using DatabaseConnection;
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
        private IDatabaseConnection connection;
        private PalmTool _tool;
        private Visibility _isEdgesDetected;
        private int _cannyParamLow, _cannyParamHigh;
        private double angle = 0.0;
        private bool _isFileLoaded, _isPalmMeasured, _isUserLogIn, _isImageReadyForRotation;
        private string _palmFilename;
        private ImageSource _palmImage, _palmEdgesImage, _palmBlurImage, _palmGrayImage;
        private Bitmap _palmEdgesBitmap, _palmRotatedEdgesBitmap;
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

        public bool IsUserLogIn
        {
            get { return _isUserLogIn; }
            set
            {
                if (_isUserLogIn != value)
                    _isUserLogIn = value;
                OnPropertyChanged("IsUserLogIn");
            }
        }

        public bool IsPalmMeasured
        {
            get { return _isPalmMeasured; }
            set
            {
                if (_isPalmMeasured != value)
                    _isPalmMeasured = value;
                OnPropertyChanged("IsPalmMeasured");
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
        private ICommand _mouseWheelCommand, _loadFileCommand, _measurePalmCommand, _recognizePalmCommand, _searchPalmCommand, _addPalmToBaseCommand, _logInCommand, _addUserToBaseCommand;

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

        public ICommand AddUserToBaseCommand
        {
            get { return _addUserToBaseCommand ?? (_addUserToBaseCommand = new DelegateCommand(AddUserToBaseCommandExecuted)); }
        }

        private void MouseWheelCommandExecuted()
        {
            if (_isImageReadyForRotation == false) return;

            int a = Mouse.MouseWheelDeltaForOneLine / 5;
            float numSteps = Mouse.RightButton == MouseButtonState.Pressed ? -a / 15 : a / 15;
            angle += numSteps;

            _palmRotatedEdgesBitmap = RotateImage(new Bitmap(_palmEdgesBitmap), (float)angle);
            PalmEdgesImage = ConvertFromBitmapToBitmapSource(_palmRotatedEdgesBitmap);
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

        public Bitmap RotateImage(Bitmap bitmap, float angle)
        {
            Bitmap newBmp = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);
            using (Graphics graphics = Graphics.FromImage(newBmp))
            {
                graphics.Clear(System.Drawing.Color.Black);
                graphics.TranslateTransform((float)bitmap.Width / 2, (float)bitmap.Height / 2);
                graphics.RotateTransform(angle);
                graphics.TranslateTransform(-(float)bitmap.Width / 2, -(float)bitmap.Height / 2);
                graphics.DrawImage(bitmap, new PointF(0, 0));
            }
            return newBmp;
        }

        private void RecognizePalmCommandExecuted()
        {
            _tool = new PalmTool(_palmFilename, _cannyParamLow, _cannyParamHigh);
            IsEdgesDetected = Visibility.Visible;
            PalmEdgesImage = ConvertFromBitmapToBitmapSource(_tool.GetEdgesPalmBitmap);
            PalmGrayImage = ConvertFromBitmapToBitmapSource(_tool.GetGrayPalmBitmap);
            PalmBlurImage = ConvertFromBitmapToBitmapSource(_tool.GetBlurPalmBitmap);

            _palmEdgesBitmap = ConvertFromBitmapSourceToBitmap(PalmEdgesImage as BitmapSource);
            if (MessageBox.Show("Edges detected properly?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                if (MessageBox.Show("Image rotated properly?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    _isImageReadyForRotation = true;
        }

        private void MeasurePalmCommandExecuted()
        {
            if (_isImageReadyForRotation)
            {
                _isImageReadyForRotation = false;
                _palmRotatedEdgesBitmap.Save(_palmFilename.Replace(".jpg", "ROTATED.jpg"));
            }

            //najpierw czynnosci w _tool związane z pomiarem + inicjalizacja _tool.MeasuredParameters 
            IsPalmMeasured = true;
        }

        private void SearchPalmCommandExecuted()
        {
            throw new NotImplementedException();
        }

        private void AddPalmToBaseCommandExecuted()
        {
            string description = "";
            DescriptionWindow dw = new DescriptionWindow();
            if (dw.ShowDialog() == true)
                description = dw.Description;
            if (description == "")
                if (MessageBox.Show("Add without description?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    description = " ";
                else
                    return;

            connection.AddNewData(Image.FromFile(_palmFilename), description, new PalmParameters()); //_tool.MeasuredParameters);
            MessageBox.Show("Palm added to base.");
        }

        private void AddUserToBaseCommandExecuted()
        {
            NewUserWindow nw = new NewUserWindow();
            if (nw.ShowDialog() == true)
                if (connection.AddNewUser(nw.newUserName, nw.newUserPassword) == false)
                    MessageBox.Show("Can't add new user to base");
        }

        private void LogInCommandExecuted()
        {
            NewUserWindow nw = new NewUserWindow();
            if (nw.ShowDialog() == true)
                if (connection.Login(nw.newUserName, nw.newUserPassword))
                    IsUserLogIn = true;
                else
                    MessageBox.Show("Can't log in user");
        }
        #endregion

        public ViewModel()
        {
            connection = Database.Instance;
            _isEdgesDetected = Visibility.Collapsed;
            _isFileLoaded = false;
            _isPalmMeasured = false;
            _isUserLogIn = true;
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
