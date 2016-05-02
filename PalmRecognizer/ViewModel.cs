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
        private Visibility _isEdgesDetected;
        private int _cannyParamLow, _cannyParamHigh, _brightnessParam, _selectedTabIndex;
        private double angle = 0.0, _contrastParam;
        private bool _isFileLoaded, _isPalmMeasured, _isUserLogIn, _isMouseDown, _isImageReadyForRotation, _isImageReadyForCrop;
        private string _palmFilename, _actualUser;
        private System.Windows.Shapes.Rectangle _imageCroppedArea;
        private Point _startMousePoint, _startMousePointImage;
        private ImageSource _palmImage, _palmEdgesImage, _palmBlurImage, _palmGrayImage;
        private Bitmap _palmBitmap, _palmEdgesBitmap, _palmRotatedEdgesBitmap;
        private DatabaseConnection.Model.Palm _selectedPalm;
        private DatabaseConnection.Model.PalmImage _selectedPalmImage;
        #endregion 

        #region Properties
        public string LogContent
        {
            get { return _logWriter.LogContent; }
        }

        public List<DatabaseConnection.Model.PalmImage> PalmItems
        {
            get { return _connection.GetAllImages(); }
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

        public int SelectedTab
        {
            get { return _selectedTabIndex; }
            set
            {
                if (_selectedTabIndex != value)
                    _selectedTabIndex = value;
                OnPropertyChanged("SelectedTab");
            }
        }

        public int BrightnessValue
        {
            get { return _brightnessParam; }
            set
            {
                if (_brightnessParam != value)
                    _brightnessParam = value;
                if (_tool == null)
                    _tool = new PalmTool(_palmFilename);
                PalmLoadedImage = ConvertFromBitmapToBitmapSource(_tool.ChangeContrastBroghtness(ContrastValue, BrightnessValue));
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
                if (_tool == null)
                    _tool = new PalmTool(_palmFilename);
                PalmLoadedImage = ConvertFromBitmapToBitmapSource(_tool.ChangeContrastBroghtness(ContrastValue, BrightnessValue));
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
        private ICommand _mouseWheelCommand, _mouseDownCommand, _mouseDownBorderCommand, _mouseUpCommand, _mouseMoveCommand, _loadFileCommand, _saveFileCommand, _cropFileCommand, _measurePalmCommand,
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
                _imageCroppedArea.Visibility = Visibility.Collapsed;
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

            _startMousePoint = Mouse.GetPosition((Application.Current.MainWindow as MainWindow).SelecionPanel);
            _startMousePointImage = Mouse.GetPosition((Application.Current.MainWindow as MainWindow).ImageArea);
        }

        private void MouseDownBorderCommandExecuted(object ob)
        {
            SelectedPalmImage = (ob as DatabaseConnection.Model.PalmImage);
            var palmList = _connection.GetAll();
            var palmImgList = _connection.GetAllImages();
            SelectedPalm = palmList.Find(p => p.PalmId == SelectedPalmImage.PalmId);
        }

        private void CropFileCommandExecuted()
        {
            IsImageReadyForCrop = !IsImageReadyForCrop;
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
            PalmLoadedImage = new BitmapImage(new Uri(_palmFilename));
            _palmBitmap = ConvertFromBitmapSourceToBitmap(_palmImage as BitmapSource);

            _logWriter.AddLoadInfo(_actualUser, _palmFilename);
            OnPropertyChanged("LogContent");
        }

        private void SaveFileCommandExecuted()
        {
            var fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Image files (*.jpg)|*.jpg";
            if (fileDialog.ShowDialog() != true) return;
            switch (SelectedTab)
            {
                case 1:
                    {
                        var tabs = (Application.Current.MainWindow as MainWindow).TabItems;
                        ConvertFromBitmapSourceToBitmap(tabs.Items.Count == 2 ? PalmLoadedImage as BitmapSource : PalmEdgesImage as BitmapSource).Save(fileDialog.FileName);
                        break;
                    }
                case 2:
                    {
                        ConvertFromBitmapSourceToBitmap(PalmBlurImage as BitmapSource).Save(fileDialog.FileName);
                        break;
                    }
                case 3:
                    {
                        ConvertFromBitmapSourceToBitmap(PalmGrayImage as BitmapSource).Save(fileDialog.FileName);
                        break;
                    }
                case 4:
                    {
                        ConvertFromBitmapSourceToBitmap(PalmLoadedImage as BitmapSource).Save(fileDialog.FileName);
                        break;
                    }
            }
            _logWriter.AddSaveInfo(_actualUser, fileDialog.FileName);
            OnPropertyChanged("LogContent");
        }

        private void RecognizePalmCommandExecuted()
        {
            if (_tool == null)
                _tool = new PalmTool(_palmFilename);

            _tool.DetectEdges(_cannyParamLow, _cannyParamHigh);
            IsEdgesDetected = Visibility.Visible;
            PalmGrayImage = ConvertFromBitmapToBitmapSource(_tool.GetGrayPalmBitmap);
            PalmBlurImage = ConvertFromBitmapToBitmapSource(_tool.GetBlurPalmBitmap);
            PalmEdgesImage = ConvertFromBitmapToBitmapSource(_tool.GetEdgesPalmBitmap);
            _palmEdgesBitmap = ConvertFromBitmapSourceToBitmap(PalmEdgesImage as BitmapSource);
            if (MessageBox.Show("Edges detected properly?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                if (MessageBox.Show("Image rotated properly?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    _isImageReadyForRotation = true;
        }

        private void MeasurePalmCommandExecuted()
        {
            _isImageReadyForRotation = false;

            //wywolanie metody _tool -> czynnosci związane z pomiarem + inicjalizacja _tool.MeasuredParameters 
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
            if (dw.ShowDialog() == true)
                description = dw.Description;
            if (description == "")
                if (MessageBox.Show("Add without description?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    description = " ";
                else
                    return;

            //NIE WIEM CZY OBRAZEK W BAZIE MA BYC ORYGINALNY CZY PRZEROBIONY
            //var fileName = _palmFilename.Contains("CROPPED") ? _palmFilename.Replace("CROPPED", "") : _palmFilename;

            var img = System.Drawing.Image.FromFile(_palmFilename);

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
                    MessageBox.Show("User logged in");
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
            MessageBox.Show("User logged out");
            IsUserLogIn = false;

            _logWriter.AddLogOutInfo(_actualUser);
            OnPropertyChanged("LogContent");
        }

        private void ClosingCommandExecuted()
        {
            if (IsUserLogIn)
            {
                MessageBox.Show("User will be log out automatically.\nUnsaved data will be lost.");
                LogOutCommandExecuted();
            }

            _logWriter.SaveLogFile();
            MessageBox.Show("Log file saved automatically");
        }

        #endregion

        public ViewModel()
        {
            _logWriter = new LogWriter();
            _connection = Database.Instance;
            _isEdgesDetected = Visibility.Collapsed;
            _isImageReadyForCrop = false;
            _isFileLoaded = false;
            _isPalmMeasured = false;
            _isUserLogIn = false;
            _cannyParamHigh = 250;
            _cannyParamLow = 100;
            _contrastParam = 1;
            _brightnessParam = 0;
        }

        private void CropImage()
        {
            var img = (Application.Current.MainWindow as MainWindow).ImageArea;
            var cropX = (int)((_startMousePointImage.X) * (_palmImage as BitmapSource).PixelWidth / (img.ActualWidth));
            var cropY = (int)((_startMousePointImage.Y) * (_palmImage as BitmapSource).PixelHeight / (img.ActualHeight));
            var cropWidth = (int)((_imageCroppedArea.Width) * (_palmImage as BitmapSource).PixelWidth / (img.ActualWidth));
            var cropHeight = (int)((_imageCroppedArea.Height) * (_palmImage as BitmapSource).PixelHeight / (img.ActualHeight));
            PalmLoadedImage = new CroppedBitmap(_palmImage as BitmapSource, new Int32Rect(cropX, cropY, cropWidth, cropHeight));

            _palmFilename = _palmFilename.Replace(".jpg", "CROPPED.jpg");
            ConvertFromBitmapSourceToBitmap(PalmLoadedImage as BitmapSource).Save(_palmFilename);
            MessageBox.Show("Cropped image saved automatically .");
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
            MemoryStream stream = new MemoryStream();
            BitmapSource image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
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
