using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class CarController : MonoBehaviour {

    public float movmentSpeed = 1500;
    public float rotationSpeed = 500;

    private float movement = 0f;
    private float rotation = 0f;

    public WheelJoint2D backWheel;
    public WheelJoint2D frontWheel;

    public Rigidbody2D rb;

    public bool is2Motors;

    void Update()
    {
        movement = -Input.GetAxisRaw("Vertical") * movmentSpeed;
        rotation = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        if(movement == 0f)
        {
            backWheel.useMotor = false;
            if(is2Motors)
            {
                frontWheel.useMotor = false;
            }
            
        }
        else
        {
            backWheel.useMotor = true;
            if (is2Motors)
            {
                frontWheel.useMotor = false;
            }
            JointMotor2D motor = new JointMotor2D { motorSpeed = movement, maxMotorTorque = backWheel.motor.maxMotorTorque };
            backWheel.motor = motor;
            if (is2Motors)
            {
                frontWheel.motor = motor;
            }
        }

        rb.AddTorque(-rotation * rotationSpeed * Time.fixedDeltaTime);
    }
}
