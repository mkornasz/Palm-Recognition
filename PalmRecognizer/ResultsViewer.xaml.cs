using DatabaseConnection.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PalmRecognizer
{
    /// <summary>
    /// Interaction logic for ResultsViewer.xaml
    /// </summary>
    public partial class ResultsViewer : UserControl
    {
        // <summary>
        /// Gets or sets the list
        /// </summary>
        public List<Tuple<PalmImage, double, double>> ImageCollection
        {
            get { return (List<Tuple<PalmImage, double, double>>)GetValue(ImageCollectionProperty); }
            set { SetValue(ImageCollectionProperty, value); }
        }

        /// <summary>
        /// Identified the ImageCollection dependency property
        /// </summary>
        public static DependencyProperty ImageCollectionProperty =
            DependencyProperty.Register("ImageCollection", typeof(List<Tuple<PalmImage, double, double>>), typeof(ResultsViewer));
        public ResultsViewer()
        {
            InitializeComponent();
        }
    }
}
