using UnityEngine;
using Cinemachine;
using UnityEditor.SceneManagement;
[RequireComponent(typeof(CinemachineVirtualCamera))]
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Camera")]
public class PlayerCamera : MonoBehaviour
{

    [Header("Camera Settings")]
    public Player player;
    public float maxDistance = 15f;
    public float initialAngel = 20f;
    public float heightOffset = 1f;

    protected CinemachineVirtualCamera m_camera;
    protected Cinemachine3rdPersonFollow m_cameraBody;
    protected CinemachineBrain m_brain;

    protected float m_cameraDistance;
    protected float m_cameraTargetYaw;
    protected float m_cameraTargetPitch;
    protected Vector3 m_cameraTargetPosition;

    protected Transform m_target;

    protected string k_targetName = "Player Follower Camera Target"; 

    protected virtual void Start()
    {
        InitializeComponents();
        InitializeFollower();
        InitializeCamera();
    }

    protected virtual void InitializeComponents()
    {
        if (!player)
        {
            player = FindObjectOfType<Player>();
        }
        m_camera = GetComponent<CinemachineVirtualCamera>();
        m_cameraBody = m_camera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
        m_brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    protected virtual void InitializeFollower()
    {
        m_target = new GameObject(k_targetName).transform;
        m_target.position = player.transform.position;
    }

    protected virtual void InitializeCamera()
    {
        m_camera.Follow = m_target.transform;
        m_camera.LookAt = player.transform;

        Reset();
    }

    public virtual void Reset()
    {
        m_cameraDistance = maxDistance;
        m_cameraTargetYaw = player.transform.rotation.eulerAngles.y;
        m_cameraTargetPitch = initialAngel;
        m_cameraTargetPosition = player.transform.position + Vector3.up * heightOffset;
        MoveTarget();
        m_brain.ManualUpdate();
    }
    protected virtual void MoveTarget()
    {
        m_target.position = m_cameraTargetPosition;
        m_target.rotation = Quaternion.Euler(m_cameraTargetPitch, m_cameraTargetYaw, 0.0f);
        m_cameraBody.CameraDistance = m_cameraDistance;
    }
    protected virtual void LateUpdate()
    {
        MoveTarget();
    }
}