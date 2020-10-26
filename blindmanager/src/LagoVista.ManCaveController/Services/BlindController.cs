using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.ManCaveController.Services
{
    public class BlindController
    {
        private static BlindController _blindController = new BlindController();

        public static BlindController Instance
        {
            get { return _blindController; }
        }

        public async Task ChangeBlindState(int blindIdx, String action)
        {
            var uri = String.Format("http://slsys.homeip.net:9300/blind/{0}/{1}", blindIdx, action);

            var request = new HttpClient();
            request.DefaultRequestHeaders.Add("clientsecret", "{D9F7D7C8-D752-47B3-8C16-B4F61B004A2A}");
            await request.GetAsync(uri);
        }
    }
}
