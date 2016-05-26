namespace PalmRecognizer.Model
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Windows;

	using Emgu.CV;
	using Emgu.CV.CvEnum;
	using Emgu.CV.Util;
	using System.Runtime.InteropServices;

	using Emgu.CV.Structure;

	using PalmRecognizer.Helpers;

	using Point = System.Drawing.Point;

	internal class Finger
	{
		#region Public Properties

		public Defect FirstDefect { get; }

		public Defect SecondDefect { get; }

		public double Height { get; private set; }

		public double Diameter { get; private set; }

		#endregion Public Properties

		#region Constructors

		public Finger(Mat mat, Defect first, Defect second)
		{
			FirstDefect = first;
			SecondDefect = second;

			CalculateMeasurements(mat);
		}

		public Finger(Mat mat, Defect first, Point second, Point2d common)
		{
			var s = new Defect(common, common, second, mat.Cols, mat.Rows);
			FirstDefect = first;
			SecondDefect = s;

			CalculateMeasurements(mat);
		}

		#endregion Constructors

		#region Private Methods

		private void CalculateMeasurements(Mat mat)
		{
			var bottom = SecondDefect.Far + ((FirstDefect.Far - SecondDefect.Far) / 2.0);
			var bottomPoint = new Point2d(bottom.X * mat.Cols, bottom.Y * mat.Rows);

			Point2d commonPoint = GetCommonPoint();
			var commonPointPt = new Point2d(commonPoint.X * mat.Cols, commonPoint.Y * mat.Rows);

			var perpendicular = commonPointPt - bottomPoint;
			Height = perpendicular.Length;

			Diameter = CalculateDiameter(mat, commonPointPt.Point, bottomPoint.Point);

			CvInvoke.Line(mat, commonPointPt.Point, bottomPoint.Point, new MCvScalar(255, 255, 255));
		}

		private double CalculateDiameter(Mat mat, Point top, Point bottom)
		{
			var direction = new Point2d(bottom.X - top.X, bottom.Y - top.Y);
			var middlePoint = new Point2d(top.X + (direction.X / 2.0), top.Y + (direction.Y / 2.0));

			var p4 = new Point2d(-direction.Y, direction.X);

			var vector = new Vector(p4.X, p4.Y);
			vector.Normalize();
			vector *= 0.1;
			var left = (middlePoint - new Point2d(vector.X * mat.Cols, vector.Y * mat.Rows)).Point;
			var right = (middlePoint + new Point2d(vector.X * mat.Cols, vector.Y * mat.Rows)).Point;

			var zeroValue = new MCvScalar(0);
			var m = new Mat(mat.Size, mat.Depth, mat.NumberOfChannels);
			m.SetTo(zeroValue);

			CvInvoke.Line(m, left, right, new MCvScalar(255, 255, 255), 3);

			var matrix = new Mat();
			CvInvoke.BitwiseAnd(mat, m, matrix);

			var points = new List<Point2d>();
			var bitmap = matrix.Bitmap;

			for (int i = 0; i < matrix.Cols; i++)
				for (int j = 0; j < matrix.Rows; j++)
				{
					if (bitmap.GetPixel(i, j).ToArgb() != Color.Black.ToArgb() && !points.Any(p => Math.Abs(p.X - i) < 10 && Math.Abs(p.Y - j) < 10))
						points.Add(new Point2d(i, j));
				}

			points.Sort((x, y) => (x - middlePoint).Length < (y - middlePoint).Length ? -1 : ((x - middlePoint).Length > (y - middlePoint).Length) ? 1 : 0);

			CvInvoke.Line(mat, points[0].Point, points[1].Point, new MCvScalar(255, 255, 255));

			return (points[0] - points[1]).Length;
		}

		private Point2d GetCommonPoint()
		{
			if ((FirstDefect.Start - SecondDefect.End).Length < (FirstDefect.End - SecondDefect.Start).Length)
				return SecondDefect.End + ((FirstDefect.Start - SecondDefect.End) / 2.0);
			return SecondDefect.Start + ((FirstDefect.End - SecondDefect.Start) / 2.0);
		}

		#endregion Private Methods
	}
}
