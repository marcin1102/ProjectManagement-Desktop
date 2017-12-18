using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectManagement.Infrastructure.Http
{
    public static class ResponseExtensions
    {
        public static void ToMessageBox(string content)
        {
            try
            {
                var jArray = JArray.Parse(content);
                MessageBox.Show($"ERROR! \n {string.Join("\n", jArray.Children()["errorMessage"].Values<string>())}");
            }
            catch (System.Exception ex)
            {
                var jsonContent = JObject.Parse(content);
                var properties = jsonContent.Properties().Where(x => x.Name == "errorMessage");
                if (properties.Any())
                    MessageBox.Show($"ERROR! \n {string.Join("\n", properties.Select(x => x.Value))}");
                else
                    MessageBox.Show($"ERROR! \n {string.Join("\n", jsonContent.Properties().Select(x => x.Value))}");
            }
        }
    }
}
