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
            Palm palm = connection.GetAll().First();
            Form1 form = new Form1();
            form.pictureBox1.Image = connection.ByteArraytToImage(palm.Image);
            form.ShowDialog();
        } 
    }
}
