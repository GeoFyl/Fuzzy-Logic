using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class ReinforcementLearning : MonoBehaviour
{
    int num_actions_ = 3;
    int num_states_ = 3;

    enum State { OBSTACLE_IN_FRONT, GOAL_IN_FRONT, NONE_IN_FRONT };
    enum Action { MOVE_FORWARD, ROTATE_RIGHT, ROTATE_LEFT };
    State previous_state_ = State.NONE_IN_FRONT;
    Action chosen_action_;

    float[,] Q = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
    String[] action_to_string_ = new String[3] { "Move Forward", "Rotate Right", "Rotate Left" };
    String[] state_to_string_ = new String[3] { "Obstacle in front", "Goal in front", "Nothing in front" };

    //Important stuff
    [SerializeField]
    float view_distance_ = 20;
    [SerializeField]
    float rotate_angles_ = 45;
    [SerializeField]
    float random_chance_ = 10;
    [SerializeField]
    float learning_rate_ = 0.1f; 
    [SerializeField]
    float discounting_rate_ = 0.7f; 

    //Extra stuff
    [SerializeField]
    bool draw_debug_;
    bool reached_goal_ = false;
    GameObject goal_;
    GameObject current_obstacle_;

    void printMatrix()
    {
        Debug.Log("Q Matrix");
        for (int i = 0; i < num_states_; i++)
        {
            for(int j = 0; j < num_actions_; j++)
            {
                Debug.Log(string.Format("For state {0}, the action {1} has a value of: {2}", state_to_string_[i], action_to_string_[j], Q[i,j]));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "Goal")
        {
            reached_goal_ = true;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            printMatrix();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        goal_ = GameObject.Find("Goal");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!reached_goal_)
        {
            // Describe State
            State current_state = GetState();

            // Choose Action
            float best_value = Q[(int)current_state, 0];
            int index = 0;
            for (int i = 0; i < num_actions_; i++)
            {
                if (Q[(int)current_state, i] > best_value)
                {
                    best_value = Q[(int)current_state, i];
                    index = i;
                }
            }
            if (Random.Range(0, 99) < random_chance_)
            {
                index = Random.Range(0, num_actions_);
            }
            Action chosen_action = (Action)index;

            // Perform the Action
            float distance_to_goal = (transform.position - goal_.transform.position).magnitude;
            switch (chosen_action)
            {
                case Action.MOVE_FORWARD:
                   // transform.Translate(Vector3.forward * 5 * Time.deltaTime);
                    GetComponent<Rigidbody>().velocity = transform.forward * 5;
                    break;
                case Action.ROTATE_LEFT:
                    GetComponent<Rigidbody>().velocity = Vector3.zero;
                    transform.Rotate(-Vector3.up * rotate_angles_);
                    break;
                case Action.ROTATE_RIGHT:
                    GetComponent<Rigidbody>().velocity = Vector3.zero;
                    transform.Rotate(Vector3.up * rotate_angles_);
                    break;
                default: break;
            }

            // Calculate Reward
            float reward = 0;
            State new_state = GetState();
           
            if (new_state != current_state)
            {
                switch (current_state)
                {
                    case State.OBSTACLE_IN_FRONT:
                        reward += 5;
                        break;
                    case State.GOAL_IN_FRONT:
                        reward -= 10;
                        break;
                    default: break;
                }
                switch (new_state)
                {
                    case State.GOAL_IN_FRONT:
                        reward += 10;
                        break;
                    case State.OBSTACLE_IN_FRONT:
                        reward -= 5;
                        break;
                }
            }
            else
            {
                switch (current_state)
                {
                    case State.OBSTACLE_IN_FRONT:
                        reward -= 10;
                        //Vector3.Dot(transform.forward, )
                       // transform.forward.
                        break;
                    case State.GOAL_IN_FRONT:
                        reward += 5;
                        break;
                    case State.NONE_IN_FRONT:
                        reward += 1;
                        break;
                }
            }
            float new_distance = (transform.position - goal_.transform.position).magnitude;
            if (new_distance < distance_to_goal) reward *= 100f * (distance_to_goal - new_distance);
            else if (new_distance > distance_to_goal) reward *= -100f * (new_distance - distance_to_goal); 

            // Update Q Matrix
            Q[(int)current_state, index] += learning_rate_ * (reward + discounting_rate_ * best_value - Q[(int)current_state, index]);

            // Print out which action the AI ran.
            //Debug.Log(string.Format("State was: {0} Ran action: {1}, Reward recieved: {2}", state_to_string_[(int)current_state], action_to_string_[index], reward));
        }

        //if (Input.GetKey(KeyCode.W))
        //{
        //    transform.Translate(Vector3.forward * 5 * Time.deltaTime);
        //}
        //else if (Input.GetKey(KeyCode.Q))
        //{
        //    transform.Rotate(-Vector3.up * 30 * Time.deltaTime);
        //}
        //else if (Input.GetKey(KeyCode.E))
        //{
        //    transform.Rotate(Vector3.up * 30 * Time.deltaTime);
        //}
    }

    State GetState()
    {
        State state = State.NONE_IN_FRONT;
        RaycastHit hit;
        Vector3 origin = transform.position;
        origin.y *= 0.25f;
        if (draw_debug_) Debug.DrawLine(origin, transform.forward * view_distance_ + origin, Color.green);
        if (Physics.Raycast(origin, transform.forward, out hit, view_distance_))
        {
            if (hit.transform.tag == "Obstacle")
            {
                //current_obstacle_
                state = State.OBSTACLE_IN_FRONT;
            }
            else if (hit.transform.name == "Goal") state = State.GOAL_IN_FRONT;
        }
        return state;
    }
}
