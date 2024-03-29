﻿using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
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
        private static GpioController gpio;
        private static WinUsb webUsb;
        private static Hashtable pins = new Hashtable();

        static void Main()
        {
            // visit link for demo: https://ghi-electronics.github.io/TinyCLR-WebUSB/
            gpio = GpioController.GetDefault();

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

            webUsb = new WinUsb(usbclientController, usbClientSetting);
            webUsb.DeviceStateChanged += (a, b) => Debug.WriteLine("Connection changed.");
            webUsb.DataReceived += (a, count) => Debug.WriteLine("Data received:" + count);

            webUsb.Enable();

            while (webUsb.DeviceState != DeviceState.Configured) ;
            Debug.WriteLine("UsbClient Connected");

            while (true)
            {
                try
                {
                    var command = ReadString();
                    HandleCommand(command);
                    SendString("Success");
                }
                catch (Exception ex)
                {
                    SendString(ex.Message);
                }
            }
        }

        private static void SendString(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            webUsb.Stream.WriteByte((byte)bytes.Length);
            webUsb.Stream.Write(bytes);
        }

        private static string ReadString()
        {
            while (true)
            {
                if (webUsb.Stream.BytesToRead > 0)
                {
                    var messageLength = webUsb.Stream.ReadByte();
                    if (messageLength > 0)
                    {
                        var messageBuffer = new byte[messageLength];
                        var offset = 0;
                        while (offset < messageLength)
                        {
                            var bytesRead = webUsb.Stream.Read(messageBuffer, offset, messageLength - offset);
                            if (bytesRead > 0)
                            {
                                offset += bytesRead;
                            }
                        }
                        return Encoding.UTF8.GetString(messageBuffer);
                    }
                }
                Thread.Sleep(100);
            }
        }
        
        private static void HandleCommand(string command)
        {
            var tokens = command.ToLower().Split('=');
            var pinId = int.Parse(tokens[0]);
            var pinState = int.Parse(tokens[1].Trim());

            GpioPin pin;

            if (pins.Contains(pinId))
            {
                pin = (GpioPin)pins[pinId];    
            } 
            else
            {
                pin = gpio.OpenPin(pinId);
                pin.SetDriveMode(GpioPinDriveMode.Output);
                pins[pinId] = pin;
            }
            pin.Write((GpioPinValue)pinState);
        }
    }
}
