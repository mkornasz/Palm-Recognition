using DatabaseConnection.Model;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace DatabaseConnection
{
    public interface IDatabaseConnection
    {
        bool Login(string login, string password);
        bool AddNewUser(string login, string password);
        void AddNewData(string user, DateTime dateTime, Image palmImage, string description, PalmParameters parameters);
        void AddNewData(string user, DateTime dateTime, Image palmImage, string description, PalmParameters parameters, Image defectsImage);
        void RemoveData(int palmId);
        List<PalmImage> GetAllImages();
        List<Palm> GetAll();
        List<Tuple<PalmImage, double, double>> Identify(PalmParameters parameters, int maxResults, MetricType metricType);
        Image ByteArraytToImage(byte[] data);
    }
}
