﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PalmRecognizer
{
    /// <summary>
    /// Interaction logic for DataBaseViewer.xaml
    /// </summary>
    public partial class DataBaseViewer : UserControl
    {
        // <summary>
        /// Gets or sets the list
        /// </summary>
        public List<DatabaseConnection.Model.PalmImage> ImageCollection
        {
            get { return (List<DatabaseConnection.Model.PalmImage>)GetValue(ImageCollectionProperty); }
            set { SetValue(ImageCollectionProperty, value); }
        }

        /// <summary>
        /// Identified the ImageCollection dependency property
        /// </summary>
        public static DependencyProperty ImageCollectionProperty =
            DependencyProperty.Register("ImageCollection", typeof(List<DatabaseConnection.Model.PalmImage>), typeof(DataBaseViewer));

        public DataBaseViewer()
        {
            InitializeComponent();
        }
    }
}
