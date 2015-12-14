using MiInventario.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiInventario
{
    public static class Utils
    {
        public static string GetUsernamFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return "(no user)";
            }
            if (email.IndexOf("@") == -1 || (email.IndexOf("@") != email.LastIndexOf("@")))
            {
                return email;
            }

            return email.Split('@')[0];
        }
    }
}