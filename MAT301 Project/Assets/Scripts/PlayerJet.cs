using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Jet : MonoBehaviour
{
    Rigidbody rigidbody_;

    [SerializeField]
    float acceleration_ = 50;
    [SerializeField]
    float downward_acceleration_ = 15;
    [SerializeField]
    float upward_acceleration_ = 20;
    [SerializeField]
    float x_position_limit_ = 8;
    [SerializeField]
    float z_position_limit_ = 8;
    [SerializeField]
    float y_position_limit_ = 8;


    // Start is called before the first frame update
    void Start()
    {
        rigidbody_ = GetComponent<Rigidbody>();
        Physics.gravity = Vector3.down * downward_acceleration_;
    }

    private void FixedUpdate()
    {
        //Vector3 new_direction = Vector3.zero;
        if (rigidbody_ != null) { 
            if(transform.position.x > x_position_limit_)
            {
                transform.position = new Vector3(x_position_limit_, transform.position.y, transform.position.z);
                rigidbody_.velocity = new Vector3(0, rigidbody_.velocity.y, rigidbody_.velocity.z);
            }
            else if (transform.position.x < -x_position_limit_)
            {
                transform.position = new Vector3(-x_position_limit_, transform.position.y, transform.position.z);
                rigidbody_.velocity = new Vector3(0, rigidbody_.velocity.y, rigidbody_.velocity.z);
            }
            if(transform.position.z > z_position_limit_)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, z_position_limit_);
                rigidbody_.velocity = new Vector3(rigidbody_.velocity.x, rigidbody_.velocity.y, 0);
            }
            else if (transform.position.z < -z_position_limit_)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, -z_position_limit_);
                rigidbody_.velocity = new Vector3(rigidbody_.velocity.x, rigidbody_.velocity.y, 0);
            }
            if(transform.position.y > y_position_limit_)
            {
                transform.position = new Vector3(transform.position.x, y_position_limit_, transform.position.z);
                rigidbody_.velocity = new Vector3(rigidbody_.velocity.x, 0, rigidbody_.velocity.z);
            }

            rigidbody_.AddForce(transform.right * Input.GetAxis("Horizontal") * acceleration_);
            rigidbody_.AddForce(transform.forward * Input.GetAxis("Vertical") * acceleration_);
            if (Input.GetKey(KeyCode.Space)) rigidbody_.AddForce(transform.up * upward_acceleration_);
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Smoothly tilt plane mesh according to direction
        float tiltAroundX = Input.GetAxis("Horizontal") * 20;
        Quaternion target = Quaternion.Euler(tiltAroundX, 90, 0);
        transform.GetChild(0).localRotation = Quaternion.Slerp(transform.GetChild(0).localRotation, target, Time.deltaTime * 3);

        // Rotate propeller 
        transform.GetChild(0).GetChild(2).GetChild(0).GetChild(0).Rotate(Vector3.forward, -540 * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
