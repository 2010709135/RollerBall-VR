using UnityEngine;
using System.Collections;

public class Ball_Lobby : MonoBehaviour {
    [SerializeField]
    protected float m_MovePower = 5; // The force added to the ball to move it.
    [SerializeField]
    protected bool m_UseTorque = true; // Whether or not to use torque to move the ball.
    [SerializeField]
    protected float m_MaxAngularVelocity = 7; // The maximum velocity the ball can rotate at.
    [SerializeField]
    protected float m_JumpPower = 2; // The force added to the ball when it jumps.

    protected const float k_GroundRayLength = 1f; // The length of the ray to check if the ball is grounded.
    protected Rigidbody m_Rigidbody;

    private Vector3 curPos;
    private Vector3 move;
    float h = 2;

    private Vector3 reachPoint;
    private bool arrive = false;

    System.Random ran;
    Transform tr;

    CharacterController controller;

    // Use this for initialization
    void Start () {
        m_Rigidbody = GetComponent<Rigidbody>();
        // Set the maximum angular velocity.
        GetComponent<Rigidbody>().maxAngularVelocity = m_MaxAngularVelocity;
        ran = new System.Random();
        makeReachPoint();
        m_MaxAngularVelocity = 100;

        makeReachPoint();
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        

        if (arrive)
        {
            makeReachPoint();
        }
        else
        {
            arrive = isReach();
            curPos = GetComponent<Transform>().localPosition;
            m_Rigidbody.AddTorque(curPos - reachPoint);
            //controller.Move(reachPoint - curPos);
        }

        //curPos = GetComponent<Transform>().localPosition;
        //move = (curPos  +Vector3.left).normalized;
        //move = (curPos + Vector3.left + Vector3.forward);
        //m_Rigidbody.AddTorque(move);


        //  m_Rigidbody.AddTorque(new Vector3(move.z, move.y, -move.x) * m_MovePower);

        //   nextPos = GetComponent<Transform>().localPosition + new Vector3()
        //   m_Rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * m_MovePower);
    }

    void makeReachPoint()
    {

        float x, y, z;
        x = (float) ran.Next(223, 237);
        y = (float) 0.5;
        z = (float) ran.Next(239, 265);

        reachPoint = new Vector3(x, y, z);
    }

    bool isReach()
    {
        tr = GetComponent<Transform>();

        if(reachPoint == tr.localPosition)
        {
            return true;
        }else
            return false;
    }
}
