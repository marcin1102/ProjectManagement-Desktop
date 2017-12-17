using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.Infrastructure.String
{
    public static class StringExtensions
    {
        public static string RemoveBackslashesFromJson(this string json)
        {
            return json.Replace(@"\", "");
        }
    }
}
