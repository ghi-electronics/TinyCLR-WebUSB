using GHIElectronics.TinyCLR.Devices.UsbClient;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace WebUSBApp
{
    class Program
    {
        static void Main()
        {
            var usbclientController = UsbClientController.GetDefault();

            var usbClientSetting = new UsbClientSetting
            {
                ProductName = "TinyCLR WebUsb",
                InterfaceName = "TinyCLR WebUsb",
                MaxPower = RawDevice.MAX_POWER,
                SerialNumber = "12344321",
                Guid = "{A5FDFBF7-E5EE-489D-8037-48E2EED80A29}",
                Mode = UsbClientMode.WinUsb,
            };

            var winUsb = new WebUsb(usbclientController, usbClientSetting);
            winUsb.DeviceStateChanged += (a, b) => Debug.WriteLine("Connection changed.");
            winUsb.DataReceived += (a, count) => Debug.WriteLine("Data received:" + count);

            winUsb.Enable();


            while (winUsb.DeviceState != DeviceState.Configured) ;
            Debug.WriteLine("UsbClient Connected");

            while (true)
            {
                if (winUsb.Stream.BytesToRead > 0)
                {
                    var messageLength = winUsb.Stream.ReadByte();
                    if (messageLength > 0)
                    {
                        var messageBuffer = new byte[messageLength];
                        var offset = 0;
                        while (offset < messageLength)
                        {
                            var bytesRead = winUsb.Stream.Read(messageBuffer, offset, messageLength - offset);
                            if (bytesRead > 0)
                            {
                                offset += bytesRead;
                            }
                        }

                        winUsb.Stream.WriteByte((byte)messageLength);
                        winUsb.Stream.Write(messageBuffer);
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}
