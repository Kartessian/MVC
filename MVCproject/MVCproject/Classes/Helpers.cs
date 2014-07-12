using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCproject
{
    public static class Helpers
    {

        public static string GenerateUniqueName()
        {
            Random random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string uniqueName = new string(
                    Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray()
                );

            return uniqueName;
        }

    }
}