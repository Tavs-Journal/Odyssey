using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Space space;
    public Vector3 euler = new Vector3(0, -180, 0);

    protected virtual void LateUpdate()
    {
        transform.Rotate(euler * Time.deltaTime, space);
    }
}
