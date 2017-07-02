using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitchenKiosk.Services
{
    public class SmartThingsHubs
    {
        private List<SmartThingsHub> _hubs;
        private static SmartThingsHubs _instance = new SmartThingsHubs();

        private const string HUBS_JSON = "HUBS_JSON";

        DateTime? _lastPing;

        private bool _isInitialized = false;
        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        private SmartThingsHubs()
        {

        }

        public static SmartThingsHubs Instance { get { return _instance; } }

        public async Task InitAsync()
        {
            var json = await LagoVista.Core.PlatformSupport.Services.Storage.GetKVPAsync<String>(HUBS_JSON);
            if (String.IsNullOrEmpty(json))
                _hubs = new List<SmartThingsHub>();
            else
                _hubs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SmartThingsHub>>(json);

            _isInitialized = true;
        }

        public void Ping(String hubId)
        {
            var stHub = _hubs.Where(hub => hub.Id == hub.Id).FirstOrDefault();
            if (stHub != null)
                stHub.LastPing = DateTime.Now;

            _lastPing = DateTime.Now;
        }

        public DateTime? LastPing
        {
            get { return _lastPing; }
        }

        public async void Save()
        {
            await LagoVista.Core.PlatformSupport.Services.Storage.StoreKVP(HUBS_JSON, JsonConvert.SerializeObject(_hubs));
        }

        public void Prune()
        {

        }

        public List<SmartThingsHub> Hubs
        {
            get
            {
                if (_hubs == null)
                    throw new Exception("Must initialize SmartThingsHubs service prior to accessing it.");

                return _hubs;
            }
        }

        public async Task SendToHubsAsync(String content)
        {
            foreach (var stHub in _hubs)
                await stHub.SendAsync(content);
        }
    }
}
