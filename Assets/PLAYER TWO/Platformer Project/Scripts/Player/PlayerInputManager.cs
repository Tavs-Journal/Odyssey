using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager :MonoBehaviour
{
    public InputActionAsset actions;

    protected float m_movementDirectionUnlockTime;

    protected InputAction m_movement;

    protected Camera m_camera;

    protected virtual void Awake() => CacheActions();

    protected virtual void Start()
    {
        actions.Enable();
        m_camera = Camera.main;
    }

    protected virtual void Onable() => actions?.Enable();

    protected virtual void OnDisable() => actions?.Disable();

    protected virtual void CacheActions()
    {
        m_movement = actions["Movement"];
    }

    public virtual Vector3 GetMovementDirection()
    {
        if(Time.time < m_movementDirectionUnlockTime) return Vector3.zero;

        var value = m_movement.ReadValue<Vector2>();
        return GetAxisWithCrossDeadZone(value);
    }

    public virtual Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
    {
        var deadzone = InputSystem.settings.defaultDeadzoneMin;
        axis.x = Mathf.Abs(axis.x) > deadzone ? RemapToDeadZone(axis.x, deadzone) : 0;
        axis.y = Mathf.Abs(axis.y) > deadzone ? RemapToDeadZone(axis.y, deadzone) : 0;
        return new Vector3(axis.x, 0, axis.y);
    }

    protected float RemapToDeadZone(float value, float deadzone) => (value - (value > 0 ? -deadzone : deadzone)) / (1 - deadzone);

    public virtual Vector3 GetMovementCameraDirection()
    {
        var direction = GetMovementDirection();
        if(direction.sqrMagnitude > 0)
        {
            var rotation = Quaternion.AngleAxis(m_camera.transform.eulerAngles.y, Vector3.up);
            direction = rotation * direction;
            direction = direction.normalized;
        }
        return direction;
    }
}
