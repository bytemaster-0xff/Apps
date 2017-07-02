using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KitchenKiosk.Services
{
    public class SmartThingsHub
    {
        public String Id { get; set; }
        public String IPAddress { get; set; }
        public String Port { get; set; }
        public DateTime LastPing { get; set; }


        public async Task<bool> SendAsync(String content)
        {
            try
            {
                var client = new HttpClient();
                Port = 39500.ToString();
                Debug.WriteLine("--------------------------------------------------------------");
                Debug.WriteLine(String.Format("Sending Notification To: {0} - {1}", IPAddress, Port));

                var messageContent = new StringContent(content);
                var response = await client.PostAsync(new Uri(String.Format("http://{0}:{1}", IPAddress, Port), UriKind.Absolute), messageContent);
                Debug.WriteLine("RESPONSE CODE: " + response.StatusCode);
                Debug.WriteLine("RESPONSE CONTENT: " + response.Content);
                Debug.WriteLine("--------------------------------------------------------------");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION: " + ex.Message);
                Debug.WriteLine("--------------------------------------------------------------");
                return false;
            }
        }
    }
}
