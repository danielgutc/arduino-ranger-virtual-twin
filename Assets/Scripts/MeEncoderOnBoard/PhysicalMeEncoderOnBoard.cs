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
            currentSpeed = speed * speedMultiplier;

            if (this.transform.gameObject.name.ToLower().Contains("left"))
            {
                rangerComm.Command.LeftMotorSpeed = -speed * speedMultiplier;
            }
            else
            {
                rangerComm.Command.RightMotorSpeed = speed * speedMultiplier;
            }
        }

        public void StopMotor()
        {
            currentSpeed = rangerComm.Command.LeftMotorSpeed = rangerComm.Command.RightMotorSpeed = 0;
        }

        public float GetCurrentSpeed()
        {
            if (this.transform.gameObject.name.ToLower().Contains("left"))
            {
                return rangerComm.Telemetry.LeftMotorSpeed / speedMultiplier;
            }
            else
            {
                return rangerComm.Telemetry.RightMotorSpeed / speedMultiplier;
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