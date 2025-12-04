using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager :MonoBehaviour
{
    public InputActionAsset actions;

    protected float m_movementDirectionUnlockTime;

    protected InputAction m_movement;
    protected InputAction m_look;
    protected InputAction m_jump;

    protected Camera m_camera;

    protected const string k_mouseDeviceName = "Mouse";

    protected float? m_lastJumpTime;
    protected const float k_jumpBuffer = 0.15f;

    protected virtual void Awake() => CacheActions();

    protected virtual void Start()
    {
        actions.Enable();
        m_camera = Camera.main;
    }

    protected virtual void Update()
    {
        if (m_jump.WasPressedThisFrame())
        {
            m_lastJumpTime = Time.time;
        }
    }

    protected virtual void OnEnable() => actions?.Enable();

    protected virtual void OnDisable() => actions?.Disable();

    protected virtual void CacheActions()
    {
        m_movement = actions["Movement"];
        m_look = actions["Look"];
        m_jump = actions["Jump"];
    }

    public virtual Vector3 GetLookDirection()
    {
        var value = m_look.ReadValue<Vector2>();

        if (IsLookingWithMouse())
        {
            return new Vector3(value.x, 0, value.y);
        }

        return GetAxisWithCrossDeadZone(value);
    }

    public virtual bool IsLookingWithMouse()
    {
        if(m_look.activeControl == null) return false;
        return m_look.activeControl.device.name.Equals(k_mouseDeviceName);
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

    public virtual bool GetJumpDown()
    {
        if(m_lastJumpTime != null &&
            Time.time -  m_lastJumpTime < k_jumpBuffer)
        {
            m_lastJumpTime = null;
            return true;
        }
        return false;
    }
    public virtual bool GetJumpUp() => m_jump.WasReleasedThisFrame();
}
