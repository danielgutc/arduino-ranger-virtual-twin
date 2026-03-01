using Communication.Ble;
using MeUltrasonicSensor;
using UnityEngine;

public class PhysicalMeUltrasonicSensor : MonoBehaviour, IMeUltrasonicSensor
{
    public RangerCommunicationBle rangerBle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rangerBle == null)
        {
            rangerBle = FindFirstObjectByType<RangerCommunicationBle>();
        }
    }

    public void Initialize()
    {
        // Initialization logic for the ultrasonic sensor
        Debug.Log("Ultrasonic Sensor Initialized");
    }

    // Implement the interface methods here
    public float GetDistance()
    {
        return rangerBle.Telemetry.Ultrasonic;
    }

    public float GetDistanceCm()
    {
        throw new System.NotImplementedException();
    }
}
