using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 1.35f;

    void FixedUpdate()
    {
        Vector3 desiredPos = new Vector2(Camera.main.gameObject.transform.position.x, target.position.y + 2.5f);
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);
        transform.position = smoothedPos;
    }
}
