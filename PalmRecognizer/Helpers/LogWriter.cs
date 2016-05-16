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

        public void AddEdgesDetectionInfo()
        {
            //stosowane parametry
            //czy automatyczne wystarczylo, czy byla ingerencja ze str eksperta
        }

        public void AddPalmMetricsInfo()
        {
            //wykryte dlugosci i szerokosci palcow
        }

        public void AddSearchingInfo()
        {
            //stosowana metoda porownywania
            //wyniki metody
            //kilka pozycji ustawionych wg % podobienstwa
        }

        public void AddPreprocessingInfo()
        {
            //czy zmianiono kontrast/jasnosc
            //moze nowe wartosci kontrastu/jasnosci
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
