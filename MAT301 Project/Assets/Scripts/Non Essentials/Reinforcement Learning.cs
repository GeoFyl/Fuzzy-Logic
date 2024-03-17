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
    int index_ = -1;
    float best_value_;

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
    float distance_to_goal_;

    float time_ = 0;

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
        distance_to_goal_ = (transform.position - goal_.transform.position).magnitude;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!reached_goal_)
        {
           // time_ += Time.deltaTime;
            //if (time_ >= 0.2)
           // {
           //     time_ = 0;

                // Describe State
                State new_state = GetState();

                if (index_ > -1) // Can't do this the first time
                {
                    // Get reward for previous action
                    float reward = CalculateReward(new_state);

                    // Print out which action the AI ran.
                    Debug.Log(string.Format("State was: {0} Ran action: {1}, Reward recieved: {2}", state_to_string_[(int)previous_state_], action_to_string_[index_], reward));

                    // Update Q Matrix
                    Q[(int)previous_state_, index_] += learning_rate_ * (reward + discounting_rate_ * best_value_ - Q[(int)previous_state_, index_]);
                }
                previous_state_ = new_state;

                // Choose Action
                best_value_ = Q[(int)new_state, 0];
                index_ = 0;
                for (int i = 0; i < num_actions_; i++)
                {
                    if (Q[(int)new_state, i] > best_value_)
                    {
                        best_value_ = Q[(int)new_state, i];
                        index_ = i;
                    }
                }
                if (Random.Range(0, 99) < random_chance_)
                {
                    index_ = Random.Range(0, num_actions_);
                }
                Action chosen_action = (Action)index_;

                // Perform the Action
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

            //}

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
    }

    float CalculateReward(State new_state)
    {
        float reward = 0;
        if (new_state != previous_state_)
        {
            switch (previous_state_)
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
                default: break;
            }
        }
        else
        {
            switch (new_state)
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
                default: break;
            }
        }
        float new_distance = (transform.position - goal_.transform.position).magnitude;
        if (new_distance < distance_to_goal_) reward += 100f * (distance_to_goal_ - new_distance);
        else if (new_distance > distance_to_goal_) reward -= 100f * (new_distance - distance_to_goal_);

        //Debug.Log("old dist: " + distance_to_goal_ + ", new dist: " + new_distance);

        distance_to_goal_ = new_distance;

        return reward;
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
