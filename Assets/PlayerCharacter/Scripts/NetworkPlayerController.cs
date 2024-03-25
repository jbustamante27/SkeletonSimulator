using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.InputSystem;

public class NetworkPlayerController : NetworkComponent
{
    public Material[] SkeletonMatArray;
    public Renderer SkeletonRenderer;
    public Rigidbody MyRig;
    public Animator MyAnime;

    public PlayerInput MyInput;
    public InputActionAsset MyMap;
    public InputAction MoveA;
    public InputAction FireA;

    public Vector2 lastMove;
    public bool isFiring;
    public bool canFire = true;
    public bool gameOver = false;
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "MOVE" && IsServer)
        {
            string[] args = value.Split(',');
            lastMove = new Vector2(float.Parse(args[0]), float.Parse(args[1]));
        }
        if (flag == "FIRE" && canFire)
        {
            if (IsServer)
            {
                canFire = false;
                StartCoroutine(Reload());
                SendUpdate("FIRE", "1");
            }
            MyAnime.SetTrigger("Attack1h1");
        }
    }

    public IEnumerator Reload()
    {
        yield return new WaitForSeconds(.5f);
        canFire = true;
    }

    public override void NetworkedStart()
    {
        SkeletonRenderer.materials[0] = SkeletonMatArray[this.Owner % 3];
        SkeletonRenderer.material = SkeletonMatArray[this.Owner % 3];
        if (IsServer)
        {
            int tstart = (this.Owner % 3) + 1;
            GameObject temp = GameObject.Find("SpawnPoint" + tstart);
            MyRig.position = temp.transform.position;
            MyRig.useGravity = true;
        }
        if (IsLocalPlayer)
        {
            MyInput.actions = MyMap;
            MoveA = MyInput.currentActionMap.FindAction("Move", true);
            MoveA.started += this.OnDirectionChanged;
            MoveA.performed += this.OnDirectionChanged;
            MoveA.canceled  += this.OnDirectionChanged;
            FireA = MyInput.currentActionMap.FindAction("Fire", true);
            FireA.started += this.OnFire;
        }
    }

    private void OnDestroy()
    {
        MoveA.started -= this.OnDirectionChanged;
        MoveA.performed -= this.OnDirectionChanged;
        MoveA.canceled -= this.OnDirectionChanged;
        FireA.started -= this.OnFire;
    }

    public void OnDirectionChanged(InputAction.CallbackContext context)
        {
            if (IsLocalPlayer)
            {
                Debug.Log("Inside move");
                if (context.action.phase == InputActionPhase.Started || context.action.phase == InputActionPhase.Performed)
                {
                    Debug.Log("Sending MOVE - Non-stop");
                    //This means the direction has changed!
                    lastMove = context.ReadValue<Vector2>();
                    SendCommand("MOVE", lastMove.x + "," + lastMove.y);
                }
                if (context.action.phase == InputActionPhase.Canceled)
                {
                    //Hmmm.. why would we need this check    
                    lastMove = Vector2.zero;
                    SendCommand("MOVE", lastMove.x + "," + lastMove.y);
                }
            }    
        }

    public void OnFire(InputAction.CallbackContext context)
    {
        if(context.action.phase == InputActionPhase.Started)
        {
            //Fire Button was pushed!
            SendCommand("FIRE", "1");
        }
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(.1f);
    }

    // Start is called before the first frame update
    void Start()
    {
        MyAnime = GetComponent<Animator>();
        MyInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            Debug.Log("Forward velocity is " + lastMove.y);
            MyRig.velocity = this.transform.forward * lastMove.y * 3 + new Vector3(0, MyRig.velocity.y, 0);
            MyRig.angularVelocity = new Vector3(0, lastMove.x, 0) * Mathf.PI / 3.0f;
            var speed = Mathf.Max(Mathf.Abs(MyRig.velocity.x), Mathf.Abs(MyRig.angularVelocity.y));
            MyAnime.SetFloat("speedv", speed);
        }
        if (IsLocalPlayer)
        {
            Camera.main.transform.position = this.transform.position + this.transform.forward * -3 + this.transform.up;
            Camera.main.transform.LookAt(this.transform.position);
        }
        if (IsClient)
        {
            if (Mathf.Abs(MyRig.velocity.magnitude) > Mathf.Abs(MyRig.angularVelocity.y))
            {
                MyAnime.SetFloat("speedv", MyRig.velocity.magnitude);
            }
            else
            {
                MyAnime.SetFloat("speedv", MyRig.angularVelocity.y);
            }
        }
    }
}
