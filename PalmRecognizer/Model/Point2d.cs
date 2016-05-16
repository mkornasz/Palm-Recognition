namespace PalmRecognizer.Model
{
	using System.Drawing;

	using PalmRecognizer.ViewModels;

	internal class Point2d : ViewModelBase
	{
		private double _x, _y;

		public Point2d(double x, double y)
		{
			X = x;
			Y = y;
		}

		public Point2d(Point p)
		{
			X = p.X;
			Y = p.Y;
		}

		public double X
		{
			get { return _x; }
			set
			{
				if (_x == value) return;
				_x = value;
				OnPropertyChanged("X");
			}
		}

		public double Y
		{
			get { return _y; }
			set
			{
				if (_y == value) return;
				_y = value;
				OnPropertyChanged("Y");
			}
		}
	}
}
