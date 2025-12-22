using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    public float speed = 2f;
    public float amltitude = 0.5f;

    protected virtual void LateUpdate()
    {
        var wave = Mathf.Sin(Time.time * speed) * amltitude;
        transform.position += transform.up * wave * Time.deltaTime;
    }
}
