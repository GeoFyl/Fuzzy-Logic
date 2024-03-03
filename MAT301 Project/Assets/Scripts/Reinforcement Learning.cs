using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ReinforcementLearning : MonoBehaviour
{
    int num_actions_ = 3;
    int num_states_ = 3;

    enum State { OBSTACLE_IN_FRONT, GOAL_IN_FRONT, NONE_IN_FRONT };
    enum Action { MOVE_FORWARD, ROTATE_RIGHT, ROTATE_LEFT };

    float[,] Q = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
    String[] action_to_string_ = new String[3] { "Move Forward", "Rotate Right", "Rotate Left" };
    String[] state_to_string_ = new String[3] { "Obstacle in front", "Goal in front", "Nothing in front" };

    //Important stuff
    [SerializeField]
    float view_distance_ = 20;
    [SerializeField]
    float random_chance_ = 10;


    //Extra stuff
    [SerializeField]
    bool draw_debug_;
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
            State current_state = State.NONE_IN_FRONT;
            RaycastHit hit;
            Vector3 origin = transform.position;
            origin.y *= 0.25f;
            if (draw_debug_) Debug.DrawLine(origin, transform.forward * view_distance_ + origin, Color.green);
            if (Physics.Raycast(origin, transform.forward, out hit, view_distance_))
            {
                if (hit.transform.name == "Obstacle") current_state = State.OBSTACLE_IN_FRONT;
                else if (hit.transform.name == "Goal") current_state = State.GOAL_IN_FRONT;
            }

            float best_value = Q[(int)current_state, 0];
            int index = 0;
            for(int i = 0; i < num_actions_; i++)
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
        }
    }
}
