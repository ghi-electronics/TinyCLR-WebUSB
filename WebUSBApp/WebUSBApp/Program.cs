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

            var usbClientSetting = new UsbClientSetting()
            {
                Mode = UsbClientMode.WinUsb,
                ManufactureName = "GHI Electronics",
                ProductName = "Test WebUSB",
                SerialNumber = "12345678",
                Guid = "{F99BAABA-F033-483D-AFB2-A6E4F3AE6310}",
                VendorId = 0x1209,
                ProductId = 0xa800,
                BcdUsb = 0x210,
            };

            var winUsb = new WinUsb(usbclientController, usbClientSetting);
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
