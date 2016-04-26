using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseConnection
{
    class Program
    {
        static void Main(string[] args)
        {
            IDatabaseConnection connection = Database.Instance;
            Image image = Image.FromFile(@"C:\Users\Kuba\Desktop\test.png");
            PalmParameters pp = new PalmParameters();
            pp.PalmRadius = 100;
            connection.AddNewData(image, "testImage", pp);
        } 
    }
}
