using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace KitchenKiosk.Managers
{
    public class ScreenManager
    {
        private const int PIN_UP = 26;
        private const int PIN_STOP = 13;
        private const int PIN_DOWN = 6;

        private GpioPin _pinUp;
        private GpioPin _pinStop;
        private GpioPin _pinDown;

        private static ScreenManager _instance = new ScreenManager();

        public Task InitAsync(ILogger logger)
        {
            var gpio = GpioController.GetDefault();

            _pinUp = gpio.OpenPin(PIN_UP);
            _pinStop = gpio.OpenPin(PIN_STOP);
            _pinDown = gpio.OpenPin(PIN_DOWN);

            _pinUp.SetDriveMode(GpioPinDriveMode.Output);
            _pinStop.SetDriveMode(GpioPinDriveMode.Output);
            _pinDown.SetDriveMode(GpioPinDriveMode.Output);

            return Task.CompletedTask;
        }


        public static ScreenManager Instance
        {
            get { return _instance; }
        }

        public async void Up()
        {
            _pinUp.Write(GpioPinValue.Low);
            await Task.Delay(3000);
            _pinUp.Write(GpioPinValue.High);
        }

        public async void Down()
        {
            _pinDown.Write(GpioPinValue.Low);
            await Task.Delay(3000);
            _pinDown.Write(GpioPinValue.High);
        }

        public async void Stop()
        {
            _pinStop.Write(GpioPinValue.Low);
            await Task.Delay(3000);
            _pinStop.Write(GpioPinValue.High);
        }
    }
}
