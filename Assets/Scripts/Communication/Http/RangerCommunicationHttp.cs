using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Communication.Http
{
    public class RangerCommunicationHttp : MonoBehaviour, IRangerCommunication
    {
        [Header("HTTP Server")]
        public int listenPort = 8080;
        public TerminalDisplay terminalDisplay;
        private readonly object telemetryLock = new object();
        private Telemetry telemetry = new Telemetry();
        private Command command = new Command(); 
        private HttpListener listener;
        private Thread listenerThread;
        private volatile bool isRunning;

        public Telemetry Telemetry
        {
            get
            {
                lock (telemetryLock)
                {
                    return telemetry;
                }
            }
        }

        public Command Command
        {
            get
            {
                lock (telemetryLock)
                {
                    return command;
                }
            }
        }

        void Start()
        {
            Subscribe();
        }

        void Update()
        {
            terminalDisplay.UpdateDisplay(Telemetry.ToString());
        }

        public void Subscribe()
        {
            if (isRunning)
            {
                return;
            }

            string listenPrefix = $"http://+:{listenPort}/";

            listener = new HttpListener();
            listener.Prefixes.Add(listenPrefix);
            listener.Start();

            isRunning = true;
            listenerThread = new Thread(ListenLoop)
            {
                IsBackground = true
            };
            listenerThread.Start();
            Debug.Log($"[{nameof(RangerCommunicationHttp)}] Listening on {listenPrefix}");
        }
        
        public void Write(string message)
        {
            Debug.LogWarning($"[{nameof(RangerCommunicationHttp)}] Write is not supported. Received: {message}");
        }

        void OnDestroy()
        {
            StopServer();
        }

        private void StopServer()
        {
            isRunning = false;

            if (listener != null)
            {
                try
                {
                    if (listener.IsListening)
                    {
                        listener.Stop();
                    }
                    listener.Close();
                }
                catch
                {
                    // Ignore listener shutdown errors.
                }
                listener = null;
            }

            if (listenerThread != null && listenerThread.IsAlive)
            {
                listenerThread.Join(500);
                listenerThread = null;
            }
        }

        private void ListenLoop()
        {
            while (isRunning)
            {
                HttpListenerContext context;

                try
                {
                    context = listener.GetContext();
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch
                {
                    continue;
                }

                HandleRequest(context);
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                if (!request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    WriteResponse(response, 405, "Only POST is allowed.");
                    return;
                }

                if (!request.Url.AbsolutePath.Equals("/ranger-command", StringComparison.OrdinalIgnoreCase))
                {
                    WriteResponse(response, 404, "Not found.");
                    return;
                }

                string body;
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8))
                {
                    body = reader.ReadToEnd();
                }

                var payload = JsonUtility.FromJson<TelemetryPayload>(body);
                if (payload == null)
                {
                    WriteResponse(response, 400, "Invalid telemetry payload.");
                    return;
                }

                lock (telemetryLock)
                {
                    telemetry = payload.ToTelemetry();
                }

                WriteResponse(response, 200, JsonUtility.ToJson(Command), "application/json");
            }
            catch (Exception ex)
            {
                WriteResponse(response, 400, $"Invalid request: {ex.Message}");
            }
        }

        private static void WriteResponse(HttpListenerResponse response, int statusCode, string message, string contentType = "text/plain")
        {
            response.StatusCode = statusCode;
            response.ContentType = contentType;

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        [Serializable]
        private class TelemetryPayload
        {
            public int State;
            public int Lidar;
            public int Ultrasonic;
            public int Angle;
            public bool ObstacleDetected;
            public int CurrentScanMaxDistance;
            public int CurrentScanMaxDistanceAngle;
            public int MaxDistanceAngle;
            public int WaitNextScan;
            public int LeftMotorSpeed;
            public int RightMotorSpeed;

            public Telemetry ToTelemetry()
            {
                return new Telemetry
                {
                    State = State,
                    Lidar = Lidar,
                    Ultrasonic = Ultrasonic,
                    Angle = Angle,
                    ObstacleDetected = ObstacleDetected,
                    CurrentScanMaxDistance = CurrentScanMaxDistance,
                    CurrentScanMaxDistanceAngle = CurrentScanMaxDistanceAngle,
                    MaxDistanceAngle = MaxDistanceAngle,
                    WaitNextScan = WaitNextScan,
                    LeftMotorSpeed = LeftMotorSpeed,
                    RightMotorSpeed = RightMotorSpeed
                };
            }
        }
    }
}
