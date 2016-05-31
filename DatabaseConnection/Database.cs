using DatabaseConnection.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

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
        private void ClearAllData()
        {
            using (var db = new PalmContext())
            {
                foreach (var p in db.PalmImages)
                    db.PalmImages.Remove(p);

                foreach (var p in db.Palms)
                    db.Palms.Remove(p);

                db.SaveChanges();
            }
        }

        public void AddNewData(string user, DateTime dateTime, Image palmImage, string description, PalmParameters parameters)
        {
            int userId = -1;
            using (var db = new PalmContext())
            {
                userId = db.Users.Where(u => u.Login == user).First().UserId;

                Palm palm = new Palm() { Description = description, UserId = userId, Date = dateTime };
                palm.IndexFingerBot = parameters.IndexFingerBot;
                palm.IndexFingerMid = parameters.IndexFingerMid;
                palm.IndexFingerTop = parameters.IndexFingerTop;
                palm.IndexFingerLength = parameters.IndexFingerLength;
                palm.MiddleFingerBot = parameters.MiddleFingerBot;
                palm.MiddleFingerMid = parameters.MiddleFingerMid;
                palm.MiddleFingerTop = parameters.MiddleFingerTop;
                palm.MiddleFingerLength = parameters.MiddleFingerLength;
                palm.PinkyFingerBot = parameters.PinkyFingerBot;
                palm.PinkyFingerMid = parameters.PinkyFingerMid;
                palm.PinkyFingerTop = parameters.PinkyFingerTop;
                palm.PinkyFingerLength = parameters.PinkyFingerLength;
                palm.RingFingerBot = parameters.RingFingerBot;
                palm.RingFingerMid = parameters.RingFingerMid;
                palm.RingFingerTop = parameters.RingFingerTop;
                palm.RingFingerLength = parameters.RingFingerLength;
                palm.PalmRadius = parameters.PalmRadius;

                db.Palms.Add(palm);
                db.SaveChanges();

                PalmImage image = new PalmImage() { PalmId = palm.PalmId, Image = ImageToByteArray(palmImage) };
                db.PalmImages.Add(image);
                db.SaveChanges();

                palm.Image = image;
                db.SaveChanges();
            }
        }

        public void AddNewData(string user, DateTime dateTime, Image palmImage, string description, PalmParameters parameters, Image defectsImage)
        {
            int userId = -1;
            using (var db = new PalmContext())
            {
                userId = db.Users.Where(u => u.Login == user).First().UserId;

                Palm palm = new Palm() { Description = description, UserId = userId, Date = dateTime };
                palm.IndexFingerBot = parameters.IndexFingerBot;
                palm.IndexFingerMid = parameters.IndexFingerMid;
                palm.IndexFingerTop = parameters.IndexFingerTop;
                palm.IndexFingerLength = parameters.IndexFingerLength;
                palm.MiddleFingerBot = parameters.MiddleFingerBot;
                palm.MiddleFingerMid = parameters.MiddleFingerMid;
                palm.MiddleFingerTop = parameters.MiddleFingerTop;
                palm.MiddleFingerLength = parameters.MiddleFingerLength;
                palm.PinkyFingerBot = parameters.PinkyFingerBot;
                palm.PinkyFingerMid = parameters.PinkyFingerMid;
                palm.PinkyFingerTop = parameters.PinkyFingerTop;
                palm.PinkyFingerLength = parameters.PinkyFingerLength;
                palm.RingFingerBot = parameters.RingFingerBot;
                palm.RingFingerMid = parameters.RingFingerMid;
                palm.RingFingerTop = parameters.RingFingerTop;
                palm.RingFingerLength = parameters.RingFingerLength;
                palm.PalmRadius = parameters.PalmRadius;

                db.Palms.Add(palm);
                db.SaveChanges();

                PalmImage image = new PalmImage() { PalmId = palm.PalmId, Image = ImageToByteArray(palmImage), DefectsImage = ImageToByteArray(defectsImage) };
                db.PalmImages.Add(image);
                db.SaveChanges();

                palm.Image = image;
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

                db.PalmImages.RemoveRange(db.PalmImages.Where(pi => palms.Any(p => p.PalmId == pi.PalmId)));
                db.Palms.Remove(palms.First());
                db.SaveChanges();
            }
        }

        public List<Model.Palm> Identify(PalmParameters parameters)
        {
            List<Model.Palm> result = new List<Palm>();
            using (var db = new PalmContext())
            {
                var palms = (from p in db.Palms
                            select new
                            {
                                p,
                                score = Metrics.EuclideanDistance(PalmParametersToArray(parameters), PalmToArray(p))
                            }).OrderBy(x => x.score).Take(5).Select(x => x.p).ToList();
                result = palms;
            }

            return result;
        }

        public bool AddNewUser(string login, string password) // zmienić, żeby login był unikalny
        {
            using (var db = new PalmContext())
            {
                var userLogin = db.Users.FirstOrDefault(u => u.Login == login);
                if (userLogin != null)
                    return false;

                var salt = PasswordEncoder.GeneratePassword(10);
                var encodedPassword = PasswordEncoder.EncodePassword(password, salt);
                var user = new User { Login = login, Password = encodedPassword, Salt = salt };
                db.Users.Add(user);
                db.SaveChanges();
            }
            return true;
        }

        public bool Login(string login, string password)
        {
            using (var db = new PalmContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == login);
                if (user != null) // zgodność loginu
                {
                    var hashCode = user.Salt;
                    var encodingPasswordString = PasswordEncoder.EncodePassword(password, hashCode);
                    var userLP = db.Users.FirstOrDefault(u => u.Login == login && u.Password == encodingPasswordString);
                    return userLP != null;
                }
            }
            return false;
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

        public List<PalmImage> GetAllImages()
        {
            List<PalmImage> result = new List<PalmImage>();
            using (var db = new PalmContext())
            {
                var palms = from p in db.PalmImages
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

        private double[] PalmParametersToArray(PalmParameters palmParameters)
        {
            return new double[] { palmParameters.PalmRadius, palmParameters.IndexFingerBot, palmParameters.IndexFingerMid, palmParameters.IndexFingerTop, palmParameters.IndexFingerLength, 
                palmParameters.MiddleFingerBot, palmParameters.MiddleFingerMid, palmParameters.MiddleFingerTop, palmParameters.MiddleFingerLength,
                palmParameters.RingFingerBot, palmParameters.RingFingerMid, palmParameters.RingFingerTop, palmParameters.RingFingerLength,
                palmParameters.PinkyFingerBot, palmParameters.PinkyFingerMid, palmParameters.PinkyFingerTop, palmParameters.PinkyFingerLength};
        }

        private double[] PalmToArray(Palm palm)
        {
            return new double[] { palm.PalmRadius, palm.IndexFingerBot, palm.IndexFingerMid, palm.IndexFingerTop, palm.IndexFingerLength, 
                palm.MiddleFingerBot, palm.MiddleFingerMid, palm.MiddleFingerTop, palm.MiddleFingerLength,
                palm.RingFingerBot, palm.RingFingerMid, palm.RingFingerTop, palm.RingFingerLength,
                palm.PinkyFingerBot, palm.PinkyFingerMid, palm.PinkyFingerTop, palm.PinkyFingerLength};
        }
        #endregion
    }
}
