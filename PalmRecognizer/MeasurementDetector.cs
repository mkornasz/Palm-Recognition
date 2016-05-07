namespace PalmRecognizer
{
	using Emgu.CV;
	using Emgu.CV.CvEnum;
	using Emgu.CV.Structure;
	using Emgu.CV.Util;
	using System;
	using System.Drawing;

	internal class MeasurementDetector
	{
		private Mat _originalImg;
		private VectorOfPoint _fingerTips;

		public MeasurementDetector(Mat src)
		{
			_originalImg = src;
		}

		public Mat MeasureHand()
		{
			var scalar = new MCvScalar(0);
			var m = new Mat(_originalImg.Size, DepthType.Cv8U, 3);
			m.SetTo(scalar);

			using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
			{
				CvInvoke.FindContours(_originalImg, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

				var maxContourIndex = FindBiggestContour(contours);
				var maxContour = contours[maxContourIndex];

				var convexHullP = new VectorOfPoint();
				var convexHullI = new VectorOfInt();
				var approxContour = new VectorOfPoint();
				var defects = new VectorOfRect();

				CvInvoke.ConvexHull(maxContour, convexHullP, false, false);
				CvInvoke.ConvexHull(maxContour, convexHullI, false, false);
				CvInvoke.ApproxPolyDP(convexHullP, convexHullP, 18, true);

				if (maxContour.Size > 3)
				{
					CvInvoke.ConvexityDefects(maxContour, convexHullI, defects);
					defects = EleminateDefects(maxContour, defects);
				}

				if (maxContour.Size > 0)
				{
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

						double depth = defects[i].Height / 256;
						//display start points
						CvInvoke.Circle(m, ptStart, 5, new MCvScalar(255, 0, 0), 2);
						//display all end points
						CvInvoke.Circle(m, ptEnd, 5, new MCvScalar(255, 255, 0), 2);
						//display all far points
						CvInvoke.Circle(m, ptFar, 5, new MCvScalar(0, 0, 255), 2);
					}
				}



				//GetFingerTips(m, maxContour, defects);
				//var boundingBox = CvInvoke.BoundingRectangle(maxContour);

				//var isHand = DetectIfHand(boundingBox);
			}

			return m;
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

			double angleTol = 40;
			var newDefects = new VectorOfRect();
			foreach (var defect in defects.ToArray())
			{
				Point ptStart = contour[defect.X];
				Point ptEnd = contour[defect.Y];
				Point ptFar = contour[defect.Width];

				var distFar = Distance(ptStart, ptFar);
				var distEnd = Distance(ptFar, ptEnd);

				if (distFar > minTolerance && distEnd > minTolerance &&
					//distFar < maxTolerance && distEnd < maxTolerance && 
					Angle(ptStart, ptFar, ptEnd) < angleTol &&
					ptEnd.Y < (boundingBox.Y + boundingBox.Height - boundingBox.Height / 4) && ptStart.Y < (boundingBox.Y + boundingBox.Height - boundingBox.Height / 4))
				{
					newDefects.Push(new[] { defect });
				}
			}

			return newDefects;
			//return RemoveRedundantEndPoints(contour, newDefects, boundingBox.Width);
		}

		// remove endpoint of convexity defects if they are at the same fingertip
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
					if (Distance(ptStart, ptEnd2) < tolerance)
					{
						contour = Swap(contour, startidx, ptEnd2);
						break;
					}
					if (Distance(ptEnd, ptStart2) < tolerance)
					{
						contour = Swap(contour, startidx2, ptEnd);
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
			double l1 = Distance(f, s);
			double l2 = Distance(f, e);
			double dot = (s.X - f.X) * (e.X - f.X) + (s.Y - f.Y) * (e.Y - f.Y);
			double angle = Math.Acos(dot / (l1 * l2));
			angle = angle * 180 / Math.PI;
			return angle;
		}

		private void GetFingerTips(Mat m, VectorOfPoint contour, VectorOfRect defects)
		{
			_fingerTips = new VectorOfPoint();
			int i = 0;
			foreach (var d in defects.ToArray())
			{
				int startidx = d.X;
				Point ptStart = contour[startidx];

				int endidx = d.Y;
				Point ptEnd = contour[endidx];

				int faridx = d.Width;
				Point ptFar = contour[faridx];

				if (i == 0)
				{
					_fingerTips.Push(new[] { ptStart });
					i++;
				}
				_fingerTips.Push(new[] { ptEnd });
				i++;
			}

			RemoveRedundantFingerTips();
		}
		private void RemoveRedundantFingerTips()
		{
			var newFingers = new VectorOfPoint();
			for (int i = 0; i < _fingerTips.Size; i++)
			{
				for (int j = i; j < _fingerTips.Size; j++)
				{
					if (Distance(_fingerTips[i], _fingerTips[j]) < 50 && i != j)
					{
					}
					else
					{
						newFingers.Push(new[] { _fingerTips[i] });
						break;
					}
				}
			}
			_fingerTips = newFingers;
		}

		private bool DetectIfHand(Rectangle boundingBox)
		{
			double h = boundingBox.Height;
			double w = boundingBox.Width;
			bool isHand = true;

			if (_fingerTips.Size > 5)
			{
				isHand = false;
			}
			else if (h == 0 || w == 0)
			{
				isHand = false;
			}
			else if (h / w > 4 || w / h > 4)
			{
				isHand = false;
			}
			else if (boundingBox.X < 20)
			{
				isHand = false;
			}
			return isHand;
		}
	}
}
