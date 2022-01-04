using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

[RequireComponent(typeof(Rigidbody2D))]

public class NetworkRigidBody : NetworkComponent
{
    public Vector2 LastPosition;
    public Vector2 LastVelocity;
    public Vector2 OffsetVelocity;

    public float Threshold = .1f;
    public float EThreshold = 2.5f;
    public Rigidbody2D MyRig;
    public bool UseOffsetVelocity = true;

    public static Vector2 VectorFromString(string value)
    {
        char[] temp = { '(', ')' };
        string[] args = value.Trim(temp).Split(',');
        return new Vector2(float.Parse(args[0].Trim()), float.Parse(args[1].Trim()));
    }

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "POS" && IsClient)
        {
            LastPosition = VectorFromString(value);
            float d = (MyRig.position - LastPosition).magnitude;
            if(d > EThreshold || !UseOffsetVelocity || LastVelocity.magnitude < .1)
            {
                OffsetVelocity = Vector2.zero;
                MyRig.position = LastPosition;
            }
            else
            {
                OffsetVelocity = (LastPosition - MyRig.position);
            }
        }

        if (flag == "VEL" && IsClient)
        {
            LastVelocity = VectorFromString(value);

            if(LastVelocity.magnitude < .1f)
            {
                LastVelocity = Vector2.zero;
                OffsetVelocity = Vector2.zero;
            }
        }
    }

    public override void NetworkedStart()
    {
        
    }

    public override IEnumerator SlowUpdate()
    {
        while(true)
        {
            if(IsServer)
            {
                if((LastPosition - MyRig.position).magnitude > Threshold)
                {
                    SendUpdate("POS", MyRig.position.ToString("F3"));
                    LastPosition = MyRig.position;
                }
                if ((LastVelocity - MyRig.velocity).magnitude > Threshold)
                {
                    SendUpdate("VEL", MyRig.velocity.ToString("F3"));
                    LastVelocity = MyRig.velocity;
                }
                if(IsDirty)
                {
                    SendUpdate("POS", MyRig.position.ToString("F3"));
                    SendUpdate("VEL", MyRig.velocity.ToString("F3"));
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        MyRig = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(IsClient)
        {
            if(LastVelocity.magnitude < .05f)
            {
                OffsetVelocity = Vector2.zero;
            }
            if(UseOffsetVelocity)
            {
                MyRig.velocity = LastVelocity + OffsetVelocity;
            }
            else
            {
                MyRig.velocity = LastVelocity;
            }
        }
    }
}
