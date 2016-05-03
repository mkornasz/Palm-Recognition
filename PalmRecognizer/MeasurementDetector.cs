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

		public MeasurementDetector(Mat src)
		{
			_originalImg = src;
			//GetAverages();
		}

		private void GetAverages()
		{
			throw new NotImplementedException();
		}

		public Mat MeasureHand()
		{
			//Mat downM = new Mat();
			//CvInvoke.PyrDown(_originalImg, downM);
			//CvInvoke.GaussianBlur(downM, downM, new Size(3, 3), 0, 0, BorderType.Constant);
			//CvInvoke.CvtColor(downM, downM, ColorConversion.Bgr2Hls);
			////CvInvoke.MedianBlur(downM, downM, 7);
			////ProduceBinaries(mat);
			//CvInvoke.CvtColor(downM, downM, ColorConversion.Hls2Bgr);
			//CvInvoke.PyrUp(_originalImg, _originalImg);

			var scalar = new MCvScalar(0);
			var m = new Mat(_originalImg.Size, _originalImg.Depth, _originalImg.NumberOfChannels);
			m.SetTo(scalar);
			var m1 = new Mat(_originalImg.Size, _originalImg.Depth, _originalImg.NumberOfChannels);
			m1.SetTo(scalar);

			using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
			{
				CvInvoke.FindContours(_originalImg, contours, null, RetrType.External, ChainApproxMethod.ChainApproxNone);

				var maxContour = contours[FindBiggestContour(contours)];
				//CvInvoke.Polylines(m, maxContour.ToArray(), false, new Bgr(Color.DarkBlue).MCvScalar, 1);
				//for (int i = 0; i < contours.Size; i++)
				//{
				//	using (VectorOfPoint contour = contours[i])
				//	using (VectorOfPoint approxContour = new VectorOfPoint())
				//	{
				//		//CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
				//		//if (CvInvoke.ContourArea(contour, false) > 10)
				//		CvInvoke.Polylines(m, contour.ToArray(), false, new Bgr(Color.DarkBlue).MCvScalar, 1);
				//	}
				//}


				var convexHullP = new VectorOfPoint();
				var convexHullI = new VectorOfInt();
				var approxContour = new VectorOfPoint();
				var defects = new VectorOfRect();

				CvInvoke.ConvexHull(maxContour, convexHullP, false, true);
				CvInvoke.ConvexHull(maxContour, convexHullI, false, false);

				CvInvoke.ApproxPolyDP(convexHullP, convexHullP, 18, true);
				//CvInvoke.Polylines(m, maxContour.ToArray(), false, new Bgr(Color.DarkBlue).MCvScalar, 1);
				if (maxContour.Size > 3)
				{
					CvInvoke.ConvexityDefects(maxContour, convexHullI, defects);
					//CvInvoke.Polylines(m, defects.ToArray(), false, new Bgr(Color.DarkBlue).MCvScalar, 1);
					var cont = EleminateDefects(maxContour, defects);
					CvInvoke.Polylines(m, maxContour.ToArray(), false, new Bgr(Color.DarkBlue).MCvScalar, 1);
				}
			}

			return m;
		}

		//private static void ProduceBinaries(Mat mat)
		//{
		//	MCvScalar lowerBound;
		//	MCvScalar upperBound;
		//	Mat foo;
		//	for (int i = 0; i < 7; i++)
		//	{
		//		normalizeColors(mat);
		//		lowerBound = new MCvScalar(avgColor[i][0] - c_lower[i][0], avgColor[i][1] - c_lower[i][1], avgColor[i][2] - c_lower[i][2]);
		//		upperBound = new MCvScalar(avgColor[i][0] + c_upper[i][0], avgColor[i][1] + c_upper[i][1], avgColor[i][2] + c_upper[i][2]);
		//		mat->bwList.push_back(Mat(mat->srcLR.rows, mat->srcLR.cols, CV_8U));
		//		inRange(mat->srcLR, lowerBound, upperBound, mat->bwList[i]);
		//	}
		//	mat->bwList[0].copyTo(mat->bw);
		//	for (int i = 1; i < 7; i++)
		//	{
		//		mat->bw += mat->bwList[i];
		//	}
		//	medianBlur(mat->bw, mat->bw, 7);
		//}

		private static int FindBiggestContour(VectorOfVectorOfPoint contours)
		{
			int indexOfBiggestContour = -1;
			int sizeOfBiggestContour = 0;
			for (int i = 0; i < contours.Size; i++)
			{
				if (contours[i].Size > sizeOfBiggestContour)
				{
					sizeOfBiggestContour = contours[i].Size;
					indexOfBiggestContour = i;
				}
			}
			return indexOfBiggestContour;
		}

		private static VectorOfPoint EleminateDefects(VectorOfPoint contour, VectorOfRect defects)
		{
			var boundingBox = CvInvoke.BoundingRectangle(contour);
			int tolerance = boundingBox.Height / 5;

			double angleTol = 95;
			VectorOfRect newDefects = new VectorOfRect();
			int startidx, endidx, faridx;
			foreach (var defect in defects.ToArray())
			{
				startidx = defect.X;
				Point ptStart = contour[startidx];

				endidx = defect.Y;
				Point ptEnd = contour[endidx];
				faridx = defect.Width;

				Point ptFar = contour[faridx];

				if (Distance(ptStart, ptFar) > tolerance && Distance(ptEnd, ptFar) > tolerance && Angle(ptStart, ptFar, ptEnd) < angleTol)
				{
					if (ptEnd.Y > (boundingBox.Y + boundingBox.Height - boundingBox.Height / 4))
					{
					}
					else if (ptStart.Y > (boundingBox.Y + boundingBox.Height - boundingBox.Height / 4))
					{
					}
					else
					{
						newDefects.Push(new[] { defect });
					}
				}
			}

			//nrOfDefects = newDefects.Size;

			defects = newDefects;
			return RemoveRedundantEndPoints(contour, defects, boundingBox.Width);
		}

		// remove endpoint of convexity defects if they are at the same fingertip
		private static VectorOfPoint RemoveRedundantEndPoints(VectorOfPoint contour, VectorOfRect defects, double width)
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

		private static VectorOfPoint Swap(VectorOfPoint contour, int startidx, Point ptEnd2)
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

		private static double Distance(Point p, Point v)
		{
			return Math.Sqrt(Math.Abs(Math.Pow(p.X - v.X, 2) + Math.Pow(p.Y - v.Y, 2)));
		}

		private static double Angle(Point s, Point f, Point e)
		{
			double l1 = Distance(f, s);
			double l2 = Distance(f, e);
			double dot = (s.X - f.X) * (e.X - f.X) + (s.Y - f.Y) * (e.Y - f.Y);
			double angle = Math.Acos(dot / (l1 * l2));
			angle = angle * 180 / Math.PI;
			return angle;
		}

		private static void GetFingerTips(Mat m, VectorOfPoint contour, VectorOfRect defects)
		{
			var fingerTips = new VectorOfPoint();
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
					fingerTips.Push(new[] { ptStart });
					i++;
				}
				fingerTips.Push(new[] { ptEnd });
				i++;
			}
			if (fingerTips.Size == 0)
			{
				//checkForOneFinger(m);
			}
		}

		private static bool DetectIfHand(Rectangle boundingBox)
		{
			double h = boundingBox.Height;
			double w = boundingBox.Width;
			bool isHand = true;

			//if (fingerTips.size() > 5)
			//{
			//	isHand = false;
			//}
			//else if (h == 0 || w == 0)
			//{
			//	isHand = false;
			//}
			//else if (h / w > 4 || w / h > 4)
			//{
			//	isHand = false;
			//}
			//else if (boundingBox.X < 20)
			//{
			//	isHand = false;
			//}
			return isHand;
		}
	}
}
