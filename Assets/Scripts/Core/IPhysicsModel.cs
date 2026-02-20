namespace PIDSimulator
{
    public interface IPhysicsModel
    {
        string GetModelName();
        float GetCurrentValue();
        float GetCurrentVelocity();
        void SetTargetValue(float target);
        float GetTargetValue();
        void SetControlInput(float input);
        void SetMass(float mass);
        void Reset();
    }
}
