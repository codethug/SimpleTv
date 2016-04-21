using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTv.Sdk.Http
{
    public static class CookieExtensions
    {
        public static void SetCookie(this CookieContainer cookies, Uri uri, string name, string value)
        {
            bool cookieHasBeenSet = false;
            // If the cookie name already exists in the container, update the existing cookie
            foreach (var item in cookies.GetCookies(uri))
            {
                var cookie = (Cookie)item;
                if (cookie.Name == name)
                {
                    cookie.Value = value;
                    cookieHasBeenSet = true;
                }
            }
            if (!cookieHasBeenSet)
            {
                // The cookie does not yet exist in our container, so add it
                cookies.Add(new Cookie(name, value, "/", uri.Host));
            }
        }
    }
}
