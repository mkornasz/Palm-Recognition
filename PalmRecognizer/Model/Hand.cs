namespace PalmRecognizer.Model
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;

	using Emgu.CV;
	using Emgu.CV.Structure;
	using System.Windows;

	internal class Hand
	{
		#region Public Properties

		public List<Finger> Fingers { get; set; }

		#endregion Public Properties

		public Hand(Mat mat, List<Defect> defects)
		{
			Fingers = new List<Finger>();

			defects.Sort(SortByX);

			Fingers.Add(new Finger(mat, defects[0], defects[1]));
			Fingers.Add(new Finger(mat, defects[1], defects[2]));
			Fingers.Add(GetFinger(mat, defects, true));
			Fingers.Add(GetFinger(mat, defects, false));
		}

		private Finger GetFinger(Mat mat, List<Defect> defects, bool isLeft)
		{
			var zeroValue = new MCvScalar(0);
			var m = new Mat(mat.Size, mat.Depth, mat.NumberOfChannels);
			m.SetTo(zeroValue);

			var firstDefect = isLeft ? defects[0] : defects[2];
			var secondDefect = defects[1];

			var far0 = firstDefect.Far;
			var far1 = secondDefect.Far;

			var direction = new Vector(far0.X - far1.X, far0.Y - far1.Y);
			direction.Normalize();
			direction *= 10;

			var left = (far0 + new Point2d(direction.X * mat.Cols, direction.Y * mat.Rows)).Point;
			var right = new Point2d(far0.X * mat.Cols, far0.Y * mat.Rows).Point;

			CvInvoke.Line(m, left, right, new MCvScalar(255, 255, 255), 3);

			var matrix = new Mat();
			CvInvoke.BitwiseAnd(mat, m, matrix);

			var points = new List<Point2d>();
			var bitmap = matrix.Bitmap;

			for (int i = 0; i < matrix.Cols; i++)
				for (int j = 0; j < matrix.Rows; j++)
				{
					if (bitmap.GetPixel(i, j).ToArgb() != Color.Black.ToArgb() &&
						!points.Any(p => Math.Abs(p.X - i) < 10 && Math.Abs(p.Y - j) < 10) &&
						(Math.Abs(i - far0.X * mat.Cols) > 15 || Math.Abs(j - far0.Y * mat.Rows) > 15))
						points.Add(new Point2d(i, j));
				}

			points.Sort(SortByX);

			return new Finger(mat, firstDefect, points.First().Point, isLeft ? firstDefect.End : firstDefect.Start);
		}

		private static int SortByX(Point2d x, Point2d y)
		{
			return x.X < y.X ? -1 : (x.X > y.X ? 1 : 0);
		}

		private static int SortByX(Defect x, Defect y)
		{
			return x.Far.X < y.Far.X ? -1 : (x.Far.X > y.Far.X ? 1 : 0);
		}
	}
}
