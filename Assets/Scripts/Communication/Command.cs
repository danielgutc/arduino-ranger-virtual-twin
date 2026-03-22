using System;

namespace Communication
{
[Serializable]
    public class Command
    {
        public float LeftMotorSpeed;
        public float RightMotorSpeed;

        public Command()
        {
            LeftMotorSpeed = 0;
            RightMotorSpeed = 0;
        }

        public Command(float leftMotorSpeed, float rightMotorSpeed)
        {
            LeftMotorSpeed = leftMotorSpeed;
            RightMotorSpeed = rightMotorSpeed;
        }

        public override string ToString()
        {
            return $"Command: LeftMotorSpeed={LeftMotorSpeed}, RightMotorSpeed={RightMotorSpeed}";
        }
    }
}
