using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public Transform target;
    public float height = 20f;

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 pos = target.position;
            pos.y += height;
            transform.position = pos;
        }
    }
}
