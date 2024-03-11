using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Car : MonoBehaviour
{
    Rigidbody rigidbody_;

    [SerializeField]
    float accelerate = 10;
    [SerializeField]
    float x_position_limit_ = 8;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody_ = GetComponent<Rigidbody>();
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

            //if (Input.GetKey(KeyCode.A))
            //{
            //    new_direction -= transform.right;
            //}
            //if (Input.GetKey(KeyCode.D))
            //{
            //    new_direction += transform.right;
            //}
            rigidbody_.AddForce(transform.right * Input.GetAxis("Horizontal") * accelerate);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
