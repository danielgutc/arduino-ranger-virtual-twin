using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Communication.Ble
{
    public class RangerCommunicationBle : MonoBehaviour, IRangerCommunication
    {
        public string rangerNameKeyword = "Makeblock_LE703e97f555d4";
        public string telemetryServiceUuid = "00006287-3c17-d293-8e48-14fe2e4da212";
        public string telemetryCharUuid = "0000ffe2-0000-1000-8000-00805f9b34fb";

        private string connectedDeviceId;
        private bool isScanningDevices = false;
        private bool isDeviceScanStarted = false;
        private bool isSubscribed = false;
        private Telemetry telemetry;
        public TerminalDisplay terminalDisplay;
        public Telemetry Telemetry { get => telemetry; }

        void Start()
        {
            isScanningDevices = true;
            isDeviceScanStarted = false;
            isSubscribed = false;
            telemetry = new Telemetry();
        }

        void Update()
        {
            if (isScanningDevices)
            {
                PollDevicesNonBlocking();
            }

            if (isSubscribed)
            {
                BleApi.BLEData telemetryBleData;
                while (BleApi.PollData(out telemetryBleData, false))
                {
                    string message = Encoding.ASCII.GetString(telemetryBleData.buf, 0, telemetryBleData.size).TrimEnd('\0');

                    try
                    {
                        telemetry = JsonUtility.FromJson<Telemetry>(message);
                    }
                    catch
                    {
                        telemetry = Parse(message);
                    }

                    terminalDisplay.UpdateDisplay(telemetry.ToString());
                }
            }
        }

        private void PollDevicesNonBlocking()
        {
            if (!isDeviceScanStarted)
            {
                BleApi.StartDeviceScan();
                isDeviceScanStarted = true;
            }

            BleApi.DeviceUpdate device = new();
            var status = BleApi.PollDevice(ref device, false);

            if (status == BleApi.ScanStatus.AVAILABLE)
            {
                if (!string.IsNullOrEmpty(device.name) &&
                    device.name.ToLower().Contains(rangerNameKeyword.ToLower()))
                {
                    terminalDisplay.UpdateDisplay($"Found Ranger device: {device.name} ({device.id}). ");
                    connectedDeviceId = device.id;
                    isScanningDevices = false;
                    BleApi.StopDeviceScan();
                    isDeviceScanStarted = false;
                    Subscribe();
                }
            }
            else if (status == BleApi.ScanStatus.FINISHED)
            {
                terminalDisplay.UpdateDisplay("Scan finished with no match. ");
                BleApi.StopDeviceScan();
                isDeviceScanStarted = false;
                isScanningDevices = false;
            }
        }

        public void Subscribe()
        {
            // no error code available in non-blocking mode
            bool s = BleApi.SubscribeCharacteristic(connectedDeviceId, telemetryServiceUuid, telemetryCharUuid, false);
            if (!s)
            {
                terminalDisplay.AppendToDisplay("Failed to subscribe to telemetry characteristic. ");
                return;
            }
            isSubscribed = true;
        }

        public void Write(string message)
        {
            byte[] payload = Encoding.ASCII.GetBytes(message);
            BleApi.BLEData data = new()
            {
                buf = new byte[512],
                size = (short)payload.Length,
                deviceId = connectedDeviceId,
                serviceUuid = telemetryServiceUuid,
                characteristicUuid = telemetryCharUuid
            };

            for (int i = 0; i < payload.Length; i++)
            {
                data.buf[i] = payload[i];
            }
            // no error code available in non-blocking mode
            BleApi.SendData(in data, false);
        }

        private Telemetry Parse(string input)
        {
            var telemetry = new Telemetry();
            var props = typeof(Telemetry).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var pair in input.Split(','))
            {
                var kv = pair.Split(':');
                if (kv.Length != 2) continue;

                var key = kv[0].Trim();
                var value = kv[1].Trim();

                var prop = Array.Find(props, p => string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));
                if (prop != null && prop.CanWrite)
                {
                    if (prop.PropertyType == typeof(bool))
                    {
                        prop.SetValue(telemetry, ParseFlexibleBool(value));
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        prop.SetValue(telemetry, int.Parse(value));
                    }
                    else
                    {
                        prop.SetValue(telemetry, value);
                    }

                }
            }

            return telemetry;
        }

        private bool ParseFlexibleBool(string input)
        {
            switch (input.ToLowerInvariant())
            {
                case "true":
                case "1":
                case "yes":
                case "on":
                    return true;
                case "false":
                case "0":
                case "no":
                case "off":
                    return false;
                default:
                    throw new FormatException($"'{input}' is not a valid boolean value.");
            }
        }

    }
}
