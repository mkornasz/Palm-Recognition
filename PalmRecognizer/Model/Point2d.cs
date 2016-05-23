namespace PalmRecognizer.Model
{
	using System;
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
		public double Length { get { return Math.Sqrt(_x * _x + _y * _y); } }

		public double LengthSquared { get { return _x * _x + _y * _y; } }

		public Point2d Normalized { get { return this / Math.Sqrt(_x * _x + _y * _y); } }

		public Point Point { get { return new Point((int)_x, (int)_y); } }

		public double Dot(Point2d vector)
		{
			return _x * vector.X + _y * vector.Y;
		}

		public static Point2d operator *(Point2d vector, double value)
		{
			return new Point2d(vector.X * value, vector.Y * value);
		}

		public static Point2d operator *(double value, Point2d vector)
		{
			return new Point2d(vector.X * value, vector.Y * value);
		}

		public static Point2d operator *(Point2d vector, int value)
		{
			return new Point2d(vector.X * value, vector.Y * value);
		}

		public static Point2d operator *(int value, Point2d vector)
		{
			return new Point2d(vector.X * value, vector.Y * value);
		}

		public static double operator *(Point2d vector, Point2d value)
		{
			return vector.X * value.X + vector.Y * value.Y;
		}

		public static Point2d operator /(Point2d vector, double value)
		{
			return new Point2d(vector.X / value, vector.Y / value);
		}

		public static Point2d operator +(Point2d vector, Point2d vector1)
		{
			return new Point2d(vector.X + vector1.X, vector.Y + vector1.Y);
		}

		public static Point2d operator -(Point2d vector, Point2d vector1)
		{
			return new Point2d(vector.X - vector1.X, vector.Y - vector1.Y);
		}
	}
}
