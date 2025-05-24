using System;
using Unity.Netcode;
using UnityEngine;

public class MoveTheBall : NetworkBehaviour
{
    public Rigidbody rb;    
    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            rb.linearVelocity = new Vector3(0, 0.0f,5*(float)Math.Cos(Time.time));
        }
    }
}
