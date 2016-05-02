using System;
using System.IO;

namespace PalmRecognizer
{
    class LogWriter
    {
        public string LogContent { get; private set; }

        public LogWriter()
        {
            LogContent += DateTime.Now.ToString() + " log created."+ Environment.NewLine;
        }
        public void AddLogInInfo(string user)
        {
            LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " log in." + Environment.NewLine;
        }

        public void AddLogOutInfo(string user)
        {
            LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " log out." + Environment.NewLine;
        }

        public void AddLoadInfo(string user, string fileName)
        {
            LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " load file from location " + fileName + Environment.NewLine;
        }

        public void AddSaveInfo(string user, string fileName)
        {
            LogContent += Environment.NewLine + DateTime.Now.ToString() + " User " + user + " save file to location " + fileName + Environment.NewLine;
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

        public void AddRotationInfo()
        {
            //czy byl poprawnie obrocony, czy ekspert obracal
            //moze kąt obrotu?
        }

        public void AddCropInfo()
        {
            //czy byl przycinany
            //kto przycial
        }

        public void SaveLogFile()
        {
            string name = "PalmRecognizerLog" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            using (var streamWriter = new StreamWriter(name))
            {
                streamWriter.Write(LogContent);
            }
        }
    }
}
