using DatabaseConnection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PalmRecognizer
{
    /// <summary>
    /// Interaction logic for WantedPropertiesViewer.xaml
    /// </summary>
    public partial class WantedPropertiesViewer : UserControl
    {
        // <summary>
        /// Gets or sets palm image
        /// </summary>
        public ImageSource WantedPalmImageSource
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // <summary>
        /// Gets or sets palm defects image
        /// </summary>
        public ImageSource WantedPalmDefectsImageSource
        {
            get { return (ImageSource)GetValue(ImageDefectsProperty); }
            set { SetValue(ImageDefectsProperty, value); }
        }

        // <summary>
        /// Gets or sets palm
        /// </summary>
        public PalmParameters WantedPalmParameters
        {
            get { return (PalmParameters)GetValue(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }

        /// <summary>
        /// Identified the Images dependency property
        /// </summary>
        public static DependencyProperty ImageProperty =
            DependencyProperty.Register("WantedPalmImageSource", typeof(ImageSource), typeof(WantedPropertiesViewer));

        /// <summary>
        /// Identified the Images dependency property
        /// </summary>
        public static DependencyProperty ImageDefectsProperty =
            DependencyProperty.Register("WantedPalmDefectsImageSource", typeof(ImageSource), typeof(WantedPropertiesViewer));

        /// <summary>
        /// Identified the Palm dependency property
        /// </summary>
        public static DependencyProperty ParametersProperty =
            DependencyProperty.Register("WantedPalmParameters", typeof(PalmParameters), typeof(WantedPropertiesViewer));

        public WantedPropertiesViewer()
        {
            InitializeComponent();
        }
    }
}
