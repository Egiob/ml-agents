using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CatchAgent : Agent
{

    public float m_speed = 0f;
    Rigidbody rBody;
    public Transform Target;
    
    public const int numSkills=1;
    [Header("Skills")]
    [SerializeField]
    [Range(1,numSkills)]
    public int activeSkills;
    public bool useSkills = false;
    // Start is called before the first frame update
    void Start()
    {
        rBody = this.gameObject.GetComponent<Rigidbody>();


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float dist = Vector3.Distance(Target.position, this.transform.position);
        if (dist>5f){
            AddReward(-0.1f);

        }
        else {
            AddReward(-0.1f- (dist - 5.0f)/50.0f);
        }


        
    }
    
    public override void OnEpisodeBegin(){


        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.eulerAngles = new Vector3(0,180,0);
        this.transform.position= new Vector3(0,0,10);

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
    if (useSkills){
        sensor.AddOneHotObservation(activeSkills-1,numSkills);
    }

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
}
