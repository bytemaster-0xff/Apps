using LagoVista.Core.PlatformSupport;
using LagoVista.Core.UWP.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace KitchenKiosk.Managers
{
    public class LedController
    {
        private const string I2C_CONTROLLER_NAME = "I2C1";        /* For Raspberry Pi 2, use I2C1 */

        private I2cDevice _ledController;

        private ILogger _logger;

        private static LedController _instance = new LedController();

        public static LedController Instance { get { return _instance; } }

        public async Task InitAsync(int address, ILogger logger)
        {
            _logger = logger;

            var settings = new I2cConnectionSettings(address);
            settings.BusSpeed = I2cBusSpeed.FastMode;

            string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);  /* Find the selector string for the I2C bus controller                   */
            var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the I2C bus controller device with our selector string           */
            _ledController = await I2cDevice.FromIdAsync(dis[0].Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings */
        }


        public void ShowColor(string color)
        {
            _logger.AddCustomEvent(LogLevel.Message, "LedController_ShowColor", color);

            try
            {
                var buffer = new byte[]
                {
                    (byte)'c',
                    (byte)'o',
                    (byte)'l',
                    (byte)'o',
                    (byte)'r',
                    (byte)' ',
                    (byte)byte.Parse(color.Substring(0,2), System.Globalization.NumberStyles.HexNumber),
                    (byte)' ',
                    (byte)byte.Parse(color.Substring(2,2), System.Globalization.NumberStyles.HexNumber),
                    (byte)' ',
                    (byte)byte.Parse(color.Substring(4,2), System.Globalization.NumberStyles.HexNumber),
                    (byte)';',
                };

                _ledController.Write(buffer);
            }
            catch (Exception ex)
            {
                _logger.AddException("LedController", ex);
            }
        }

        public void ShowPattern(String pattern)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(String.Format("pattern {0};", pattern));

            _logger.AddCustomEvent(LogLevel.Message, "LedController_ShowPattern", pattern);

            try
            {
                _ledController.Write(bytes);
            }
            catch (Exception ex)
            {
                _logger.AddException("LedController", ex);
            }
        }
    }
}
