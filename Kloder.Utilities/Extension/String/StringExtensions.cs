using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Kloder.Utilities.Extension.String
{
    public static class StringExtensions
    {
        public static string GetDigists(this string item)
        {
            try
            {
                Regex rgx = new Regex(@"[^0-9]");
                var result = rgx.Replace(item, "");
                return result;
            }
            catch
            {
                return item;
            }
        }
        public static string ClearEmail(this string email)
        {
            Regex rgx = new Regex(@"[^a-zA-Z0-9\._\-@]");
            var str = rgx.Replace(email, "");

            return str;
        }

        public static string PhoneWithoutCode(this string phone)
        {
            phone = phone.GetDigists();

            return phone.Length >= 10 ? phone.Substring(phone.Length - 10) : phone;
        }
    }
}
