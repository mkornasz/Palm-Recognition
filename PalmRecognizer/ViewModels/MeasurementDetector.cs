namespace PalmRecognizer.ViewModels
{
	using System;
	using System.Collections.ObjectModel;
	using System.Drawing;
	using System.Windows;

	using Emgu.CV;
	using Emgu.CV.CvEnum;
	using Emgu.CV.Structure;
	using Emgu.CV.Util;

	using PalmRecognizer.Model;

	using Point = System.Drawing.Point;

	internal class MeasurementDetector : ViewModelBase
	{
		#region Private Members

		private Mat _originalImg;

		private VectorOfPoint _fingerTips;

		private ObservableCollection<Defect> _defects;

		private MCvScalar[] _colors =
		{
			new MCvScalar(255, 0, 0),
			new MCvScalar(255, 255, 0),
			new MCvScalar(0, 255, 0),
			new MCvScalar(0, 0, 255),
			new MCvScalar(255, 0, 255),
			new MCvScalar(0, 255, 255),
			new MCvScalar(255, 255, 255),
			new MCvScalar(255, 100, 0),
			new MCvScalar(255, 0, 100),
			new MCvScalar(100, 100, 100),
			new MCvScalar(100, 255, 0),
			new MCvScalar(100, 0, 255),
		};

		#endregion Private Members

		#region Public Properties

		public ObservableCollection<Defect> Defects
		{
			get
			{
				return this._defects;
			}
			set
			{
				this._defects = value;
				this.OnPropertyChanged("Defects");
			}
		}

		#endregion Public Properties

		#region Constructors

		public MeasurementDetector(Mat src)
		{
			this._originalImg = src;
			this.Defects = new ObservableCollection<Defect>();
		}

		#endregion Constructors

		#region Public Methods

		public Mat MeasureHand()
		{
			var zeroValue = new MCvScalar(0);
			var m = new Mat(this._originalImg.Size, DepthType.Cv8U, 3);
			m.SetTo(zeroValue);

			using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
			{
				var defects = new VectorOfRect();
				var isHand = this.GetImageDefects(m, contours, ref defects);
			}

			return m;
		}

		#endregion Public Methods

		#region Private Methods

		private bool GetImageDefects(Mat m, VectorOfVectorOfPoint contours, ref VectorOfRect defects)
		{
			CvInvoke.FindContours(this._originalImg, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

			var maxContourIndex = this.FindBiggestContour(contours);
			var maxContour = contours[maxContourIndex];

			var convexHullP = new VectorOfPoint();
			var convexHullI = new VectorOfInt();

			CvInvoke.ConvexHull(maxContour, convexHullP, false, false);
			CvInvoke.ConvexHull(maxContour, convexHullI, false, false);
			//CvInvoke.ApproxPolyDP(convexHullP, convexHullP, 18, true);

			if (maxContour.Size > 3)
			{
				CvInvoke.ConvexityDefects(maxContour, convexHullI, defects);
				defects = this.EleminateDefects(maxContour, defects);
				this.DrawDefects(m, contours, maxContourIndex, maxContour, convexHullP, defects);
			}

			var boundingBox = CvInvoke.BoundingRectangle(maxContour);
			return this.DetectIfHand(defects, boundingBox);
		}

		private int FindBiggestContour(VectorOfVectorOfPoint contours)
		{
			int indexOfBiggestContour = -1;
			double sizeOfBiggestContour = 0;
			for (int i = 0; i < contours.Size; i++)
			{
				var contourSize = CvInvoke.ContourArea(contours[i], false);
				if (contourSize > sizeOfBiggestContour)
				{
					sizeOfBiggestContour = contourSize;
					indexOfBiggestContour = i;
				}
			}

			return indexOfBiggestContour;
		}

		private VectorOfRect EleminateDefects(VectorOfPoint contour, VectorOfRect defects)
		{
			var boundingBox = CvInvoke.BoundingRectangle(contour);
			int minTolerance = boundingBox.Height / 5;
			int maxTolerance = boundingBox.Height / 2;

			double angleMinTol = 5;
			double angleMaxTol = 40;
			var newDefects = new VectorOfRect();
			foreach (var defect in defects.ToArray())
			{
				Point ptStart = contour[defect.X];
				Point ptEnd = contour[defect.Y];
				Point ptFar = contour[defect.Width];

				var distFar = this.Distance(ptStart, ptFar);
				var distEnd = this.Distance(ptFar, ptEnd);
				var angle = this.Angle(ptStart, ptFar, ptEnd);

				if (distFar > minTolerance && distEnd > minTolerance &&
					//distFar < maxTolerance && distEnd < maxTolerance && 
					angle < angleMaxTol && angle > angleMinTol &&
					ptEnd.Y < (boundingBox.Y + boundingBox.Height - boundingBox.Height / 4) && ptStart.Y < (boundingBox.Y + boundingBox.Height - boundingBox.Height / 4))
				{
					newDefects.Push(new[] { defect });
				}
			}

			return newDefects;
			//return RemoveRedundantEndPoints(contour, newDefects, boundingBox.Width);
		}

		private VectorOfPoint RemoveRedundantEndPoints(VectorOfPoint contour, VectorOfRect defects, double width)
		{
			double tolerance = width / 6;
			int startidx, endidx;
			int startidx2, endidx2;
			for (int i = 0; i < defects.Size; i++)
			{
				for (int j = i; j < defects.Size; j++)
				{
					startidx = defects[i].X;
					Point ptStart = contour[startidx];

					endidx = defects[i].Y;
					Point ptEnd = contour[endidx];

					startidx2 = defects[j].X;
					Point ptStart2 = contour[startidx2];

					endidx2 = defects[j].Y;
					Point ptEnd2 = contour[endidx2];
					if (this.Distance(ptStart, ptEnd2) < tolerance)
					{
						contour = this.Swap(contour, startidx, ptEnd2);
						break;
					}
					if (this.Distance(ptEnd, ptStart2) < tolerance)
					{
						contour = this.Swap(contour, startidx2, ptEnd);
					}
				}
			}

			return contour;
		}

		private VectorOfPoint Swap(VectorOfPoint contour, int startidx, Point ptEnd2)
		{
			var newContour = new VectorOfPoint();

			for (int i = 0; i < contour.Size; i++)
			{
				if (i != startidx)
					newContour.Push(new[] { contour[i] });
				else
					newContour.Push(new[] { ptEnd2 });
			}

			return newContour;
		}

		private double Distance(Point p, Point v)
		{
			return Math.Sqrt(Math.Abs(Math.Pow(p.X - v.X, 2) + Math.Pow(p.Y - v.Y, 2)));
		}

		private double Angle(Point s, Point f, Point e)
		{
			double l1 = this.Distance(f, s);
			double l2 = this.Distance(f, e);
			double dot = (s.X - f.X) * (e.X - f.X) + (s.Y - f.Y) * (e.Y - f.Y);
			double angle = Math.Acos(dot / (l1 * l2));
			angle = angle * 180 / Math.PI;
			return angle;
		}

		private bool DetectIfHand(VectorOfRect defects, Rectangle boundingBox)
		{
			double h = boundingBox.Height;
			double w = boundingBox.Width;

			if (defects.Size > 5
				|| h == 0 || w == 0
				|| h / w > 4 || w / h > 4
				|| boundingBox.X < 20)
			{
				MessageBox.Show("The provided photo doesn't present a hand");
				return false;
			}

			return true;
		}

		private void DrawDefects(Mat m, VectorOfVectorOfPoint contours, int maxContourIndex, VectorOfPoint maxContour, VectorOfPoint convexHullP, VectorOfRect defects)
		{
			var defectsList = new ObservableCollection<Defect>();
			CvInvoke.DrawContours(m, contours, maxContourIndex, new MCvScalar(0, 255, 0));
			CvInvoke.Polylines(m, convexHullP.ToArray(), true, new MCvScalar(0, 0, 255));


			for (int i = 0; i < defects.Size; i++)
			{
				int startIdx = defects[i].X;
				Point ptStart = maxContour[startIdx];

				int endIdx = defects[i].Y;
				Point ptEnd = maxContour[endIdx];

				int farIdx = defects[i].Width;
				Point ptFar = maxContour[farIdx];

				//CvInvoke.Circle(m, ptStart, 5, new MCvScalar(255, 0, 0), 2);
				//CvInvoke.Circle(m, ptEnd, 5, new MCvScalar(255, 255, 0), 2);
				//CvInvoke.Circle(m, ptFar, 5, new MCvScalar(0, 0, 255), 2);

				//CvInvoke.Line(m, ptFar, ptStart, this._colors[i]);
				//CvInvoke.Line(m, ptEnd, ptFar, this._colors[i]);

				defectsList.Add(new Defect(ptStart, ptEnd, ptFar, m.Cols, m.Rows));
			}

			this.Defects = defectsList;
		}

		#endregion Private Methods
	}
}
