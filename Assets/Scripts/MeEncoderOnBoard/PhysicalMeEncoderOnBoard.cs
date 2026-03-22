using Communication.Http;
using UnityEngine;

namespace MeEncoderOnBoard
{
    public class PhysicalMeEncoderOnBoard : MonoBehaviour, IMeEncoderOnBoard
    {
        public RangerCommunicationHttp rangerComm;
        public float speedMultiplier = 0.1f;
        private float currentSpeed = 0;
        private float targetPosition = 0;

        private void Start()
        {
            if (rangerComm == null)
            {
                rangerComm = FindFirstObjectByType<RangerCommunicationHttp>();
            }
        }

        private void Update()
        {
            this.transform.Rotate(Vector3.left * currentSpeed);
        }

        public void SetCurrentSpeed(float speed)
        {
            if (this.transform.gameObject.name.ToLower().Contains("left"))
            {
                rangerComm.Command.LeftMotorSpeed = speed;
            }
            else
            {
                rangerComm.Command.RightMotorSpeed = speed;
            }
        }

        public void StopMotor()
        {
            rangerComm.Command.LeftMotorSpeed = rangerComm.Command.RightMotorSpeed = 0;
        }

        public float GetCurrentSpeed()
        {
            if (this.transform.gameObject.name.ToLower().Contains("left"))
            {
                return rangerComm.Telemetry.LeftMotorSpeed;
            }
            else
            {
                return rangerComm.Telemetry.RightMotorSpeed;
            }
        }

        public void SetPosition(float position)
        {
            targetPosition = position;
        }

        public float GetPosition()
        {
            return this.transform.localEulerAngles.z;
        }
    }
}