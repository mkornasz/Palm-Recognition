namespace PalmRecognizer.ViewModels
{
	using System;
	using System.Collections;
	using System.Collections.ObjectModel;
	using System.Drawing;

	using Emgu.CV;
	using Emgu.CV.Structure;

	using PalmRecognizer.Helpers;
	using PalmRecognizer.Model;

	class PalmTool
	{
		private Image<Bgr, Byte> _palmOriginalImage, _palmEqualizedHist, _newImage;
		private Mat _palmOriginal, _palmGray, _palmBlur, _palmEdges, _palmContour, _palmBw;

		private MeasurementDetector _measurementDetector;

		#region Properties

		public int CannyParamLow { get; set; }

		public int CannyParamHigh { get; set; }

		public int BrightnessParam { get; set; }

		public double ContrastParam { get; set; }

		public Bitmap GetBlurPalmBitmap { get { return this._palmBlur.Bitmap; } }

		public Bitmap GetGrayPalmBitmap { get { return this._palmGray.Bitmap; } }

		public Bitmap GetEdgesPalmBitmap { get { return this._palmEdges.Bitmap; } }

		public Bitmap GetContourPalmBitmap { get { return this._palmContour.Bitmap; } }

		public Bitmap GetBwPalmBitmap { get { return this._palmBw.Bitmap; } }

		public DatabaseConnection.PalmParameters MeasuredParameters { get; private set; }

		public ObservableCollection<Defect> Defects { get; set; }

		#endregion

		public PalmTool(string palmFilename, int cannyHigh, int cannyLow, double contrast, int brightness)
		{
			this.CannyParamHigh = cannyHigh;
			this.CannyParamLow = cannyLow;
			this.ContrastParam = contrast;
			this.BrightnessParam = brightness;
			this._palmOriginal = new Mat(palmFilename, Emgu.CV.CvEnum.LoadImageType.Color);
			this._newImage = this._palmOriginal.ToImage<Bgr, Byte>();
			this._palmOriginalImage = this._palmOriginal.ToImage<Bgr, Byte>();
			this._palmBw = new Mat();
			this._palmContour = new Mat();
			this._palmGray = new Mat();
			this._palmEdges = new Mat();
			this._palmBlur = new Mat();
			this.Defects = new ObservableCollection<Defect>();
		}

		public Bitmap ChangeContrastBrightness()
		{
			this._newImage = this._palmOriginalImage.Convert(b => this.SaturateCast(this.ContrastParam * b + this.BrightnessParam));
			return this._newImage.Mat.Bitmap;
		}

		private byte SaturateCast(double value)
		{
			var rounded = Math.Round(value, 0);
			return rounded < byte.MinValue ? byte.MinValue : rounded > byte.MaxValue ? byte.MaxValue : (byte)rounded;
		}

		public void DetectEdges()
		{
			this.ChangeContrastBrightness();
			CvInvoke.CvtColor(this._newImage.Mat, this._palmGray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
			CvInvoke.Threshold(this._palmGray, this._palmBw, 50, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
			CvInvoke.GaussianBlur(this._palmBw, this._palmBlur, new Size(5, 5), 0);

			this.OpenCvCannyDetector(this._palmBlur);
		}

		public void GetDefects()
		{
			_measurementDetector = new MeasurementDetector(this._palmEdges);
			_palmContour = _measurementDetector.GetDefects();
			Defects = _measurementDetector.Defects;
		}

		public Bitmap EqualizeHistogram()
		{
			Mat imageGray = new Mat();
			Mat imageEqualizeHist = new Mat();
			CvInvoke.CvtColor(_newImage.Mat, imageGray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
			CvInvoke.EqualizeHist(imageGray, imageEqualizeHist);
			_palmEqualizedHist = imageEqualizeHist.ToImage<Bgr, Byte>();
			return this._palmEqualizedHist.Mat.Bitmap;
		}

		public Bitmap UNDOHistogramEqualization()
		{
			this._palmEqualizedHist = this._palmOriginalImage;
			this.ChangeContrastBrightness();
			return this._newImage.Mat.Bitmap;
		}

		private void OpenCvCannyDetector(Mat matToFindEdges)
		{
			this._palmEdges = new Mat();
			CvInvoke.Canny(matToFindEdges, this._palmEdges, this.CannyParamLow, this.CannyParamHigh);
		}

		#region Methods for own Canny

		private void OwnCannyDetector()
		{
			this._palmEdges = new Mat();
			Mat grad_x = new Mat();
			Mat grad_y = new Mat();
			CvInvoke.Sobel(this._palmBlur, grad_x, Emgu.CV.CvEnum.DepthType.Cv16S, 1, 0, 3);
			CvInvoke.Sobel(this._palmBlur, grad_y, Emgu.CV.CvEnum.DepthType.Cv16S, 0, 1, 3);

			Mat grad, theta;

			this.CalculateGradTheta(grad_x, grad_y, out grad, out theta);

			Mat grad_border = new Mat(grad.Rows + 2, grad.Cols + 2, grad.Depth, 1);
			CvInvoke.CopyMakeBorder(grad, grad_border, 1, 1, 1, 1, Emgu.CV.CvEnum.BorderType.Replicate);

			this.CalculateBorder(ref grad, theta, grad_border);

			this._palmEdges = this.Hysteresis(250, 100, grad, theta);
			var bmp = this._palmEdges.Bitmap;
		}

		private Mat Hysteresis(double lowT, double highT, Mat gradValue, Mat thetaValue)
		{
			Stack pointsStack = new Stack();

			for (int i = 0; i < gradValue.Cols; i++)
				for (int j = 0; j < gradValue.Rows; j++)
				{
					if (gradValue.GetValue(j, i) > highT)
						pointsStack.Push(new Point(j, i));
				}

			Mat grad_border = new Mat(gradValue.Rows + 2, gradValue.Cols + 2, Emgu.CV.CvEnum.DepthType.Cv8U, 1);

			for (int i = 0; i < grad_border.Cols; i++)
				for (int j = 0; j < grad_border.Rows; j++)
					this.SetMatValue(j, i, ref grad_border, 0);

			while (pointsStack.Count != 0)
			{
				Point p = (Point)pointsStack.Pop();

				if (grad_border.GetValue(p.X, p.Y) == 255)
					continue;

				this.SetMatValue(p.X, p.Y, ref grad_border, 255);
				Point a, b;
				CheckNeighbors(thetaValue.GetValue(p.X, p.Y), out a, out b);
				if (gradValue.GetValue(p.X + a.Y, p.Y + a.X) > lowT)
					pointsStack.Push(new Point(p.X + a.Y, p.Y + a.X));
				if (gradValue.GetValue(p.X - a.Y, p.Y - a.X) > lowT)
					pointsStack.Push(new Point(p.X - a.Y, p.Y - a.X));
			}

			return grad_border;
		}

		private void CalculateGradTheta(Mat grad_x, Mat grad_y, out Mat gradVal, out Mat thetaVal)
		{
			gradVal = new Mat(grad_x.Rows, grad_x.Cols, Emgu.CV.CvEnum.DepthType.Cv16S, 1);
			thetaVal = new Mat(grad_x.Rows, grad_x.Cols, Emgu.CV.CvEnum.DepthType.Cv16S, 1);

			for (int i = 0; i < gradVal.Cols; i++)
				for (int j = 0; j < gradVal.Rows; j++)
				{
					var valueGrad = Math.Sqrt(Math.Pow(grad_x.GetValue(j, i), 2) + Math.Pow(grad_y.GetValue(j, i), 2));
					var valueTheta = Math.Atan2(grad_y.GetValue(j, i), grad_x.GetValue(j, i));

					SetMatValue(j, i, ref gradVal, valueGrad);
					SetMatValue(j, i, ref thetaVal, valueTheta);
				}
		}

		private void CalculateBorder(ref Mat gradVal, Mat thetaVal, Mat grad_border)
		{
			Point m1, m2;
			for (int i = 0; i < gradVal.Cols; i++)
				for (int j = 0; j < gradVal.Rows; j++)
				{
					float w = CheckNeighbors(thetaVal.GetValue(j, i), out m1, out m2);
					float f1 = (grad_border.GetValue(j + 1 + m1.Y, i + 1 + m1.X) * w) + (grad_border.GetValue(j + 1 + m2.Y, i + 1 + m2.X) * (1 - w));
					float f2 = (grad_border.GetValue(j + 1 - m1.Y, i + 1 - m1.X) * w) + (grad_border.GetValue(j + 1 - m2.Y, i + 1 - m2.X) * (1 - w));

					if (gradVal.GetValue(j, i) < f1 || gradVal.GetValue(j, i) < f2)
						this.SetMatValue(j, i, ref gradVal, 0);
				}
		}

		private float CheckNeighbors(short angle, out Point m1, out Point m2)
		{
			if (angle >= 180)
				angle -= 180;
			int quarter = angle / 45;
			int weight = angle - quarter;

			switch (quarter)
			{
				case 0:
					m1 = new Point(1, 0);
					m2 = new Point(1, 1);
					break;
				case 1:
					m1 = new Point(1, 1);
					m2 = new Point(0, 1);
					break;
				case 2:
					m1 = new Point(0, 1);
					m2 = new Point(-1, 1);
					break;
				default:
					m1 = new Point(-1, 1);
					m2 = new Point(-1, 0);
					break;
			}
			return weight / 45.0f;
		}

		private void SetMatValue(int j, int i, ref Mat mat, double value)
		{
			mat.SetDoubleValue(j, i, value);
		}
		#endregion

		public void CalculateMeasurements(ObservableCollection<Defect> defects)
		{
			_palmContour = _measurementDetector.MeasureHand(defects);
		}
	}
}
