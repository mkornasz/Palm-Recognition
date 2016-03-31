using System.Drawing;
using Emgu.CV;
using System.IO;
using System.Windows.Media.Imaging;

namespace PalmRecognizer
{
    class PalmTool
    {
        private Bitmap _palmOriginalBitmap, _palmEdgesBitmap;
        private Bitmap _palmGrayBitmap, _palmBlurBitmap, _palmGradientBitmap;
        private Mat _palmOriginal, _palmGray, _palmBlur, _palmGradient;
        public PalmTool(string palmFilename)
        {
            _palmOriginal = new Mat(palmFilename, Emgu.CV.CvEnum.LoadImageType.Color);
            _palmOriginalBitmap = _palmOriginal.Bitmap;

            _palmGray = new Mat(palmFilename, Emgu.CV.CvEnum.LoadImageType.Grayscale);
            _palmGrayBitmap = _palmGray.Bitmap;
        }


        private void CannyDetector()
        {
            
        }

        private Bitmap ConvertToBitmap(BitmapSource bitmapsource)
        {
            Bitmap resultBitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();

                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                resultBitmap = new System.Drawing.Bitmap(outStream);
            }
            return resultBitmap;
        }


    }
}
