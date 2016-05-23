namespace PalmRecognizer.ViewModels
{
	using System;
	using System.Collections.ObjectModel;
	using System.Drawing;
	using System.Linq;
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

		private VectorOfPoint _contour;

		private Mat _m;

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

		public Hand Hand { get; set; }

		#endregion Public Properties

		#region Constructors

		public MeasurementDetector(Mat src)
		{
			this._originalImg = src;
			this.Defects = new ObservableCollection<Defect>();
		}

		#endregion Constructors

		#region Public Methods

		public Mat GetDefects()
		{
			var zeroValue = new MCvScalar(0);
			_m = new Mat(_originalImg.Size, DepthType.Cv8U, 3);
			_m.SetTo(zeroValue);

			var isHand = GetImageDefects(_m);

			return _m;
		}

		public Mat MeasureHand(ObservableCollection<Defect> defects)
		{
			if (defects.Count < 3)
			{
				throw new Exception("It's not a hand.");
			}

			Hand =  new Hand(_m, defects.ToList());
			return _m;
		}

		#endregion Public Methods

		#region Private Methods

		private bool GetImageDefects(Mat m)
		{
			var defects = new VectorOfRect();

			using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
			{
				CvInvoke.FindContours(_originalImg, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

				var maxContourIndex = FindBiggestContour(contours);
				_contour = new VectorOfPoint(contours[maxContourIndex].ToArray());

				var convexHullP = new VectorOfPoint();
				var convexHullI = new VectorOfInt();

				CvInvoke.ConvexHull(_contour, convexHullP, false, false);
				CvInvoke.ConvexHull(_contour, convexHullI, false, false);

				if (_contour.Size > 3)
				{
					CvInvoke.ConvexityDefects(_contour, convexHullI, defects);
					defects = EleminateDefects(defects);
					DrawDefects(m, contours, maxContourIndex, convexHullP, defects);
				}
			}

			var boundingBox = CvInvoke.BoundingRectangle(_contour);
			return DetectIfHand(defects, boundingBox);
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

		private VectorOfRect EleminateDefects(VectorOfRect defects)
		{
			var boundingBox = CvInvoke.BoundingRectangle(_contour);
			int minTolerance = boundingBox.Height / 5;
			int maxTolerance = boundingBox.Height / 2;

			double angleMinTol = 5;
			double angleMaxTol = 40;
			var newDefects = new VectorOfRect();
			foreach (var defect in defects.ToArray())
			{
				Point ptStart = _contour[defect.X];
				Point ptEnd = _contour[defect.Y];
				Point ptFar = _contour[defect.Width];

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

		private void DrawDefects(Mat m, VectorOfVectorOfPoint contours, int maxContourIndex, VectorOfPoint convexHullP, VectorOfRect defects)
		{
			var defectsList = new ObservableCollection<Defect>();
			CvInvoke.DrawContours(m, contours, maxContourIndex, new MCvScalar(0, 255, 0));
			CvInvoke.Polylines(m, convexHullP.ToArray(), true, new MCvScalar(0, 0, 255));


			for (int i = 0; i < defects.Size; i++)
			{
				int startIdx = defects[i].X;
				Point ptStart = _contour[startIdx];

				int endIdx = defects[i].Y;
				Point ptEnd = _contour[endIdx];

				int farIdx = defects[i].Width;
				Point ptFar = _contour[farIdx];

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
