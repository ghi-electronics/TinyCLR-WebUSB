using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.UsbClient;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SerialWebUsbApp {
    class Program {
        private static GpioController gpio;
        private static Cdc webUsb;
        private static Hashtable pins = new Hashtable();

        static void Main() {
            gpio = GpioController.GetDefault();

            var usbclientController = UsbClientController.GetDefault();

            var usbClientSetting = new UsbClientSetting
            {
                ProductName = "TinyCLR Serial WebUsb",
                InterfaceName = "TinyCLR Serial WebUsb",
                MaxPower = RawDevice.MAX_POWER,
                SerialNumber = "12344321",                
                Mode = UsbClientMode.Cdc,
            };

            webUsb = new Cdc(usbclientController, usbClientSetting);
            webUsb.DeviceStateChanged += (a, b) => Debug.WriteLine("Connection changed.");
            webUsb.DataReceived += (a, count) => Debug.WriteLine("Data received:" + count);

            webUsb.Enable();

            while (webUsb.DeviceState != DeviceState.Configured) ;
            Debug.WriteLine("UsbClient Connected");

            while (true) {
                try {
                    var command = ReadString();
                    HandleCommand(command);
                    SendString("Success");
                }
                catch (Exception ex) {
                    SendString(ex.Message);
                }
            }
        }

        private static void SendString(string s) {
            var bytes = Encoding.UTF8.GetBytes(s);
            webUsb.Stream.WriteByte((byte)bytes.Length);
            webUsb.Stream.Write(bytes);
        }

        private static string ReadString() {
            while (true) {
                if (webUsb.Stream.BytesToRead > 0) {
                    var messageLength = webUsb.Stream.BytesToRead;
                    if (messageLength > 0) {
                        var messageBuffer = new byte[messageLength];
                        var offset = 0;
                        while (offset < messageLength) {
                            var bytesRead = webUsb.Stream.Read(messageBuffer, offset, messageLength - offset);
                            if (bytesRead > 0) {
                                offset += bytesRead;
                            }
                        }
                        return Encoding.UTF8.GetString(messageBuffer);
                    }
                }
                Thread.Sleep(100);
            }
        }

        private static void HandleCommand(string command) {
            var tokens = command.ToLower().Split('=');
            var pinId = int.Parse(tokens[0]);
            var pinState = int.Parse(tokens[1].Trim());

            GpioPin pin;

            if (pins.Contains(pinId)) {
                pin = (GpioPin)pins[pinId];
            }
            else {
                pin = gpio.OpenPin(pinId);
                pin.SetDriveMode(GpioPinDriveMode.Output);
                pins[pinId] = pin;
            }
            pin.Write((GpioPinValue)pinState);
        }
    }
}
