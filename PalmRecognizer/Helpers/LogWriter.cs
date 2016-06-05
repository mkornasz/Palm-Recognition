namespace PalmRecognizer.Helpers
{
    using System;
    using System.IO;

    class LogWriter
    {
        public string LogContent { get; private set; }

        public LogWriter()
        {
            this.LogContent += DateTime.Now.ToString() + " Log created." + Environment.NewLine;
        }

        public void AddCloseInfo()
        {
            this.LogContent += DateTime.Now.ToString() + " Application closed. Log saved.";
        }

        public void AddLogInInfo(string user)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " logged in." + Environment.NewLine;
        }

        public void AddLogOutInfo(string user)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " logged out." + Environment.NewLine;
        }

        public void AddLoadInfo(string user, string fileName)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " loaded file from location " + fileName + Environment.NewLine;
        }

        public void AddSaveInfo(string user, string type, string fileName)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " saved file of type " + type + " to location " + fileName + Environment.NewLine;
        }

        public void AddResetInfo(string user)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " reseted program." + Environment.NewLine + Environment.NewLine + Environment.NewLine;
        }

        public void AddRotationInfo(string user)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " rotated file by 1 degree" + Environment.NewLine;
        }

        public void AddCropInfo(string user, string fileName)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " cropped file, new name " + fileName + Environment.NewLine;
        }

        public void AddResizeInfo(string fileName)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " Program resized file automatically, new name " + fileName + Environment.NewLine;
        }

        public void AddPreprocessingInfo(string user, double contrast, double brightness)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " changes contrast/brightness of palm image. New value of contrast parameter: " + contrast.ToString() + ", brightness parameter: " + brightness.ToString() + Environment.NewLine;
        }

        public void AddEdgesDetectionInfo(string user, int lowParam, int highParam)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " detected edges on palm image. Canny low parameter:  " + lowParam.ToString() + " , high parameter: " + highParam.ToString() + Environment.NewLine;
        }

        public void AddPalmToBaseInfo(string user)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " add new palm to database." + Environment.NewLine;
        }

        public void RemovePalmFromBaseInfo(string user, int palmId)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " remove palm with id: " + palmId.ToString() + " from database." + Environment.NewLine;
        }

        public void AddPalmMetricsInfo(string user)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " Measured palm from image. Detected parameters:   " + Environment.NewLine;
            //dodac wszystkie wymiary????
        }
        public void AddSearchingInfo(string user)
        {
            this.LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " looked for palm in database." + Environment.NewLine;
            //dodac kilka pozycji ustawionych wg % podobienstwa????
        }

        public void SaveLogFile()
        {
            string name = "PalmRecognizerLog" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            using (var streamWriter = new StreamWriter(name))
            {
                streamWriter.Write(this.LogContent);
            }
        }
    }
}
