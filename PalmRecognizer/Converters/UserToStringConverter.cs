using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PalmRecognizer.Converters
{
    class UserToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string userLogin = "";
            using (var db = new DatabaseConnection.Model.PalmContext())
            {
                userLogin = db.Users.Where(u => u.UserId == (int)value).First().Login;
            }
            return userLogin;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
