using DatabaseConnection.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseConnection
{
    public class Database : IDatabaseConnection
    {
        private static Database instance = null;
        public static Database Instance 
        {
            get
            {
                if (instance == null)
                    instance = new Database();
                return instance;
            }
        }

        private Database() { }

        #region IDatabaseConnection
        public void AddNewData(Image palmImage, string description, PalmParameters parameters)
        {
            using (var db = new PalmContext())
            {
                Palm palm = new Palm() { Image = ImageToByteArray(palmImage), Description = description };
                palm.IndexFingerBot = parameters.IndexFingerBot;
                palm.IndexFingerMid = parameters.IndexFingerMid;
                palm.IndexFingerTop = parameters.IndexFingerTop;
                palm.MiddleFingerBot = parameters.MiddleFingerBot;
                palm.MiddleFingerMid = parameters.MiddleFingerMid;
                palm.MiddleFingerTop = parameters.MiddleFingerTop;
                palm.PinkyFingerBot = parameters.PinkyFingerBot;
                palm.PinkyFingerMid = parameters.PinkyFingerMid;
                palm.PinkyFingerTop = parameters.PinkyFingerTop;
                palm.RingFingerBot = parameters.RingFingerBot;
                palm.RingFingerMid = parameters.RingFingerMid;
                palm.RingFingerTop = parameters.RingFingerTop;
                palm.PalmRadius = parameters.PalmRadius;

                db.Palms.Add(palm);
                db.SaveChanges();
            }
        }

        public void RemoveData(int palmId)
        {
            using (var db = new PalmContext())
            {
                var palms = from p in db.Palms 
                            where p.PalmId == palmId
                            select p;

                db.Palms.Remove(palms.First());
                db.SaveChanges();
            }
        }

        public List<Model.Palm> Identify(PalmParameters parameters)
        {
            throw new NotImplementedException();
        }

        public void AddNewUser(string login, string password)
        {
            using (var db = new UserContext())
            {
                var user = new User { Login = login, Password = password };
                db.Users.Add(user);
                db.SaveChanges();
            }
        }

        public bool Login()
        {
            throw new NotImplementedException();
        }
        public Image ByteArraytToImage(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            return Image.FromStream(ms);
        }
        
        public List<Palm> GetAll()
        {
            List<Palm> result = new List<Palm>();
            using (var db = new PalmContext())
            {
                var palms = from p in db.Palms
                            orderby p.PalmId
                            select p;

                result = palms.ToList();
            }
            return result;
        }
        #endregion
        #region Private Methods
        public byte[] ImageToByteArray(Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Gif);
            return ms.ToArray();
        }
        #endregion
    }
}
