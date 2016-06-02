namespace PalmRecognizer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using DatabaseConnection;

    using Microsoft.Win32;

    using Model;

    using Helpers;

    using Point = System.Windows.Point;
    using System.Windows.Shapes;

    using Color = System.Drawing.Color;
    using System.Windows.Data;
    class ViewModel : ViewModelBase
    {
        #region Variables
        private IDatabaseConnection _connection;
        private LogWriter _logWriter;
        private PalmTool _tool;
        private int _cannyParamLow, _cannyParamHigh, _brightnessParam;
        private double _angle = 0.0, _contrastParam;
        private bool _isFileLoaded, _isPalmMeasured, _isPalmDefectsCalculated, _isEdgesDetected, _isResultsVisible, _isUserLogIn, _isMouseDown, _isImageReadyForRotation, _isImageReadyForCrop;
        private string _palmFilename, _palmFilenameExtension, _actualUser, _windowTitle;
        private System.Windows.Shapes.Rectangle _imageCroppedArea;
        private Point _startMousePoint, _startMousePointImage;
        private ImageSource _palmImage, _palmEdgesImage, _palmBlurImage, _palmGrayImage, _palmContourImage, _palmBwImage;
        private Bitmap _palmBitmap, _palmEdgesBitmap, _palmRotatedEdgesBitmap;
        private DatabaseConnection.Model.Palm _selectedPalm;
        private DatabaseConnection.Model.PalmImage _selectedPalmImage;
        private object _selectedTab;

        private int _metricTypeIndex = 0;
        private readonly CollectionView _metricTypes;

        private ObservableCollection<Defect> _defects;

        private bool _isDefectMouseDown;

        private Point _mouseCurrentPosition;

        private Ellipse _currentEllipse;

        private double _imageWidth, _imageHeight;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public double Width
        {
            get { return _imageWidth; }
            set
            {
                if (_imageWidth == value)
                    return;
                _imageWidth = value;
                OnPropertyChanged("Width");
            }
        }

        public int MetricTypeIndex
        {
            get { return _metricTypeIndex; }
            set
            {
                _metricTypeIndex = value;
                OnPropertyChanged("MetricTypeIndex");
            }
        }


        public CollectionView MetricTypes
        {
            get { return _metricTypes; }
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public double Height
        {
            get { return _imageHeight; }
            set
            {
                if (_imageHeight == value)
                    return;
                _imageHeight = value;
                OnPropertyChanged("Height");
            }
        }

        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                if (_windowTitle != value)
                    _windowTitle = value;
                OnPropertyChanged("WindowTitle");
            }
        }

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

        public List<Tuple<DatabaseConnection.Model.PalmImage, double>> FoundPalmItems
        {
            get;
            set;
        }

        public PalmParameters WantedPalmParameters
        {
            get; private set;
        }

        public ImageSource WantedPalmImage
        {
            get; private set;
        }

        public ImageSource WantedPalmDefectsImage
        {
            get; private set;
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

        public bool IsResultsVisible
        {
            get { return _isResultsVisible; }
            set
            {
                if (_isResultsVisible != value)
                    _isResultsVisible = value;
                OnPropertyChanged("IsResultsVisible");
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

        public bool IsPalmDefectsCalculated
        {
            get { return _isPalmDefectsCalculated; }
            set
            {
                if (_isPalmDefectsCalculated != value)
                    _isPalmDefectsCalculated = value;
                OnPropertyChanged("IsPalmDefectsCalculated");
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

        public ObservableCollection<Defect> Defects
        {
            get
            {
                return _defects;
            }
            set
            {
                _defects = value;
                OnPropertyChanged("Defects");
            }
        }
        #endregion

        #region Commands
        private ICommand _mouseWheelCommand, _mouseDownCommand, _mouseDownBorderCommand, _mouseDownPreviewCommand, _mouseDownDefectsPreviewCommand, _mouseUpCommand, _mouseMoveCommand;

        private ICommand _recognizePalmCommand, _searchPalmCommand, _addPalmToBaseCommand, _removePalmFromBaseCommand, _loadFileCommand, _saveFileCommand, _resetCommand, _cropFileCommand,
            _measurePalmCommand, _histogramEqualizationCommand, _histogramEqualizationUNDOCommand, _logInCommand, _logOutCommand, _addUserToBaseCommand, _closingCommand;

        private ICommand _mouseDownDefectCommand, _mouseMoveDefectCommand, _mouseUpDefectCommand, _removeDefectCommand, _addDefectCommand, _calculateCommand;

        public ICommand MouseDownPreviewCommand
        {
            get { return _mouseDownPreviewCommand ?? (_mouseDownPreviewCommand = new DelegateCommand(MouseDownPreviewCommandExecuted)); }
        }

        public ICommand MouseDownDefectsPreviewCommand
        {
            get { return _mouseDownDefectsPreviewCommand ?? (_mouseDownDefectsPreviewCommand = new DelegateCommand(MouseDownDefectsPreviewCommandExecuted)); }
        }

        public ICommand MouseWheelCommand
        {
            get { return _mouseWheelCommand ?? (_mouseWheelCommand = new ActionCommand<MouseWheelEventArgs>(MouseWheelCommandExecuted)); }
        }

        public ICommand MouseDownDefectCommand
        {
            get { return _mouseDownDefectCommand ?? (_mouseDownDefectCommand = new ActionCommand<MouseButtonEventArgs>(MouseDownDefectCommandExecuted)); }
        }

        public ICommand MouseMoveDefectCommand
        {
            get { return _mouseMoveDefectCommand ?? (_mouseMoveDefectCommand = new ActionCommand<MouseEventArgs>(MouseMoveDefectCommandExecuted)); }
        }

        public ICommand MouseUpDefectCommand
        {
            get { return _mouseUpDefectCommand ?? (_mouseUpDefectCommand = new ActionCommand<MouseButtonEventArgs>(MouseUpDefectCommandExecuted)); }
        }

        public ICommand ResetCommand
        {
            get { return _resetCommand ?? (_resetCommand = new DelegateCommand(ResetCommandExecuted)); }
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

        public ICommand HistogramEqualizationCommand
        {
            get { return _histogramEqualizationCommand ?? (_histogramEqualizationCommand = new DelegateCommand(HistogramEqualizationCommandExecuted)); }
        }

        public ICommand HistogramEqualizationUNDOCommand
        {
            get { return _histogramEqualizationUNDOCommand ?? (_histogramEqualizationUNDOCommand = new DelegateCommand(HistogramEqualizationUNDOCommandExecuted)); }
        }

        public ICommand MeasurePalmCommand
        {
            get { return _measurePalmCommand ?? (_measurePalmCommand = new DelegateCommand(MeasurePalmCommandExecuted)); }
        }

        public ICommand RecognizePalmCommand
        {
            get { return _recognizePalmCommand ?? (_recognizePalmCommand = new DelegateCommand(DetectPalmEdgesCommandExecuted)); }
        }

        public ICommand SearchPalmCommand
        {
            get { return _searchPalmCommand ?? (_searchPalmCommand = new DelegateCommand(SearchPalmCommandExecuted)); }
        }

        public ICommand RemovePalmFromBaseCommand
        {
            get { return _removePalmFromBaseCommand ?? (_removePalmFromBaseCommand = new DelegateCommand(RemovePalmFromBaseCommandExecuted)); }
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
            get { return _mouseDownBorderCommand ?? (_mouseDownBorderCommand = new DelegateCommand(MouseDownBorderCommandExecuted)); }
        }

        public ICommand MouseUpCommand
        {
            get { return _mouseUpCommand ?? (_mouseUpCommand = new DelegateCommand(MouseUpCommandExecuted)); }
        }

        public ICommand MouseMoveCommand
        {
            get { return _mouseMoveCommand ?? (_mouseMoveCommand = new DelegateCommand(MouseMoveCommandExecuted)); }
        }

        public ICommand RemoveDefectCommand
        {
            get { return _removeDefectCommand ?? (_removeDefectCommand = new DelegateCommand(RemoveDefectCommandExecuted)); }
        }

        public ICommand AddDefectCommand
        {
            get { return _addDefectCommand ?? (_addDefectCommand = new DelegateCommand(AddDefectCommandExecuted)); }
        }

        public ICommand CalculateCommand
        {
            get { return _calculateCommand ?? (_calculateCommand = new DelegateCommand(CalculateCommandExecuted)); }
        }

        #endregion

        #region Commands Executions
        private void MouseDownPreviewCommandExecuted(object o)
        {
            using (MemoryStream ms = new MemoryStream(SelectedPalmImage.Image))
            {
                var img = System.Drawing.Image.FromStream(ms);
                Window window = new Window { WindowStyle = WindowStyle.ToolWindow, WindowStartupLocation = WindowStartupLocation.CenterScreen, Title = "Preview" };
                window.Width = window.Height = SystemParameters.PrimaryScreenHeight - 40;
                var imageControl = new System.Windows.Controls.Image { Source = ConvertFromBitmapToBitmapSource((Bitmap)img), Stretch = Stretch.None };
                window.Content = new System.Windows.Controls.ScrollViewer
                {
                    Content = imageControl,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                window.ShowDialog();
            }
        }

        private void MouseDownDefectsPreviewCommandExecuted(object obj)
        {
            using (MemoryStream ms = new MemoryStream(SelectedPalmImage.DefectsImage))
            {
                var img = System.Drawing.Image.FromStream(ms);
                Window window = new Window { WindowStyle = WindowStyle.ToolWindow, WindowStartupLocation = WindowStartupLocation.CenterScreen, Title = "Preview" };
                window.Width = window.Height = SystemParameters.PrimaryScreenHeight - 40;
                var imageControl = new System.Windows.Controls.Image { Source = ConvertFromBitmapToBitmapSource((Bitmap)img), Stretch = Stretch.None };
                window.Content = new System.Windows.Controls.ScrollViewer
                {
                    Content = imageControl,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                window.ShowDialog();
            }
        }

        private void HistogramEqualizationCommandExecuted(object obj)
        {
            PalmLoadedImage = ConvertFromBitmapToBitmapSource(_tool.EqualizeHistogram());
        }

        private void HistogramEqualizationUNDOCommandExecuted(object obj)
        {
            PalmLoadedImage = ConvertFromBitmapToBitmapSource(_tool.UNDOHistogramEqualization());
        }

        private void MouseMoveCommandExecuted(object o)
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

        private void MouseUpCommandExecuted(object o)
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

        private void MouseDownCommandExecuted(object o)
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

        private void MouseWheelCommandExecuted(MouseWheelEventArgs args)
        {
            if (_isImageReadyForRotation == false) return;
            _angle += args.Delta > 0 ? -1.5 : 1.5;
            _palmRotatedEdgesBitmap = RotateImage(new Bitmap(_palmEdgesBitmap), (float)_angle);
            PalmEdgesImage = ConvertFromBitmapToBitmapSource(_palmRotatedEdgesBitmap);
            _logWriter.AddRotationInfo(_actualUser);
            OnPropertyChanged("LogContent");
        }

        private void CropFileCommandExecuted(object o)
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

        private void LoadFileCommandExecuted(object o)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image files (*.png;*.jpg;*.bmp;*.gif)|*.png;*.jpg;*.bmp;*.gif";
            if (fileDialog.ShowDialog() != true) return;

            PalmEdgesImage = PalmContourImage = PalmGrayImage = PalmBwImage = null;
            IsEdgesDetected = false;
            IsPalmMeasured = false;
            IsPalmDefectsCalculated = false;
            IsResultsVisible = false;
            IsFileLoaded = true;
            _palmRotatedEdgesBitmap = null;
            _palmFilename = fileDialog.FileName;
            _palmFilenameExtension = System.IO.Path.GetExtension(_palmFilename);
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

        private void ResetCommandExecuted(object o)
        {
            if (_palmFilename == "") return;
            if (MessageBox.Show("Are you sure?!", "Reset", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            _logWriter.AddResetInfo(_actualUser);
            OnPropertyChanged("LogContent");
            if(IsFileLoaded)
            {
                CannyParamHigh = "250";
                CannyParamLow = "100";
                ContrastValue = 1.0;
                BrightnessValue = 0;
            }
            IsEdgesDetected = false;
            IsImageReadyForCrop = false;
            IsFileLoaded = false;
            IsPalmMeasured = false;
            IsPalmDefectsCalculated = false;
            IsResultsVisible = false;
            _palmRotatedEdgesBitmap = null;
            _palmFilename = "";
            PalmLoadedImage = PalmEdgesImage = PalmBlurImage = PalmBwImage = PalmGrayImage = PalmContourImage = null;
        }

        private void SaveFileCommandExecuted(object o)
        {
            if ((SelectedTab as TabItem).Header.ToString().Contains("Browser")) return;
            var fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Image files (*.jpg)|*.jpg";
            if (fileDialog.ShowDialog() != true) return;
            switch ((SelectedTab as TabItem).Header.ToString())
            {
                case "Contour":
                    {
                        BitmapFromDefectsImage().Save(fileDialog.FileName);
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

        private void DetectPalmEdgesCommandExecuted(object o)
        {
            _angle = 0;
            _logWriter.AddPreprocessingInfo(_actualUser, ContrastValue, BrightnessValue);
            OnPropertyChanged("LogContent");
            //_tool = new PalmTool(_palmFilename, _cannyParamHigh, _cannyParamLow, _contrastParam, _brightnessParam);
            _tool.DetectEdges();

            IsEdgesDetected = true;
            PalmGrayImage = ConvertFromBitmapToBitmapSource(_tool.GetGrayPalmBitmap);
            PalmBlurImage = ConvertFromBitmapToBitmapSource(_tool.GetBlurPalmBitmap);
            PalmEdgesImage = ConvertFromBitmapToBitmapSource(_tool.GetEdgesPalmBitmap);
            PalmBwImage = ConvertFromBitmapToBitmapSource(_tool.GetBwPalmBitmap);
            _palmEdgesBitmap = ConvertFromBitmapSourceToBitmap(PalmEdgesImage as BitmapSource);
            if (MessageBox.Show("Edges detected properly?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _logWriter.AddEdgesDetectionInfo(_actualUser, _cannyParamLow, _cannyParamHigh);
                OnPropertyChanged("LogContent");
                if (MessageBox.Show("Image rotated properly?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    _isImageReadyForRotation = true;
            }

        }

        private void MeasurePalmCommandExecuted(object o)
        {
            _isImageReadyForRotation = false;
            if (_palmRotatedEdgesBitmap != null) _tool.SaveRotatedBitmap(_palmRotatedEdgesBitmap);
            _tool.GetDefects();
            PalmContourImage = ConvertFromBitmapToBitmapSource(_tool.GetContourPalmBitmap);
            Defects = _tool.Defects;
            IsPalmMeasured = true;
            _logWriter.AddPalmMetricsInfo(_actualUser);
            OnPropertyChanged("LogContent");
        }



        private const int NROFRESULTSTOSHOW = 5;

        private void SearchPalmCommandExecuted(object o)
        {
            SetWantedPalm();
            IsResultsVisible = true;
            FoundPalmItems = _connection.Identify(_tool.MeasuredParameters, NROFRESULTSTOSHOW, (MetricType)MetricTypeIndex);
            OnPropertyChanged("FoundPalmItems");
        }

        private void AddPalmToBaseCommandExecuted(object o)
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
            PalmParameters parameters = _tool.MeasuredParameters;
            System.Drawing.Image img = ConvertFromBitmapSourceToBitmap(PalmLoadedImage as BitmapSource);
            System.Drawing.Image imgDefects = BitmapFromDefectsImage();
            _connection.AddNewData(_actualUser, DateTime.Now, img, /*, imgDefects*/ description, _tool.MeasuredParameters, imgDefects);
            MessageBox.Show("Palm added to base.");
            OnPropertyChanged("PalmItems");
        }

        private void RemovePalmFromBaseCommandExecuted(object obj)
        {
            if (_selectedPalm == null) return;
            if (MessageBox.Show("Are you sure?!", "Remove from base", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            _connection.RemoveData(_selectedPalm.PalmId);
            OnPropertyChanged("PalmItems");
            SelectedPalm = null;
            SelectedPalmImage = null;
        }

        private void AddUserToBaseCommandExecuted(object o)
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

        private void LogInCommandExecuted(object o)
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
            WindowTitle = "Palm Recognizer     USER: " + _actualUser + "     LOGGED: " + DateTime.Now.ToString();
            _logWriter.AddLogInInfo(_actualUser);
            OnPropertyChanged("LogContent");
        }

        private void LogOutCommandExecuted(object o)
        {
            if (MessageBox.Show("Are you sure you want to log out?\nUnsaved data will be lost.\n", "Confirm logging out", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
				return;

            IsUserLogIn = false;
            if (IsFileLoaded)
            {
                CannyParamHigh = "250";
                CannyParamLow = "100";
                ContrastValue = 1.0;
                BrightnessValue = 0;
            }
            IsEdgesDetected = false;
            IsImageReadyForCrop = false;
            IsFileLoaded = false;
            IsPalmMeasured = false;
            IsPalmDefectsCalculated = false;
            IsResultsVisible = false;
            _palmRotatedEdgesBitmap = null;
            _palmFilename = "";
            PalmLoadedImage = PalmEdgesImage = PalmBlurImage = PalmBwImage = PalmGrayImage = PalmContourImage = null;
            ResetCommandExecuted(o);
            _logWriter.AddLogOutInfo(_actualUser);
            OnPropertyChanged("LogContent");
            WindowTitle = "Palm Recognizer";
        }

        private void ClosingCommandExecuted(object o)
        {
            if (IsUserLogIn)
            {
                MessageBox.Show("User will be automatically log out.\nUnsaved data will be lost.\n\nLog file will be automatically saved.");
                LogOutCommandExecuted(null);
            }
            _logWriter.AddCloseInfo();
            OnPropertyChanged("LogContent");
            _logWriter.SaveLogFile();
        }

        private void MouseDownDefectCommandExecuted(MouseButtonEventArgs args)
        {
            var ellipse = args.OriginalSource as Ellipse;
            if (ellipse != null)
            {
                _mouseCurrentPosition = args.GetPosition((Canvas)ellipse.Parent);
                _isDefectMouseDown = true;
                _currentEllipse = ellipse;
            }
        }

        private void MouseMoveDefectCommandExecuted(MouseEventArgs args)
        {
            if (_currentEllipse != null && _isDefectMouseDown)
            {
                var parent = (Canvas)_currentEllipse.Parent;
                var currentPosition = args.GetPosition(parent);
                var delta = currentPosition - _mouseCurrentPosition;
                var defect = Defects.First(d => d.Id == (string)_currentEllipse.Tag);

                var c = ((SolidColorBrush)_currentEllipse.Fill).Color;
                var color = Color.FromArgb(c.A, c.R, c.G, c.B).ToArgb();

                switch (color)
                {
                    case -8388864: // chartreuse
                        defect.Far.X = defect.Far.X + (delta.X / Width);
                        defect.Far.Y = defect.Far.Y + (delta.Y / Height);
                        break;
                    case -10185235: // cornflower blue
                        defect.End.X = defect.End.X + (delta.X / Width);
                        defect.End.Y = defect.End.Y + (delta.Y / Height);
                        break;
                    case -3730043: // medium violet red
                        defect.Start.X = defect.Start.X + (delta.X / Width);
                        defect.Start.Y = defect.Start.Y + (delta.Y / Height);
                        break;
                }

                _mouseCurrentPosition = currentPosition;
            }
        }

        private void MouseUpDefectCommandExecuted(MouseButtonEventArgs args)
        {
            _isDefectMouseDown = false;
            _currentEllipse = null;
        }

        private void RemoveDefectCommandExecuted(object obj)
        {
            throw new NotImplementedException();
        }

        private void AddDefectCommandExecuted(object obj)
        {
            throw new NotImplementedException();
        }

        private void CalculateCommandExecuted(object obj)
        {
            PalmContourImage = ConvertFromBitmapToBitmapSource(_tool.CalculateMeasurements(Defects).Bitmap);
            IsPalmDefectsCalculated = true;
        }

        #endregion

        public ViewModel()
        {
            _windowTitle = "Palm Recognizer";
            _logWriter = new LogWriter();
            _connection = Database.Instance;
            _isEdgesDetected = false;
            _isImageReadyForCrop = false;
            _isFileLoaded = false;
            _isPalmMeasured = false;
            _isPalmDefectsCalculated = false;
            _isUserLogIn = false;
            _isResultsVisible = false;
            _cannyParamHigh = 250;
            _cannyParamLow = 100;
            _contrastParam = 1;
            _brightnessParam = 0;

            IList<string> metricNamesList = new List<string>();
            foreach (string metricName in Enum.GetNames(typeof(MetricType)))
                metricNamesList.Add(metricName + " distance");
            _metricTypes = new CollectionView(metricNamesList);
        }

        private Bitmap BitmapFromDefectsImage()
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds((Application.Current.MainWindow as MainWindow).DefectsCanvas);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, (PalmContourImage as BitmapSource).DpiX, (PalmContourImage as BitmapSource).DpiY, PixelFormats.Default);
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                VisualBrush visual = new VisualBrush((Application.Current.MainWindow as MainWindow).DefectsCanvas);
                drawingContext.DrawRectangle(System.Windows.Media.Brushes.Black, null, new Rect(0, 0, (int)bounds.Width, (int)bounds.Height));
                drawingContext.DrawRectangle(visual, null, new Rect(0, 0, (int)bounds.Width, (int)bounds.Height));
            }
            rtb.Render(drawingVisual);
            return ConvertFromBitmapSourceToBitmap(rtb);
        }

        private void SetWantedPalm()
        {
            WantedPalmDefectsImage = ConvertFromBitmapToBitmapSource(BitmapFromDefectsImage());
            OnPropertyChanged("WantedPalmDefectsImage");
            WantedPalmImage = PalmLoadedImage;
            OnPropertyChanged("WantedPalmImage");
            WantedPalmParameters = _tool.MeasuredParameters;
            OnPropertyChanged("WantedPalmParameters");
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
