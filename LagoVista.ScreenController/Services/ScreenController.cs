using LagoVista.Core.Networking.Interfaces;
using LagoVista.Core.Networking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace LagoVista.ScreenController.Services
{
    public class ScreenManager : IApiHandler
    {
        private const int PIN_DOWN = 26;
        private const int PIN_STOP = 20;
        private const int PIN_UP = 21;

        private GpioPin _pinUp;
        private GpioPin _pinStop;
        private GpioPin _pinDown;

        private static ScreenManager _instance = new ScreenManager();

        public void Init()
        {
            var gpio = GpioController.GetDefault();

            _pinUp = gpio.OpenPin(PIN_UP);
            _pinStop = gpio.OpenPin(PIN_STOP);
            _pinDown = gpio.OpenPin(PIN_DOWN);

            _pinUp.SetDriveMode(GpioPinDriveMode.Output); _pinUp.Write(GpioPinValue.High);
            _pinStop.SetDriveMode(GpioPinDriveMode.Output); _pinStop.Write(GpioPinValue.High);
            _pinDown.SetDriveMode(GpioPinDriveMode.Output); _pinDown.Write(GpioPinValue.High);
        }


        public static ScreenManager Instance
        {
            get { return _instance; }
        }

        [MethodHandler(MethodHandlerAttribute.MethodTypes.GET, FullPath = "/screen/up")]
        public HttpResponseMessage Up(HttpRequestMessage msg)
        {
            Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"SHOULD GO UP {PIN_UP}");
                _pinUp.Write(GpioPinValue.Low);
                await Task.Delay(3000);
                _pinUp.Write(GpioPinValue.High);
            });

            return msg.GetResponseMessage();
        }

        [MethodHandler(MethodHandlerAttribute.MethodTypes.GET, FullPath = "/screen/down")]
        public HttpResponseMessage Down(HttpRequestMessage msg)
        {
            Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"screen down {PIN_DOWN}");
                _pinDown.Write(GpioPinValue.Low);
                await Task.Delay(3000);
                _pinDown.Write(GpioPinValue.High);
            });

            return msg.GetResponseMessage();
        }

        [MethodHandler(MethodHandlerAttribute.MethodTypes.GET, FullPath = "/screen/stop")]
        public HttpResponseMessage Stop(HttpRequestMessage msg)
        {
            Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"SHOULD STOP {PIN_STOP}");
                _pinStop.Write(GpioPinValue.Low);
                await Task.Delay(3000);
                _pinStop.Write(GpioPinValue.High);
            });

            return msg.GetResponseMessage();
        }
    }
}
