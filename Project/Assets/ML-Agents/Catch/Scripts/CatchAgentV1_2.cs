using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.SideChannels;
using System.Security.Policy;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.Rendering;
using System;
using Random=UnityEngine.Random;

public class CatchAgentV1_2 : Agent
{
    float currentTs = 0f;
    public float m_speed = 0f;
    Rigidbody rBody;
    public Transform Target;
    DiscriminatorSideChannel discChannel;
    public const int numSkills=2;
    [Header("Skills")]
    [SerializeField]
    [Range(1,numSkills)]
    public int activeSkills;
    public bool useSkills = false;

    [Header("Reward")]
    public bool useShowReward;
    public bool useHideReward;

    [Header("Discriminator")]
    public bool useHideShowDisc = false;
    bool agentSpotted;
    int targetSurveillance = 0;

    // Start is called before the first frame update
    void Start()
    {
        agentSpotted = false;
        rBody = this.gameObject.GetComponent<Rigidbody>();
        discChannel = new DiscriminatorSideChannel();
        SideChannelManager.RegisterSideChannel(discChannel);
    }

    private void OnDestroy() {
        if (Academy.IsInitialized){
            SideChannelManager.UnregisterSideChannel(discChannel);
        }

        
    }
    // Update is called once per frame
    void FixedUpdate()
    {   
        
        float dist = Vector3.Distance(Target.position, this.transform.position);
        Debug.Log(agentSpotted);
        ShootRaysTarget();
        if (useShowReward){
            if (!agentSpotted){
                AddReward(-2.0f);
            }

        }

        if (useHideReward){
            if (agentSpotted){
                AddReward(-2.0f);
            }

        }

        if (dist>5f){
            AddReward(-0.1f);

        }
        else {
            AddReward(-0.1f- (dist - 5.0f)/50.0f);
        }

        
    }
    
    public override void OnEpisodeBegin(){

        currentTs = 0f;
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.eulerAngles = new Vector3(0,180,0);
        this.transform.position= new Vector3(0,0,10);

        float p = Random.value;
        float angleInit;
        if (p<=0.5){
            targetSurveillance = 1;
            angleInit = -15;
        }
        else {
            targetSurveillance = 0;
            angleInit = 15;
        }

        Target.transform.eulerAngles = new Vector3(0, angleInit, 0);

    }
    
    
    public void MoveAgent(ActionSegment<float> act)
    {
        /*var rotate = Vector3.zero;
        var forward = Vector3.zero;



        forward[2] = act[0];
        rotate[1] = act[1];

        this.transform.Rotate(rotate, Time.deltaTime * 100f);
        this.rBody.AddRelativeForce(Vector3.Max(forward,Vector3.zero) * m_speed,
            ForceMode.VelocityChange);*/


        var forward = Vector3.zero;
        forward[0] = act[0];
        forward[2] = act[1];
        this.rBody.AddForce(forward*m_speed,ForceMode.VelocityChange);

    }





    public override void CollectObservations(VectorSensor sensor)
    {
    // Target and Agent positions
    float dist = Vector3.Distance(Target.position, this.transform.position);
    Vector2 targetPos = Vector2.zero;
    Vector2 agentPos = Vector2.zero;
    targetPos[0] = Target.localPosition[0];
    targetPos[1] = Target.localPosition[2];
    agentPos[0] = this.transform.localPosition[0];
    agentPos[1] = this.transform.localPosition[2];

    sensor.AddObservation(targetPos);
    sensor.AddObservation(agentPos);
    sensor.AddObservation(dist);
    sensor.AddObservation(targetSurveillance);
    ShootRays(sensor);
    if (useSkills){
        sensor.AddOneHotObservation(activeSkills-1,numSkills);
    }
    List<float> discState = ShootRaysTarget();;

    if (useHideShowDisc){
        
        float seen;

        
        List<float> seenL= new List<float>();
        seen = Convert.ToSingle(agentSpotted);
        seenL.Add(seen);
        //seenL.Add(currentTs);
        //Debug.Log(currentTs);
 
        discChannel.SendDiscriminatorState(seenL);

    }
    else{
        discChannel.SendDiscriminatorState(discState);

    }


    for (int i = 0; i<discState.Count;i++){
            //Debug.Log(discState[i]);
        }
    
    currentTs++;

    }




    public override void OnActionReceived(ActionBuffers actionBuffers)

    {

        MoveAgent(actionBuffers.ContinuousActions);
    }
    

    public void OnTargetTouched(){
        SetReward(100f);
        EndEpisode();
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -1f*Input.GetAxis("Horizontal");
        continuousActionsOut[1] = -1f*Input.GetAxis("Vertical");
    } 


     void OnDrawGizmos()
    {
        // Display the explosion radius when selected
        //Gizmos.color = Color.white;
        //Gizmos.DrawWireSphere(Target.position, 5);
    }


    void ShootRays(VectorSensor sensor)
    {
        int raysToShoot = 10;
        float angle = 0;
        float maxDist = 5.0f;

        for  (int i=0; i<raysToShoot; i++) {
            float x = Mathf.Sin(angle)*5;
            float z = Mathf.Cos(angle)*5;
            angle += 2 * Mathf.PI / raysToShoot;
            Vector3 dir = new Vector3 (x, 0, z);
            RaycastHit hit;
            Debug.DrawLine(transform.position, transform.position+dir, Color.red);

            if (Physics.SphereCast(transform.position,0.5f, dir, out hit, maxDist)) {
                
                //Debug.Log(hit.collider.tag);
                sensor.AddObservation(hit.distance/maxDist);
                string tagName = hit.collider.tag;

                switch (tagName){
                    case "wall":
                        sensor.AddOneHotObservation(1,3);
                        break;

                    case "target":
                        sensor.AddOneHotObservation(2,3);
                        break;

                }

            }
            else{
                sensor.AddObservation(1);
                sensor.AddOneHotObservation(0,3);

            }
        }
    }

    public List<float> GetOneHotObservation(int index, int range){
        List<float> obs = new List<float>();
        for (int i=0;i<range;i++){
            if (i == index){
                obs.Add(1);
            }
            else{
                obs.Add(0);
            }
        }
        return obs;

    }

    private List<float> ShootRaysTarget(){

        List<float> discState = new List<float>();
        int raysToShoot = 10;
        float angle = -Mathf.PI/10 + Mathf.Deg2Rad * Target.transform.localEulerAngles.y;
        float maxDist = 20.0f;
        agentSpotted = false;
        for (int i=0; i<raysToShoot; i++) {  
            float x = Mathf.Sin(angle);
            float z = Mathf.Cos(angle);
            angle += 2*Mathf.PI/10 / (raysToShoot-1);

            Vector3 dir = new Vector3(x, 0, z);
            RaycastHit hit;
            Debug.DrawLine(Target.transform.localPosition, Target.transform.localPosition + maxDist*dir, Color.red);

            if (Physics.SphereCast(Target.transform.localPosition,0.5f, dir, out hit, maxDist)) {


                discState.Add(hit.distance/maxDist);
                string tagName = hit.collider.tag;

                switch (tagName){
                    case "wall":
                        discState = discState.Concat(GetOneHotObservation(1,3)).ToList();
                        break;
                    case "agent":
                        agentSpotted = true;
                        discState = discState.Concat(GetOneHotObservation(2,3)).ToList();
                        break;
                }

            }
            else{
                discState.Add(1);
                discState = discState.Concat(GetOneHotObservation(0,3)).ToList();

            }

        
        }
    return discState;

}
}
