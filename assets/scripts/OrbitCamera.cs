using Godot;

public partial class OrbitCamera : Camera3D
{
    [Export] public Node3D Target;
    [Export] public float Distance = 5.0f;
    [Export] public float MinDistance = 2.0f;
    [Export] public float MaxDistance = 20.0f;
    [Export] public float Sensitivity = 1.0f;
    [Export] public float ZoomSpeed = 1.0f;
    [Export] public float MinVerticalAngle = -80.0f;
    [Export] public float MaxVerticalAngle = 80.0f;

    private float _yaw = 0.0f;
    private float _pitch = 0.0f;

    public override void _Ready() {
        if (Target != null) {
            Vector3 offset = GlobalTransform.Origin - Target.GlobalTransform.Origin;
            Distance = offset.Length();
            _yaw = Mathf.RadToDeg(Mathf.Atan2(offset.X, offset.Z));
            _pitch = Mathf.RadToDeg(Mathf.Asin(offset.Y / Distance));
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && Input.IsMouseButtonPressed(MouseButton.Right))
        {
            _yaw -= mouseMotion.Relative.X * Sensitivity;
            _pitch -= mouseMotion.Relative.Y * Sensitivity;
            _pitch = Mathf.Clamp(_pitch, MinVerticalAngle, MaxVerticalAngle);
        }
        else if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                Distance = Mathf.Max(MinDistance, Distance - ZoomSpeed);
            else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                Distance = Mathf.Min(MaxDistance, Distance + ZoomSpeed);
        }
    }

    public override void _Process(double delta)
    {
        if (Target == null)
            return;

        float yawRad = Mathf.DegToRad(_yaw);
        float pitchRad = Mathf.DegToRad(_pitch);

        Vector3 dir = new Vector3(
            Mathf.Sin(yawRad) * Mathf.Cos(pitchRad),
            Mathf.Sin(pitchRad),
            Mathf.Cos(yawRad) * Mathf.Cos(pitchRad)
        );

        GlobalTransform = new Transform3D(
            Basis.Identity,
            Target.GlobalTransform.Origin + -dir * Distance
        );
        LookAt(Target.GlobalTransform.Origin, Vector3.Up);
    }
}