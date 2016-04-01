using Emgu.CV;
using System;
using System.Collections;
using System.Drawing;

namespace PalmRecognizer
{
    class PalmTool
    {
        private Mat _palmOriginal, _palmGray, _palmBlur, _palmEdges;
        private int _cannyParamLow, _cannyParamHigh;

        #region Properties
        public Bitmap GetBlurPalmBitmap { get { return _palmBlur.Bitmap; } }

        public Bitmap GetGrayPalmBitmap { get { return _palmGray.Bitmap; } }

        public Bitmap GetEdgesPalmBitmap { get { return _palmEdges.Bitmap; } }
        #endregion

        public PalmTool(string palmFilename, int cannyParamLow, int cannyParamHigh)
        {
            _cannyParamLow = cannyParamLow;
            _cannyParamHigh = cannyParamHigh;

            _palmOriginal = new Mat(palmFilename, Emgu.CV.CvEnum.LoadImageType.Color);
            _palmGray = new Mat(palmFilename, Emgu.CV.CvEnum.LoadImageType.Grayscale);

            _palmBlur = new Mat();
            CvInvoke.GaussianBlur(_palmGray, _palmBlur, new Size(5, 5), 0);

            //  OwnCannyDetector();
            OpenCVCannyDetector();
        }

        private void OpenCVCannyDetector()
        {
            _palmEdges = new Mat();
            CvInvoke.Canny(_palmBlur, _palmEdges, _cannyParamLow, _cannyParamHigh);
        }

        private void OwnCannyDetector()
        {
            _palmEdges = new Mat();
            Mat grad_x = new Mat();
            Mat grad_y = new Mat();
            CvInvoke.Sobel(_palmBlur, grad_x, Emgu.CV.CvEnum.DepthType.Cv16S, 1, 0, 3);
            CvInvoke.Sobel(_palmBlur, grad_y, Emgu.CV.CvEnum.DepthType.Cv16S, 0, 1, 3);

            Mat grad, theta;

            CalculateGradTheta(grad_x, grad_y, out grad, out theta);

            Mat grad_border = new Mat(grad.Rows + 2, grad.Cols + 2, grad.Depth, 1);
            CvInvoke.CopyMakeBorder(grad, grad_border, 1, 1, 1, 1, Emgu.CV.CvEnum.BorderType.Replicate);

            CalculateBorder(ref grad, theta, grad_border);

            _palmEdges = Hysteresis(250, 100, grad, theta);
            var bmp = _palmEdges.Bitmap;
        }

        #region Methods for own Canny
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
                    SetMatValue(j, i, ref grad_border, 0);

            while (pointsStack.Count != 0)
            {
                Point p = (Point)pointsStack.Pop();

                if (grad_border.GetValue(p.X, p.Y) == 255)
                    continue;

                SetMatValue(p.X, p.Y, ref grad_border, 255);
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
                        SetMatValue(j, i, ref gradVal, 0);
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
    }
}
