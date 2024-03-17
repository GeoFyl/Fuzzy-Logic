using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FLS;
using FLS.Rules;
using FLS.MembershipFunctions;
using static UnityEditor.Experimental.GraphView.GraphView;
using Unity.VisualScripting;

public class FuzzyBrain : MonoBehaviour
{
    enum MovementType { Random, Predefined };
    [SerializeField]
    MovementType movement_ = MovementType.Random;
    [SerializeField]
    float move_speed_ = 10;
    [SerializeField]
    float x_position_limit_ = 8;
    [SerializeField]
    float y_position_limit_ = 8;

    Vector3 start_position_;
    Vector3 target_position_;

    [SerializeField]
    GameObject missile_;
    [SerializeField]
    float missile_forward_speed_ = 15;

    IFuzzyEngine sideways_engine_, forward_engine_, vertical_engine_;
    LinguisticVariable sideways_distance_, sideways_direction_, forward_distance_, forward_direction_, vertical_distance_, vertical_direction_;

    // Start is called before the first frame update
    void Start()
    {
        start_position_ = transform.position;
        target_position_.x = Random.Range(-x_position_limit_, x_position_limit_);
        target_position_.y = Random.Range(-y_position_limit_, y_position_limit_);
        target_position_.z = transform.position.z;

        // ---------- Setup Fuzzy Inference System -----------

        // Engine for forward direction movement (z axis)
        #region
       // forward_engine_ = new FuzzyEngineFactory().Default();

        forward_distance_ = new LinguisticVariable("forward_distance");
        var very_behind_target = forward_distance_.MembershipFunctions.AddTrapezoid("very behind", -65, -30, -20, -15);
        var behind_target = forward_distance_.MembershipFunctions.AddTrapezoid("behind", -20, -15, -10, 4);
        var alongside_target = forward_distance_.MembershipFunctions.AddTriangle("none", -5, 0, 5);
        var in_front_of_target = forward_distance_.MembershipFunctions.AddTrapezoid("infront", 4, 10, 15, 20);
        var very_in_front_of_target = forward_distance_.MembershipFunctions.AddTrapezoid("very infront", 15, 20, 30, 65);
        //forward_direction_ = new LinguisticVariable("forward_direction");
        //var move_alot_back = forward_direction_.MembershipFunctions.AddTrapezoid("very back", -20, -15, -10, -5);
        //var move_back = forward_direction_.MembershipFunctions.AddTrapezoid("back", -10, -5, -1, 0);
        //var stay_alongside = forward_direction_.MembershipFunctions.AddTriangle("none", -0.5, 0, 0.5);
        //var move_forward = forward_direction_.MembershipFunctions.AddTrapezoid("forward", 0, 1, 5, 10);
        //var move_alot_forward = forward_direction_.MembershipFunctions.AddTrapezoid("very forward", 5, 10, 15, 20);
        //var forward_rule_1 = Rule.If(forward_distance_.Is(very_in_front_of_target)).Then(forward_direction_.Is(move_alot_back));
        //var forward_rule_2 = Rule.If(forward_distance_.Is(in_front_of_target)).Then(forward_direction_.Is(move_back));
        //var forward_rule_3 = Rule.If(forward_distance_.Is(alongside_target)).Then(forward_direction_.Is(stay_alongside));
        //var forward_rule_4 = Rule.If(forward_distance_.Is(behind_target)).Then(forward_direction_.Is(move_forward));
        //var forward_rule_5 = Rule.If(forward_distance_.Is(very_behind_target)).Then(forward_direction_.Is(move_alot_forward));

        //.Rules.Add(forward_rule_1, forward_rule_2, forward_rule_3, forward_rule_4, forward_rule_5);
        #endregion

        // Engine for vertical movement
        #region
        vertical_engine_ = new FuzzyEngineFactory().Default();

        vertical_distance_ = new LinguisticVariable("vertical_distance");
        var very_below_target = vertical_distance_.MembershipFunctions.AddTrapezoid("very below", -15, -8, -7, -5);
        var below_target = vertical_distance_.MembershipFunctions.AddTrapezoid("below", -7, -5, -3, 0.5);
        var same_height = vertical_distance_.MembershipFunctions.AddTriangle("none", -1, 0, 1);
        var above_target = vertical_distance_.MembershipFunctions.AddTrapezoid("above", 0, 3, 5, 7);
        var very_above_target = vertical_distance_.MembershipFunctions.AddTrapezoid("very above", 5, 7, 8, 15);
        vertical_direction_ = new LinguisticVariable("vertical_direction");
        var move_alot_down = vertical_direction_.MembershipFunctions.AddTrapezoid("alot down", -20, -15, -10, -5);
        var move_down = vertical_direction_.MembershipFunctions.AddTrapezoid("down", -10, -5, -1, 0);
        var stay_same_height = vertical_direction_.MembershipFunctions.AddTriangle("none", -0.5, 0, 0.5);
        var move_up = vertical_direction_.MembershipFunctions.AddTrapezoid("up", 0, 1, 5, 10);
        var move_alot_up = vertical_direction_.MembershipFunctions.AddTrapezoid("alot up", 5, 10, 15, 20);
        var vertical_rule_1 = Rule.If(vertical_distance_.Is(very_above_target)).Then(vertical_direction_.Is(move_alot_down));
        var vertical_rule_x = Rule.If(vertical_distance_.Is(above_target).And(forward_distance_.Is(alongside_target))).Then(vertical_direction_.Is(move_alot_down));
        var vertical_rule_2 = Rule.If(vertical_distance_.Is(above_target)).Then(vertical_direction_.Is(move_down));
        var vertical_rule_3 = Rule.If(vertical_distance_.Is(same_height)).Then(vertical_direction_.Is(stay_same_height));
        var vertical_rule_4 = Rule.If(vertical_distance_.Is(below_target)).Then(vertical_direction_.Is(move_up));
        var vertical_rule_5 = Rule.If(vertical_distance_.Is(below_target).And(forward_distance_.Is(alongside_target))).Then(vertical_direction_.Is(move_alot_up));
        var vertical_rule_y = Rule.If(vertical_distance_.Is(very_below_target)).Then(vertical_direction_.Is(move_alot_up));
        vertical_engine_.Rules.Add(vertical_rule_1, vertical_rule_2, vertical_rule_3, vertical_rule_4, vertical_rule_5, vertical_rule_x, vertical_rule_y);
        #endregion

        // Engine for sideways movement (x axis)
        #region
        sideways_engine_ = new FuzzyEngineFactory().Default();

        sideways_distance_ = new LinguisticVariable("sideways_distance");
        var very_left_of_target = sideways_distance_.MembershipFunctions.AddTrapezoid("very left", -30, -30, -15, -10);
        var left_of_target = sideways_distance_.MembershipFunctions.AddTrapezoid("left", -15, -10, -5, -0.5);
        var inline_with_target = sideways_distance_.MembershipFunctions.AddTriangle("none", -1, 0, 1);
        var right_of_target = sideways_distance_.MembershipFunctions.AddTrapezoid("right", 0.5, 5, 10, 15);
        var very_right_of_target = sideways_distance_.MembershipFunctions.AddTrapezoid("very right", 10, 15, 30, 30);
        sideways_direction_ = new LinguisticVariable("sideways_direction");
        var steer_alot_left = sideways_direction_.MembershipFunctions.AddTrapezoid("very left", -50, -15, -10, -5);
        var steer_left = sideways_direction_.MembershipFunctions.AddTrapezoid("left", -10, -5, -1, 0);
        var stay_centred = sideways_direction_.MembershipFunctions.AddTriangle("none", -0.5, 0, 0.5);
        var steer_right = sideways_direction_.MembershipFunctions.AddTrapezoid("right", 0, 1, 5, 10);
        var steer_alot_right = sideways_direction_.MembershipFunctions.AddTrapezoid("very right", 5, 10, 15, 50);
        var sideways_rule_1 = Rule.If(sideways_distance_.Is(very_right_of_target)).Then(sideways_direction_.Is(steer_alot_left));
        var side_test_rule = Rule.If(sideways_distance_.Is(right_of_target).And(forward_distance_.Is(alongside_target))).Then(sideways_direction_.Is(steer_alot_left));
        var sideways_rule_2 = Rule.If(sideways_distance_.Is(right_of_target)).Then(sideways_direction_.Is(steer_left));
        var sideways_rule_3 = Rule.If(sideways_distance_.Is(inline_with_target)).Then(sideways_direction_.Is(stay_centred));
        var sideways_rule_4 = Rule.If(sideways_distance_.Is(left_of_target)).Then(sideways_direction_.Is(steer_right));
        var side_test_rule2 = Rule.If(sideways_distance_.Is(left_of_target).And(forward_distance_.Is(alongside_target))).Then(sideways_direction_.Is(steer_alot_right));
        var sideways_rule_5 = Rule.If(sideways_distance_.Is(very_left_of_target)).Then(sideways_direction_.Is(steer_alot_right));

        sideways_engine_.Rules.Add(sideways_rule_1, sideways_rule_2, sideways_rule_3, sideways_rule_4, sideways_rule_5, side_test_rule, side_test_rule2);
        #endregion
    }

    public float DefuzzifySideways(float sideways, float forward)
    {
        return (float)sideways_engine_.Defuzzify(new { sideways_distance = (double)sideways, forward_distance = (double)forward });
    }

    public float DefuzzifyVertical(float vertical, float forward)
    {
        return (float)vertical_engine_.Defuzzify(new { vertical_distance = (double)vertical, forward_distance = (double)forward });
       // return (float)vertical_engine_.Defuzzify(new { vertical_distance = (double)vertical });
    }

    private void FixedUpdate()
    {
        switch(movement_)
        {
            case MovementType.Random:
                if(transform.position.x == target_position_.x && transform.position.y == target_position_.y)
                {
                    var new_missile = Instantiate(missile_, transform.position, Quaternion.identity);
                    new_missile.GetComponent<FuzzyMissile>().InitMissile(this, missile_forward_speed_, !transform.GetChild(0).gameObject.active);

                    start_position_ = target_position_;
                    target_position_.x = Random.Range(-x_position_limit_, x_position_limit_);
                    target_position_.y = Random.Range(0, y_position_limit_);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, target_position_, move_speed_ * Time.deltaTime);

                    // Smoothly tilt helicopter mesh according to direction
                    float x_delta = target_position_.x - transform.position.x;
                    float tiltAroundZ = (x_delta > 0 ? 1 : -1) * 15;
                    Quaternion target = Quaternion.Euler(-79, 180, tiltAroundZ);
                    transform.GetChild(0).localRotation = Quaternion.Slerp(transform.GetChild(0).localRotation, target, Time.deltaTime * 3);
                }
                break;
            default: break;
        }   
    }

    private void Update()
    {
        //if(Input.GetMouseButtonDown(0))
        //{
        //    var new_missile = Instantiate(missile_, transform.position, Quaternion.identity);
        //    new_missile.GetComponent<FuzzyMissile>().InitMissile(this, missile_forward_speed_, !transform.GetChild(0).gameObject.active);
        //}
    }

}
