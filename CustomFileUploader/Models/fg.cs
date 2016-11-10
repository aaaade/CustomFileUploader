using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jQuery_File_Upload.MVC5.Models
{
    public static class fg
    {
        public static string GenerateUniqueCode()
        {
            return DateTime.Now.ToString("ddMMMyyyyhhmmss");
        }
    }
}