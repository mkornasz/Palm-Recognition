using DatabaseConnection.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseConnection
{
    public interface IDatabaseConnection
    {
        bool Login(string login, string password);
        bool AddNewUser(string login, string password);
        void AddNewData(Image palmImage, string description, PalmParameters parameters);
        void RemoveData(int palmId);
        List<Palm> GetAll();
        List<Palm> Identify(PalmParameters parameters);
        Image ByteArraytToImage(byte[] data);
    }
}
