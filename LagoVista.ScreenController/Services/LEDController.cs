using LagoVista.Core.Networking.Interfaces;
using LagoVista.Core.Networking.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace LagoVista.ScreenController.Services
{
    public class LedController : IApiHandler
    {
        private const string I2C_CONTROLLER_NAME = "I2C1";        /* For Raspberry Pi 2, use I2C1 */

        private I2cDevice _ledController;

        private static LedController _instance = new LedController();

        public static LedController Instance { get { return _instance; } }

        public async Task Init(int address = 0x40)
        {
            var settings = new I2cConnectionSettings(address);
            settings.BusSpeed = I2cBusSpeed.FastMode;

            string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);  /* Find the selector string for the I2C bus controller                   */
            var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the I2C bus controller device with our selector string           */
            _ledController = await I2cDevice.FromIdAsync(dis[0].Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings */
        }

        [MethodHandler(MethodHandlerAttribute.MethodTypes.GET, FullPath = "/led/color/{color}")]
        public HttpResponseMessage ShowColor(HttpRequestMessage msg, string color)
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

            try
            {
                _ledController.Write(buffer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return msg.GetErrorMessage(ex);
            }

            return msg.GetResponseMessage();
        }

        [MethodHandler(MethodHandlerAttribute.MethodTypes.GET, FullPath = "/led/pattern/{pattern}")]
        public HttpResponseMessage ShowPattern(HttpRequestMessage msg, String pattern)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(String.Format("pattern {0};", pattern));

            try
            {
                _ledController.Write(bytes);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return msg.GetErrorMessage(ex);
            }

            return msg.GetResponseMessage();
        }
    }
}
