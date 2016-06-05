using System;
using System.Drawing;

namespace DatabaseConnection
{
    class Program
    {
        static void Main(string[] args)
        {
            IDatabaseConnection connection = Database.Instance;
            try
            {
                bool au1 = connection.AddNewUser("aaa", "xyz");
                bool au2 = connection.AddNewUser("bbb", "bbb");
                Console.Write("A");
                bool su1 = connection.Login("aaa", "xxx");
                bool su2 = connection.Login("aaa", "bbb");
                bool su3 = connection.Login("aaa", "xyz");
                Console.WriteLine(su1 + " " + su2 + " " + su3);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

           // Image image = Image.FromFile(@"C:\Users\Kuba\Desktop\test.png");
            //connection.AddNewData("aaa", DateTime.Now, image, "testtest", new PalmParameters() { PalmRadius = 100 });
            
            /*List<Palm> list = connection.GetAll();
            foreach (var elem in list)
                connection.RemoveData(elem.PalmId);*/
            
        } 
    }
}
