using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

[RequireComponent(typeof(Rigidbody))]

public class NetworkRigidBody : NetworkComponent
{
    //Position, velocity, rotation, angular velocity
    public Vector3 LastPosition;
    public Vector3 LastRotation;
    //These two values below are what make rigidbody so great
    public Vector3 LastVelocity;
    public Vector3 LastAngular;

    public Vector3 OffsetVelocity;

    public float Threshold = .1f;
    public float EThreshold = 2.5f;
    public Rigidbody MyRig;
    public bool UseOffsetVelocity = true;

    public static Vector3 VectorFromString(string value)
    {
        char[] temp = { '(', ')' };
        string[] args = value.Trim(temp).Split(',');
        return new Vector3(float.Parse(args[0].Trim()), float.Parse(args[1].Trim()), float.Parse(args[2].Trim()));
    }
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "POS" && IsClient)
        {
            LastPosition = VectorFromString(value);
            float d = (MyRig.position - LastPosition).magnitude;
            if (d > EThreshold || !UseOffsetVelocity || LastVelocity.magnitude < .1)
            {
                OffsetVelocity = Vector3.zero;
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
            // if(LastVelocity.magnitude<.05f)
            // {
            //     OffsetVelocity = Vector3.zero;
            //     //Taking out MyRig.position line here makes it more jarring but more accurate
            //     MyRig.position = LastPosition;
            // }
            // if(UseOffsetVelocity)
            // {
            //     MyRig.velocity = LastVelocity + OffsetVelocity;
            // }
            // else
            // {
            //     MyRig.velocity = LastVelocity;
            // }
        }
        if (flag == "ROT" && IsClient)
        {
            LastRotation = VectorFromString(value);
            MyRig.rotation = Quaternion.Euler(LastRotation);
        }
        if (flag == "ANG" && IsClient)
        {
            LastAngular = VectorFromString(value);
            //MyRig.angularVelocity = LastAngular;
        }
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsClient)
        {
            MyRig.useGravity = false;
            // We let the server deal with gravity!
        }
        while (true)
        {
            if (IsServer)
            {
                if ((LastPosition - MyRig.position).magnitude > Threshold)
                {
                    SendUpdate("POS", MyRig.position.ToString("F3"), false);
                    LastPosition = MyRig.position;
                }
                if ((LastVelocity - MyRig.velocity).magnitude > Threshold)
                {
                    SendUpdate("VEL", MyRig.velocity.ToString("F3"), false);
                    LastVelocity = MyRig.velocity;
                }
                if ((LastRotation - MyRig.rotation.eulerAngles).magnitude > Threshold)
                {
                    SendUpdate("ROT", MyRig.rotation.eulerAngles.ToString("F3"), false);
                    LastRotation = MyRig.rotation.eulerAngles;
                }
                if ((LastAngular - MyRig.angularVelocity).magnitude > Threshold)
                {
                    SendUpdate("ANG", MyRig.angularVelocity.ToString("F3"), false);
                    LastAngular = MyRig.angularVelocity;
                }
                if (IsDirty)
                {
                    SendUpdate("POS", MyRig.position.ToString("F3"), false);
                    SendUpdate("VEL", MyRig.velocity.ToString("F3"), false);
                    SendUpdate("ROT", MyRig.rotation.eulerAngles.ToString("F3"), false);
                    SendUpdate("ANG", MyRig.angularVelocity.ToString("F3"), false);
                    IsDirty = false;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    public override void NetworkedStart()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        MyRig = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsClient)
        {
            //Continuously update velocity
            if (LastVelocity.magnitude < .05f)
            {
                OffsetVelocity = Vector3.zero;
            }
            if (UseOffsetVelocity)
            {
                MyRig.velocity = LastVelocity + OffsetVelocity;
            }
            else
            {
                MyRig.velocity = LastVelocity;
            }
            //Continuously update angular velocity
            MyRig.angularVelocity = LastAngular;
        }

    }
}

