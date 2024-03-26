using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Jet : MonoBehaviour
{
    Rigidbody rigidbody_;

    [SerializeField]
    GameObject pause_menu_;
    [SerializeField]
    GameObject player_missile_;

    float next_missile_fire_time_ = 0.5f;
    float missile_fire_timer_ = 0;

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

    [SerializeField]
    bool player_controlled_ = true;
    Vector3 target_position_;
    [SerializeField]
    float testing_movement_speed = 20;
   // bool reached_target_position_ = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody_ = GetComponent<Rigidbody>();
        Physics.gravity = Vector3.down * downward_acceleration_;

        if(!player_controlled_)
        {
            BeginTesting();
        }
    }

    public void BeginTesting()
    {
        player_controlled_ = false;
        GetComponent<Rigidbody>().useGravity = false;
        target_position_.x = Random.Range(-x_position_limit_ + 0.5f, x_position_limit_ - 0.5f);
        target_position_.z = Random.Range(-z_position_limit_ + 0.5f, z_position_limit_ - 0.5f);
        target_position_.y = Random.Range(0.5f, y_position_limit_ - 0.5f);

    }

    public void EndTesting()
    {
        player_controlled_ = true;
        GetComponent<Rigidbody>().useGravity = true;
    }

    private void FixedUpdate()
    {
        if (player_controlled_) { 
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
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, target_position_, testing_movement_speed * Time.deltaTime);
            if (transform.position.y == target_position_.y && transform.position.x == target_position_.x && transform.position.z == target_position_.z)
            {
                target_position_.x = Random.Range(-x_position_limit_ + 0.5f, x_position_limit_ - 0.5f);
                target_position_.z = Random.Range(-z_position_limit_ + 0.5f, z_position_limit_ - 0.5f);
                target_position_.y = Random.Range(0.5f, y_position_limit_ - 0.5f);
            }

            missile_fire_timer_ += Time.deltaTime;
            if (missile_fire_timer_ >= next_missile_fire_time_)
            {
                missile_fire_timer_ = 0;
                next_missile_fire_time_ = Random.Range(0.5f, 2f);
                var new_missile = Instantiate(player_missile_, transform.position, Quaternion.identity);
                new_missile.GetComponent<PlayerMissile>().InitMissile(!transform.GetChild(0).gameObject.active);
            }
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
        if (player_controlled_ && !pause_menu_.GetComponent<PauseMenu>().paused && Input.GetMouseButtonDown(0))
        {
            var new_missile = Instantiate(player_missile_, transform.position, Quaternion.identity);
            new_missile.GetComponent<PlayerMissile>().InitMissile(!transform.GetChild(0).gameObject.active);
        }
    }
}
