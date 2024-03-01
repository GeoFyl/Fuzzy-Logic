using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ReinforcementLearning : MonoBehaviour
{
    int num_actions_ = 3;
    int num_states_ = 3;

    enum State { OBSTACLE_IN_FRONT, GOAL_IN_FRONT, NONE_IN_FRONT };
    enum Action { MOVE_FORWARD, ROTATE_RIGHT, ROTATE_LEFT };

    float[,] Q = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
    String[] action_to_string_ = new String[3] { "Move Forward", "Rotate Right", "Rotate Left" };
    String[] state_to_string_ = new String[3] { "Obstacle in front", "Goal in front", "Nothing in front" };

    bool reached_goal_ = false;

    void printMatrix()
    {
        Debug.Log("Q Matrix");
        for (int i = 0; i < num_states_; i++)
        {
            for(int j = 0; j < num_actions_; j++)
            {
                Debug.Log(string.Format("For state %s, the action %s has a value of: %f", state_to_string_[i], action_to_string_[j], Q[i,j]));
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    { 
        if (collision.collider.transform.name == "Goal")
        {
            reached_goal_ = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!reached_goal_)
        {
            
            State current_state;

        }
    }
}
