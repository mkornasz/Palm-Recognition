using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalmRecognizer
{
    class LogWriter
    {
        public void AddRotationInfo()
        {
            //czy byl poprawnie obrocony, czy ekspert obracal
            //moze kąt obrotu?
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
            //moze kilka pozycji ustawionych wg % podobienstwa
        }
    }
}
