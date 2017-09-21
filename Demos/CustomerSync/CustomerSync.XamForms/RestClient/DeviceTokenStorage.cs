using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CustomerSync
{
    public class DeviceToken
    {
        const string tokenKey = "token";

        public static string Token
        {
            get
            {
                if (!Application.Current.Properties.ContainsKey(tokenKey))
                {
                    var token = Guid.NewGuid().ToString();
                    Application.Current.Properties[tokenKey] = token;
                    Application.Current.SavePropertiesAsync();

                    return token;
                }

                return Application.Current.Properties[tokenKey].ToString();
            }
            set
            {
                Application.Current.Properties[tokenKey] = value;
                Application.Current.SavePropertiesAsync();
            }
        }
    }
}
