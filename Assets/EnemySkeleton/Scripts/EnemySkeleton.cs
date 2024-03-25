using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.AI;
public class EnemySkeleton : NetworkComponent
{

    public NavMeshAgent MyAgent;
    public List<Vector3> Goals;
    public Vector3 CurrentGoal;
    public Animator MyAnime;
    public override void HandleMessage(string flag, string value)
    {
        
    }

    public override void NetworkedStart()
    {
   
    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(.1f);
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] temp = GameObject.FindGameObjectsWithTag("NavPoint");
        Goals = new List<Vector3>();
        foreach(GameObject g in temp)
        {
            Goals.Add(g.transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
