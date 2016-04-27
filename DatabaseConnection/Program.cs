using DatabaseConnection.Model;
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
            try
            {
                bool au1 = connection.AddNewUser("aaa", "xyz");

                bool au2 = connection.AddNewUser("bbb", "bbb");
                Console.Write("A");
                bool su1 = connection.Login("aaa", "xxx");
                bool su2 = connection.Login("aaa", "bbb");
                bool su3 = connection.Login("aaa", "xyz");
                Console.ReadKey();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //Palm palm = connection.GetAll().First();
            //Form1 form = new Form1();
            //form.pictureBox1.Image = connection.ByteArraytToImage(palm.Image);
            //form.ShowDialog();
        } 
    }
}
