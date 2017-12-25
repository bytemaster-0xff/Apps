using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Networking.Interfaces;
using LagoVista.Core.UWP.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace LagoVista.ManCave.Services
{
    public class BlindsServer : SimpleRest
    {
        const int LED1 = 26;
        const int LED2 = 27;
        const int LED3 = 13;
        const int LED4 = 6;
        const int LED5 = 5;


        const int CENTER_BUTTON = 16;
        const int DOWN_BUTTON = 22;
        const int RIGHT_BUTTON = 18;
        const int UP_BUTTON = 12;
        const int LEFT_BUTTON = 25;


        private GpioPin _led1In;
        private GpioPin _led2In;
        private GpioPin _led3In;
        private GpioPin _led4In;
        private GpioPin _led5In;

        private GpioPin _btnCntrOut;
        private GpioPin _btnUpOut;
        private GpioPin _btnDownOut;
        private GpioPin _btnLeftOut;
        private GpioPin _btnRightOut;

        private static BlindsServer _instance = new BlindsServer();

        IMQTTDeviceClient _mqttClient;

        private BlindsServer()
        {

        }

        DateTime? _lastConnectionAttempt = null;

        private void _mqttClient_ConnectionStateChanged(object sender, bool e)
        {
            if (e)
            {
                Debug.Write("MQTT Client Connected");
            }
            else
            {
                if (_lastConnectionAttempt == null)
                {

                }

                Debug.Write("MQTT Connection Dropped");
            }
        }

        private void _mqttClient_MessageReceived(object sender, Core.Networking.Models.MqttMsgPublishEventArgs e)
        {
            Debug.WriteLine(e.Topic);

            if (!String.IsNullOrEmpty(e.Topic))
            {
                var parts = e.Topic.Split('/');
                if (parts.Length == 4)
                {
                    if (int.TryParse(parts[2], out int blindNumber))
                    {
                        MoveBlind(blindNumber, parts[3]);
                    }
                }
            }
        }

        public static BlindsServer Instance { get { return _instance; } }

        public async void Init(int port)
        {
            Port = port;
            StartServer();

            var gpio = GpioController.GetDefault();

            _led1In = gpio.OpenPin(LED1);
            _led2In = gpio.OpenPin(LED2);
            _led3In = gpio.OpenPin(LED3);
            _led4In = gpio.OpenPin(LED4);
            _led5In = gpio.OpenPin(LED5);

            _btnCntrOut = gpio.OpenPin(CENTER_BUTTON);
            _btnDownOut = gpio.OpenPin(DOWN_BUTTON);
            _btnLeftOut = gpio.OpenPin(LEFT_BUTTON);
            _btnRightOut = gpio.OpenPin(RIGHT_BUTTON);
            _btnUpOut = gpio.OpenPin(UP_BUTTON);

            _led1In.SetDriveMode(GpioPinDriveMode.Input);
            _led2In.SetDriveMode(GpioPinDriveMode.Input);
            _led3In.SetDriveMode(GpioPinDriveMode.Input);
            _led4In.SetDriveMode(GpioPinDriveMode.Input);
            _led5In.SetDriveMode(GpioPinDriveMode.Input);

            _btnCntrOut.SetDriveMode(GpioPinDriveMode.Output);
            _btnDownOut.SetDriveMode(GpioPinDriveMode.Output);
            _btnLeftOut.SetDriveMode(GpioPinDriveMode.Output);
            _btnUpOut.SetDriveMode(GpioPinDriveMode.Output);
            _btnRightOut.SetDriveMode(GpioPinDriveMode.Output);

            _btnCntrOut.Write(GpioPinValue.Low);
            _btnDownOut.Write(GpioPinValue.Low);
            _btnUpOut.Write(GpioPinValue.Low);
            _btnRightOut.Write(GpioPinValue.Low);
            _btnLeftOut.Write(GpioPinValue.Low);

            _mqttClient = SLWIOC.Create<IMQTTDeviceClient>();
            _mqttClient.ShowDiagnostics = true;
            _mqttClient.BrokerHostName = "mqttdev.nuviot.com";
            _mqttClient.BrokerPort = 1883;
            _mqttClient.DeviceId = [ADD as UserInfo NAME TO GET IT TO COMPILE!]; Hint...typical one
            _mqttClient.Password = [ADD as PASSWORD TO GET IT TO COMPILE!]; Hint...easy unsecure one for dev
            _mqttClient.ConnectionStateChanged += _mqttClient_ConnectionStateChanged;

            var result = await _mqttClient.ConnectAsync();
            if (result.Result == ConnAck.Accepted)
            {
                await _mqttClient.SubscribeAsync(new Core.Networking.Models.MQTTSubscription() { Topic = "mancave/blinds/+/+", QOS = EntityHeader<QOS>.Create(QOS.QOS0) });
                _mqttClient.MessageReceived += _mqttClient_MessageReceived;
            }
        }

        private async Task MoveToPosition(int position)
        {
            var correctPosition = false;
            var loopCount = 0;

            Debug.WriteLine("MOVING TO POSITION: " + position);

            while (!correctPosition && loopCount++ < 12)
            {
                _btnRightOut.Write(GpioPinValue.High);
                await Task.Delay(100);
                _btnRightOut.Write(GpioPinValue.Low);
                await Task.Delay(100);

                if (position == 0 && _led1In.Read() == GpioPinValue.Low && _led2In.Read() == GpioPinValue.Low && _led3In.Read() == GpioPinValue.Low && _led4In.Read() == GpioPinValue.Low && _led5In.Read() == GpioPinValue.Low)
                    correctPosition = true;

                if (position == 1 && _led1In.Read() == GpioPinValue.Low && _led2In.Read() == GpioPinValue.High && _led3In.Read() == GpioPinValue.High && _led4In.Read() == GpioPinValue.High && _led5In.Read() == GpioPinValue.High)
                    correctPosition = true;

                if (position == 2 && _led1In.Read() == GpioPinValue.High && _led2In.Read() == GpioPinValue.Low && _led3In.Read() == GpioPinValue.High && _led4In.Read() == GpioPinValue.High && _led5In.Read() == GpioPinValue.High)
                    correctPosition = true;

                if (position == 3 && _led1In.Read() == GpioPinValue.High && _led2In.Read() == GpioPinValue.High && _led3In.Read() == GpioPinValue.Low && _led4In.Read() == GpioPinValue.High && _led5In.Read() == GpioPinValue.High)
                    correctPosition = true;

                if (position == 4 && _led1In.Read() == GpioPinValue.High && _led2In.Read() == GpioPinValue.High && _led3In.Read() == GpioPinValue.High && _led4In.Read() == GpioPinValue.Low && _led5In.Read() == GpioPinValue.High)
                    correctPosition = true;

                if (position == 5 && _led1In.Read() == GpioPinValue.High && _led2In.Read() == GpioPinValue.High && _led3In.Read() == GpioPinValue.High && _led4In.Read() == GpioPinValue.High && _led5In.Read() == GpioPinValue.Low)
                    correctPosition = true;

                Debug.WriteLine("FOUND MATCH: " + correctPosition);
            }
        }

        private async void MoveBlind(int blind, String command, int? ms = null)
        {
            Debug.WriteLine("COMMAND => " + command);

            await MoveToPosition(blind);
            if (command == "up")
            {
                _btnUpOut.Write(GpioPinValue.High);
                await Task.Delay(1000);
                _btnUpOut.Write(GpioPinValue.Low);
                await _mqttClient.PublishAsync($"mancave/status/blinds/{blind}/{command}");
            }
            else if (command == "down")
            {
                Debug.WriteLine("DOWN");
                _btnDownOut.Write(GpioPinValue.High);
                await Task.Delay(1000);
                _btnDownOut.Write(GpioPinValue.Low);
                Debug.WriteLine("DONE");
                await _mqttClient.PublishAsync($"mancave/status/blinds/{blind}/{command}");
            }
            else if (command == "stop")
            {
                _btnCntrOut.Write(GpioPinValue.High);
                await Task.Delay(1000);
                _btnCntrOut.Write(GpioPinValue.Low);
                await _mqttClient.PublishAsync($"mancave/status/blinds/{blind}/{command}");
            }

            if (ms.HasValue)
            {
                await Task.Delay(ms.Value);
                _btnCntrOut.Write(GpioPinValue.High);
                await Task.Delay(500);
                _btnCntrOut.Write(GpioPinValue.Low);
            }

            
        }

        public override String Process(String queryString)
        {
            Debug.WriteLine(queryString);
            queryString = queryString.Trim('/');

            var parts = queryString.ToLower().Split('/');

            if (parts.Length > 0)
            {
                if (parts[0] == "blind")
                {
                    int blind = 0;
                    if (int.TryParse(parts[1], out blind))
                    {
                        var command = parts[2];
                        if (parts.Length > 3)
                        {
                            int ms = 0;
                            if (int.TryParse(parts[3], out ms))
                            {
                                MoveBlind(blind, command, ms);
                                return "{'result':'ok'}";
                            }
                            else
                            {
                                return "{'result':'invalidQUeryString'}";
                            }
                        }

                        MoveBlind(blind, command);
                        return "{'result':'ok'}";
                    }
                    else
                        return "{'result':'invalidQUeryString'}";
                }
                return "{'result':'unknownCommand'}";

            }
            return "{'result':'invalidQUeryString'}";
        }

    }
}
