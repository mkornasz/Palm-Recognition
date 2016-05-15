using DatabaseConnection;
using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace PalmRecognizer
{
    class ViewModel : ViewModelBase
    {
        #region Variables
        private IDatabaseConnection _connection;
        private LogWriter _logWriter;
        private PalmTool _tool;
        private int _cannyParamLow, _cannyParamHigh, _brightnessParam;
        private double _angle = 0.0, _contrastParam;
        private bool _isFileLoaded, _isPalmMeasured, _isEdgesDetected, _isUserLogIn, _isMouseDown, _isImageReadyForRotation, _isImageReadyForCrop;
        private string _palmFilename, _palmFilenameExtension, _actualUser;
        private System.Windows.Shapes.Rectangle _imageCroppedArea;
        private Point _startMousePoint, _startMousePointImage;
        private ImageSource _palmImage, _palmEdgesImage, _palmBlurImage, _palmGrayImage, _palmContourImage, _palmBwImage;
        private Bitmap _palmBitmap, _palmEdgesBitmap, _palmRotatedEdgesBitmap;
        private DatabaseConnection.Model.Palm _selectedPalm;
        private DatabaseConnection.Model.PalmImage _selectedPalmImage;
        private object _selectedTab;
        #endregion

        #region Properties
        public string LogContent
        {
            get { return _logWriter.LogContent; }
        }

        public List<DatabaseConnection.Model.PalmImage> PalmItems
        {
            get { return IsUserLogIn ? _connection.GetAllImages() : null; }
        }

        public DatabaseConnection.Model.Palm SelectedPalm
        {
            get { return _selectedPalm; }
            set
            {
                if (_selectedPalm != value)
                    _selectedPalm = value;
                OnPropertyChanged("SelectedPalm");
            }
        }

        public DatabaseConnection.Model.PalmImage SelectedPalmImage
        {
            get { return _selectedPalmImage; }
            set
            {
                if (_selectedPalmImage != value)
                    _selectedPalmImage = value;
                OnPropertyChanged("SelectedPalmImage");
            }
        }

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

        public ImageSource PalmContourImage
        {
            get { return _palmContourImage; }
            set
            {
                if (_palmContourImage != value)
                    _palmContourImage = value;
                OnPropertyChanged("PalmContourImage");
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

        public ImageSource PalmBwImage
        {
            get { return _palmBwImage; }
            set
            {
                if (_palmBwImage != value)
                    _palmBwImage = value;
                OnPropertyChanged("PalmBwImage");
            }
        }

        public object SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                if (_selectedTab != value)
                    _selectedTab = value;
                OnPropertyChanged("SelectedTab");
            }
        }

        public bool IsEdgesDetected
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
                return IsEdgesDetected ? false : true;
            }
        }

        public bool IsImageReadyForCrop
        {
            get { return _isImageReadyForCrop; }
            set
            {
                if (_isImageReadyForCrop != value)
                    _isImageReadyForCrop = value;
                OnPropertyChanged("IsImageReadyForCrop");
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

        public int BrightnessValue
        {
            get { return _brightnessParam; }
            set
            {
                if (_brightnessParam != value)
                    _brightnessParam = value;

                _tool.BrightnessParam = _brightnessParam;
                PalmLoadedImage = ConvertFromBitmapToBitmapSource(_tool.ChangeContrastBrightness());
                OnPropertyChanged("BrightnessValue");
            }
        }

        public double ContrastValue
        {
            get { return _contrastParam; }
            set
            {
                if (_contrastParam != value)
                    _contrastParam = value;

                _tool.ContrastParam = _contrastParam;
                PalmLoadedImage = ConvertFromBitmapToBitmapSource(_tool.ChangeContrastBrightness());
                OnPropertyChanged("ContrastValue");
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

                _tool.CannyParamLow = _cannyParamLow;
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
                _tool.CannyParamHigh = _cannyParamHigh;
                OnPropertyChanged("CannyParamHigh");
            }
        }
        #endregion

        #region Commands
        private ICommand _mouseWheelCommand, _mouseDownCommand, _mouseDownBorderCommand, _mouseDownPreviewCommand, _mouseUpCommand, _mouseMoveCommand, _loadFileCommand, _saveFileCommand, _cropFileCommand, _measurePalmCommand,
            _recognizePalmCommand, _searchPalmCommand, _addPalmToBaseCommand, _logInCommand, _logOutCommand, _addUserToBaseCommand, _closingCommand;

        public ICommand MouseWheelCommand
        {
            get { return _mouseWheelCommand ?? (_mouseWheelCommand = new DelegateCommand(MouseWheelCommandExecuted)); }
        }

        public ICommand LoadFileCommand
        {
            get { return _loadFileCommand ?? (_loadFileCommand = new DelegateCommand(LoadFileCommandExecuted)); }
        }

        public ICommand SaveFileCommand
        {
            get { return _saveFileCommand ?? (_saveFileCommand = new DelegateCommand(SaveFileCommandExecuted)); }
        }

        public ICommand CropFileCommand
        {
            get { return _cropFileCommand ?? (_cropFileCommand = new DelegateCommand(CropFileCommandExecuted)); }
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

        public ICommand WindowClosingCommand
        {
            get { return _closingCommand ?? (_closingCommand = new DelegateCommand(ClosingCommandExecuted)); }
        }

        public ICommand AddUserToBaseCommand
        {
            get { return _addUserToBaseCommand ?? (_addUserToBaseCommand = new DelegateCommand(AddUserToBaseCommandExecuted)); }
        }

        public ICommand MouseDownPreviewCommand
        {
            get { return _mouseDownPreviewCommand ?? (_mouseDownPreviewCommand = new DelegateCommand(MouseDownPreviewCommandExecuted)); }
        }

        public ICommand MouseDownCommand
        {
            get { return _mouseDownCommand ?? (_mouseDownCommand = new DelegateCommand(MouseDownCommandExecuted)); }
        }

        public ICommand MouseDownBorderCommand
        {
            get { return _mouseDownBorderCommand ?? (_mouseDownBorderCommand = new DelegateCommand<object>(MouseDownBorderCommandExecuted)); }
        }

        public ICommand MouseUpCommand
        {
            get { return _mouseUpCommand ?? (_mouseUpCommand = new DelegateCommand(MouseUpCommandExecuted)); }
        }

        public ICommand MouseMoveCommand
        {
            get { return _mouseMoveCommand ?? (_mouseMoveCommand = new DelegateCommand(MouseMoveCommandExecuted)); }
        }

        private void MouseMoveCommandExecuted()
        {
            if (_isMouseDown == false) return;

            var selectionPanelPoint = Mouse.GetPosition((Application.Current.MainWindow as MainWindow).SelecionPanel);
            _imageCroppedArea = (Application.Current.MainWindow as MainWindow).SelectionRectangle;

            _imageCroppedArea.SetValue(Canvas.LeftProperty, Math.Min(selectionPanelPoint.X, _startMousePoint.X));
            _imageCroppedArea.SetValue(Canvas.TopProperty, Math.Min(selectionPanelPoint.Y, _startMousePoint.Y));
            _imageCroppedArea.Width = Math.Abs(selectionPanelPoint.X - _startMousePoint.X);
            _imageCroppedArea.Height = Math.Abs(selectionPanelPoint.Y - _startMousePoint.Y);

            if (_imageCroppedArea.Visibility != Visibility.Visible)
                _imageCroppedArea.Visibility = Visibility.Visible;
        }

        private void MouseUpCommandExecuted()
        {
            _isMouseDown = false;
            if (IsImageReadyForCrop == false) return;
            if (MessageBox.Show("Image cropped properly?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                if (_imageCroppedArea != null) _imageCroppedArea.Visibility = Visibility.Collapsed;
                return;
            }
            IsImageReadyForCrop = false;
            _imageCroppedArea.Visibility = Visibility.Collapsed;
            CropImage();
        }

        private void MouseDownCommandExecuted()
        {
            if (IsImageReadyForCrop == false) return;
            _isMouseDown = true;
            if (_startMousePointImage.X + _startMousePointImage.Y + _startMousePoint.X + _startMousePoint.Y == 0)
            {
                _startMousePoint = Mouse.GetPosition((Application.Current.MainWindow as MainWindow).SelecionPanel);
                _startMousePointImage = Mouse.GetPosition((Application.Current.MainWindow as MainWindow).ImageArea);
            }
        }

        private void MouseDownBorderCommandExecuted(object ob)
        {
            SelectedPalmImage = (ob as DatabaseConnection.Model.PalmImage);
            var palmList = _connection.GetAll();
            var palmImgList = _connection.GetAllImages();
            SelectedPalm = palmList.Find(p => p.PalmId == SelectedPalmImage.PalmId);
        }

        private void MouseWheelCommandExecuted()
        {
            if (_isImageReadyForRotation == false) return;

            int a = Mouse.MouseWheelDeltaForOneLine / 5;
            float numSteps = Mouse.RightButton == MouseButtonState.Pressed ? -a / 15 : a / 15;
            _angle += numSteps;
            _palmRotatedEdgesBitmap = RotateImage(new Bitmap(_palmEdgesBitmap), (float)_angle);
            PalmEdgesImage = ConvertFromBitmapToBitmapSource(_palmRotatedEdgesBitmap);
            _logWriter.AddRotationInfo(_actualUser);
            OnPropertyChanged("LogContent");
        }

        private void MouseDownPreviewCommandExecuted()
        {
            MemoryStream ms = new MemoryStream(SelectedPalmImage.Image);
            var img = System.Drawing.Image.FromStream(ms);
            Window window = new Window { WindowStyle = WindowStyle.ToolWindow, WindowStartupLocation = WindowStartupLocation.CenterScreen, Title = "Preview" };
            window.Width = window.Height = SystemParameters.PrimaryScreenHeight - 40;
            var imageControl = new System.Windows.Controls.Image { Source = ConvertFromBitmapToBitmapSource((Bitmap)img), Stretch = Stretch.None };
            window.Content = new System.Windows.Controls.ScrollViewer { Content = imageControl, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
            window.ShowDialog();
        }

        private void CropFileCommandExecuted()
        {
            if ((SelectedTab as TabItem).Header.ToString().Contains("Browser"))
            {
                IsImageReadyForCrop = false;
                return;
            }
            IsEdgesDetected = false;
            IsPalmMeasured = false;
            IsImageReadyForCrop = !IsImageReadyForCrop;
        }

        private void LoadFileCommandExecuted()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image files (*.png;*.jpg;*.bmp;*.gif)|*.png;*.jpg;*.bmp;*.gif";
            if (fileDialog.ShowDialog() != true) return;

            IsEdgesDetected = false;
            IsFileLoaded = true;
            _palmFilename = fileDialog.FileName;
            _palmFilenameExtension = Path.GetExtension(_palmFilename);
            PalmLoadedImage = new BitmapImage(new Uri(_palmFilename));
            _palmBitmap = ConvertFromBitmapSourceToBitmap(_palmImage as BitmapSource);
            _tool = new PalmTool(_palmFilename, _cannyParamHigh, _cannyParamLow, _contrastParam, _brightnessParam);
            _logWriter.AddLoadInfo(_actualUser, _palmFilename);
            OnPropertyChanged("LogContent");

            CannyParamHigh = "250";
            CannyParamLow = "100";
            ContrastValue = 1.0;
            BrightnessValue = 0;

            ResizeImage();
        }

        private void SaveFileCommandExecuted()
        {
            if ((SelectedTab as TabItem).Header.ToString().Contains("Browser")) return;
            var fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Image files (*.jpg)|*.jpg";
            if (fileDialog.ShowDialog() != true) return;
            switch ((SelectedTab as TabItem).Header.ToString())
            {
                case "Contour":
                    {
                        ConvertFromBitmapSourceToBitmap(PalmContourImage as BitmapSource).Save(fileDialog.FileName);
                        break;
                    }
                case "Edges":
                    {
                        ConvertFromBitmapSourceToBitmap(PalmEdgesImage as BitmapSource).Save(fileDialog.FileName);
                        break;
                    }
                case "Blured":
                    {
                        ConvertFromBitmapSourceToBitmap(PalmBlurImage as BitmapSource).Save(fileDialog.FileName);
                        break;
                    }
                case "Black - White":
                    {
                        ConvertFromBitmapSourceToBitmap(PalmBwImage as BitmapSource).Save(fileDialog.FileName);
                        break;
                    }
                case "Gray":
                    {
                        ConvertFromBitmapSourceToBitmap(PalmGrayImage as BitmapSource).Save(fileDialog.FileName);
                        break;
                    }
                case "Original":
                    {
                        ConvertFromBitmapSourceToBitmap(PalmLoadedImage as BitmapSource).Save(fileDialog.FileName);
                        break;
                    }
                default:
                    return;
            }
            _logWriter.AddSaveInfo(_actualUser, (SelectedTab as TabItem).Header.ToString(), fileDialog.FileName);
            OnPropertyChanged("LogContent");
        }

        private void RecognizePalmCommandExecuted()
        {
            _tool = new PalmTool(_palmFilename, _cannyParamHigh, _cannyParamLow, _contrastParam, _brightnessParam);
            _tool.DetectEdges();

            IsEdgesDetected = true;
            PalmGrayImage = ConvertFromBitmapToBitmapSource(_tool.GetGrayPalmBitmap);
            PalmBlurImage = ConvertFromBitmapToBitmapSource(_tool.GetBlurPalmBitmap);
            PalmEdgesImage = ConvertFromBitmapToBitmapSource(_tool.GetEdgesPalmBitmap);
            PalmBwImage = ConvertFromBitmapToBitmapSource(_tool.GetBwPalmBitmap);
            _palmEdgesBitmap = ConvertFromBitmapSourceToBitmap(PalmEdgesImage as BitmapSource);

            if (MessageBox.Show("Edges detected properly?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                if (MessageBox.Show("Image rotated properly?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    _isImageReadyForRotation = true;
        }

        private void MeasurePalmCommandExecuted()
        {
            _isImageReadyForRotation = false;

            _tool.GetMeasurements();
            PalmContourImage = ConvertFromBitmapToBitmapSource(_tool.GetContourPalmBitmap);
            IsPalmMeasured = true;
        }

        private void SearchPalmCommandExecuted()
        {
            //wywolanie metod przeszukujacych baze danych dajacych liste kandydatow
            //wyswietlenie listy kandydatow
            throw new NotImplementedException();
        }

        private void AddPalmToBaseCommandExecuted()
        {
            string description = "";
            DescriptionWindow dw = new DescriptionWindow();
            if (dw.ShowDialog() == false) return;

            description = dw.Description;
            if (description == "")
                if (MessageBox.Show("Add without description?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    description = " ";
                else
                    return;

            //NIE WIEM CZY OBRAZEK W BAZIE MA BYC ORYGINALNY CZY PRZEROBIONY
            //var fileName = _palmFilename.Contains("CROPPED") ? _palmFilename.Replace("CROPPED", "") : _palmFilename;
            // var img = System.Drawing.Image.FromFile(fileName);

            System.Drawing.Image img = ConvertFromBitmapSourceToBitmap(PalmLoadedImage as BitmapSource);
            _connection.AddNewData(img, description, new PalmParameters()); //_tool.MeasuredParameters);
            MessageBox.Show("Palm added to base.");
            OnPropertyChanged("PalmItems");
        }

        private void AddUserToBaseCommandExecuted()
        {
            NewUserWindow nw = new NewUserWindow();
            if (nw.ShowDialog() == true)
            {
                if (_connection.AddNewUser(nw.newUserName, nw.newUserPassword) == false)
                    MessageBox.Show("Can't add new user to base");
                else
                    MessageBox.Show("New user added to base");
            }
        }

        private void LogInCommandExecuted()
        {
            NewUserWindow nw = new NewUserWindow();
            if (nw.ShowDialog() == true)
                if (_connection.Login(nw.newUserName, nw.newUserPassword))
                {
                    IsUserLogIn = true;
                    _actualUser = nw.newUserName;
                    OnPropertyChanged("PalmItems");
                }
                else
                    MessageBox.Show("Can't log in user");

            _logWriter.AddLogInInfo(_actualUser);
            OnPropertyChanged("LogContent");
        }

        private void LogOutCommandExecuted()
        {
            IsUserLogIn = false;
            IsFileLoaded = false;
            IsPalmMeasured = false;
            IsEdgesDetected = false;
            (Application.Current.MainWindow as MainWindow).ImageArea.Source = null;
            _logWriter.AddLogOutInfo(_actualUser);
            OnPropertyChanged("LogContent");
        }

        private void ClosingCommandExecuted()
        {
            if (IsUserLogIn)
            {
                MessageBox.Show("User will be automatically log out.\nUnsaved data will be lost.\n\nLog file will be automatically saved.");
                LogOutCommandExecuted();
            }
            _logWriter.AddCloseInfo();
            OnPropertyChanged("LogContent");
            _logWriter.SaveLogFile();
        }

        #endregion

        public ViewModel()
        {
            _logWriter = new LogWriter();
            _connection = Database.Instance;
            _isEdgesDetected = false;
            _isImageReadyForCrop = false;
            _isFileLoaded = false;
            _isPalmMeasured = false;
            _isUserLogIn = false;
            _cannyParamHigh = 100;
            _cannyParamLow = 100;
            _contrastParam = 1;
            _brightnessParam = 0;
        }

        private void CropImage()
        {
            var img = (Application.Current.MainWindow as MainWindow).ImageArea;
            var bmp = ConvertFromBitmapSourceToBitmap(_palmImage as BitmapSource);
            var cropX = (int)((_startMousePointImage.X) * (_palmImage as BitmapSource).PixelWidth / (img.ActualWidth));
            var cropY = (int)((_startMousePointImage.Y) * (_palmImage as BitmapSource).PixelHeight / (img.ActualHeight));
            var cropWidth = (int)((_imageCroppedArea.Width) * (_palmImage as BitmapSource).PixelWidth / (img.ActualWidth));
            var cropHeight = (int)((_imageCroppedArea.Height) * (_palmImage as BitmapSource).PixelHeight / (img.ActualHeight));
            if (cropX + cropWidth > PalmLoadedImage.Width || cropY + cropHeight > PalmLoadedImage.Height) return;

            PalmLoadedImage = new CroppedBitmap(_palmImage as BitmapSource, new Int32Rect(cropX, cropY, cropWidth, cropHeight));
            _palmFilename = _palmFilename.Replace(_palmFilenameExtension, "CROPPED" + _palmFilenameExtension);
            ConvertFromBitmapSourceToBitmap(PalmLoadedImage as BitmapSource).Save(_palmFilename);
            MessageBox.Show("Cropped image saved automatically .");
            _tool = new PalmTool(_palmFilename, _cannyParamHigh, _cannyParamLow, _contrastParam, _brightnessParam);
            _startMousePoint = new Point();
            _startMousePointImage = new Point();
            _logWriter.AddCropInfo(_actualUser, _palmFilename);
            OnPropertyChanged("LogContent");
        }

        private void ResizeImage()
        {
            var bitmap = _palmImage as BitmapSource;
            if (bitmap.PixelHeight <= 900 && bitmap.PixelWidth <= 900) return;

            var scale = bitmap.PixelWidth > 900 ? 900.0 / bitmap.PixelWidth : 900.0 / bitmap.PixelHeight;
            PalmLoadedImage = new TransformedBitmap(bitmap, new ScaleTransform(scale, scale));

            _palmFilename = _palmFilename.Replace(_palmFilenameExtension, "RESIZED" + _palmFilenameExtension);
            ConvertFromBitmapSourceToBitmap(PalmLoadedImage as BitmapSource).Save(_palmFilename);
            MessageBox.Show("Resized image saved automatically .");
            _tool = new PalmTool(_palmFilename, _cannyParamHigh, _cannyParamLow, _contrastParam, _brightnessParam);
            _logWriter.AddResizeInfo(_palmFilename);
            OnPropertyChanged("LogContent");
        }

        private Bitmap RotateImage(Bitmap bitmap, float angle)
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

        private BitmapSource ConvertFromBitmapToBitmapSource(Bitmap bmp)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        private Bitmap ConvertFromBitmapSourceToBitmap(BitmapSource bmp)
        {
            Bitmap newBmp;
            using (MemoryStream ms = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(ms);
                newBmp = new Bitmap(ms);
            }
            return newBmp;
        }
    }
}
