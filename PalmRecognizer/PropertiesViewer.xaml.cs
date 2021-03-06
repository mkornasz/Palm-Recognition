﻿using DatabaseConnection.Model;
using System.Windows;
using System.Windows.Controls;

namespace PalmRecognizer
{
    /// <summary>
    /// Interaction logic for PropertiesViewer.xaml
    /// </summary>
    public partial class PropertiesViewer : UserControl
    {
        // <summary>
        /// Gets or sets palm images
        /// </summary>
        public PalmImage DisplayedPalmImages
        {
            get { return (PalmImage)GetValue(ImagesProperty); }
            set { SetValue(ImagesProperty, value); }
        }

        // <summary>
        /// Gets or sets palm
        /// </summary>
        public Palm DisplayedPalm
        {
            get { return (Palm)GetValue(PalmProperty); }
            set { SetValue(PalmProperty, value); }
        }
        
        public bool ActiveRemoving
        {
            get { return (bool)GetValue(ActiveRemoveProperty); }
            set { SetValue(ActiveRemoveProperty, value); }
        }
        
        public static DependencyProperty ActiveRemoveProperty =
            DependencyProperty.Register("ActiveRemoving", typeof(bool), typeof(PropertiesViewer));

        /// <summary>
        /// Identified the Images dependency property
        /// </summary>
        public static DependencyProperty ImagesProperty =
            DependencyProperty.Register("DisplayedPalmImages", typeof(PalmImage), typeof(PropertiesViewer));

        /// <summary>
        /// Identified the Palm dependency property
        /// </summary>
        public static DependencyProperty PalmProperty =
            DependencyProperty.Register("DisplayedPalm", typeof(Palm), typeof(PropertiesViewer));

        public PropertiesViewer()
        {
            InitializeComponent();
        }
    }
}
