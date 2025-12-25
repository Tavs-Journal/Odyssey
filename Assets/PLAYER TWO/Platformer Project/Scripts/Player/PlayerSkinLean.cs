using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkinLean : MonoBehaviour
{
    [Header("LeanSettings")]
    public bool canLean = true;
    public float maxLeanAngle;
    public float leanSmoothTime;
    public float leanVelocity;
    public float minLeanSpeed;

    protected virtual Vector3 HandleLean(Player player, Transform skin)
    {
        var speed = player.lateralvelocity.magnitude;
        var amount = 0f;
        if (canLean && speed > minLeanSpeed)
        {
            var targetDirection = player.input.GetMovementCameraDirection();
            var moveDirection = player.lateralvelocity / speed;
            var angle = Vector3.SignedAngle(targetDirection, moveDirection, Vector3.up);
            amount = Mathf.Clamp(angle, -maxLeanAngle, maxLeanAngle);
        }
        var rotation = skin.localEulerAngles;
        rotation.z = Mathf.SmoothDampAngle(rotation.z, amount, ref leanVelocity, leanSmoothTime);
        return rotation;
    }
}
