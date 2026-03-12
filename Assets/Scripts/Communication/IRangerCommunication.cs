namespace Communication
{    
    public interface IRangerCommunication
    {
        Telemetry Telemetry { get; }
        void Subscribe();
        void Write(string message);
    }
}