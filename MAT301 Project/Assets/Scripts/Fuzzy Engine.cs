using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FLS;
using FLS.Rules;
using FLS.MembershipFunctions;
using System;
using Random = UnityEngine.Random;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using TMPro;
using System.Linq;

//public class MembershipFunctionValues : MonoBehaviour
//{
//    public IMembershipFunction function;
//    public float[] values;
//}

public class FuzzyBrain : MonoBehaviour
{

   // List<GameObject> graph_lines_ = new List<GameObject>();

    enum MovementType { Random, Predefined };
    [SerializeField]
    MovementType movement_ = MovementType.Random;
    [SerializeField]
    float move_speed_ = 10;
    [SerializeField]
    float x_position_limit_ = 8;
    [SerializeField]
    float y_position_limit_ = 8;

    [SerializeField]
    GameObject graph_object_;
    [SerializeField]
    GameObject membership_object_;
    [SerializeField]
    GameObject value1_object_;
    [SerializeField]
    GameObject value2_object_;
    [SerializeField]
    GameObject value3_object_;
    [SerializeField]
    GameObject value4_object_;
    TMP_InputField value1_, value2_, value3_, value4_;
    TMP_Dropdown membership_dropdown_;
    SimplestPlot graph_;
    int currently_selected_linguistic_;
    int currently_selected_membership_;

    Vector3 target_position_;

    [SerializeField]
    GameObject missile_;
    [SerializeField]
    float missile_forward_speed_ = 15;

    IFuzzyEngine sideways_engine_, forward_engine_, vertical_engine_;
    LinguisticVariable sideways_distance_, sideways_direction_, forward_distance_, forward_direction_, vertical_distance_, vertical_direction_;
    IMembershipFunction very_behind_target, behind_target, alongside_target, in_front_of_target, very_in_front_of_target,
                        very_below_target, below_target, same_height, above_target, very_above_target,
                        move_alot_down, move_down, stay_same_height, move_up, move_alot_up,
                        very_left_of_target, left_of_target, inline_with_target, right_of_target, very_right_of_target,
                        steer_alot_left, steer_left, stay_centred, steer_right, steer_alot_right;
    List<List<MembershipFunctionValues>> mf_values_ = new List<List<MembershipFunctionValues>>(); 

    // Start is called before the first frame update
    void Start()
    {
        target_position_.x = Random.Range(-x_position_limit_, x_position_limit_);
        target_position_.y = Random.Range(-y_position_limit_, y_position_limit_);
        target_position_.z = transform.position.z;

        // ---------- Setup Membership Function values -----------
        InitMembershipFuncs();
        CreateFuzzyEngines();

        InitGraph();
        UpdateGraph(0);
        DisplayValueInputs(0);
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

    private void InitMembershipFuncs()
    {
        mf_values_.Add(new List<MembershipFunctionValues>());
        mf_values_[0].Add(new MembershipFunctionValues("Far Behind", -65, -30, -20, -15));
        mf_values_[0].Add(new MembershipFunctionValues("Behind", -20, -15, -10, -4));
        mf_values_[0].Add(new MembershipFunctionValues("Alongside", -5, 0, 0, 5));
        mf_values_[0].Add(new MembershipFunctionValues("In Front", 4, 10, 15, 20));
        mf_values_[0].Add(new MembershipFunctionValues("Far In Front", 15, 20, 30, 65));

        mf_values_.Add(new List<MembershipFunctionValues>());
        mf_values_[1].Add(new MembershipFunctionValues("Far Below", -15, -8, -7, -5));
        mf_values_[1].Add(new MembershipFunctionValues("Below", -7, -5, -3, -0.5f));
        mf_values_[1].Add(new MembershipFunctionValues("None", -1, 0, 0, 1));
        mf_values_[1].Add(new MembershipFunctionValues("Above", 0.5f, 3, 5, 7));
        mf_values_[1].Add(new MembershipFunctionValues("Far Above", 5, 7, 8, 15));

        mf_values_.Add(new List<MembershipFunctionValues>());
        mf_values_[2].Add(new MembershipFunctionValues("Far Down", -20, -15, -10, -5));
        mf_values_[2].Add(new MembershipFunctionValues("Down", -10, -5, -1, 0));
        mf_values_[2].Add(new MembershipFunctionValues("None", -0.5f, 0, 0, 0.5f));
        mf_values_[2].Add(new MembershipFunctionValues("Up", 0, 1, 5, 10));
        mf_values_[2].Add(new MembershipFunctionValues("Far Up", 5, 10, 15, 20));

        mf_values_.Add(new List<MembershipFunctionValues>());
        mf_values_[3].Add(new MembershipFunctionValues("Far Left", -30, -30, -15, -10));
        mf_values_[3].Add(new MembershipFunctionValues("Left", -15, -10, -5, -0.5f));
        mf_values_[3].Add(new MembershipFunctionValues("None", -1, 0, 0, 1));
        mf_values_[3].Add(new MembershipFunctionValues("Right", 0.5f, 5, 10, 15));
        mf_values_[3].Add(new MembershipFunctionValues("Far Right", 10, 15, 30, 30));

        mf_values_.Add(new List<MembershipFunctionValues>());
        mf_values_[4].Add(new MembershipFunctionValues("Far Left", -50, -15, -10, -5));
        mf_values_[4].Add(new MembershipFunctionValues("Left", -10, -5, -1, 0));
        mf_values_[4].Add(new MembershipFunctionValues("None", -0.5f, 0, 0, 0.5f));
        mf_values_[4].Add(new MembershipFunctionValues("Right", 0, 1, 5, 10));
        mf_values_[4].Add(new MembershipFunctionValues("Far Right", 5, 10, 15, 50));
    }

    void CreateFuzzyEngines()
    {
        forward_distance_ = new LinguisticVariable("forward_distance");
        very_behind_target = forward_distance_.MembershipFunctions.AddTrapezoid(mf_values_[0][0].function_name, mf_values_[0][0].values[0], mf_values_[0][0].values[1], mf_values_[0][0].values[2], mf_values_[0][0].values[3]);
        behind_target = forward_distance_.MembershipFunctions.AddTrapezoid(mf_values_[0][1].function_name, mf_values_[0][1].values[0], mf_values_[0][1].values[1], mf_values_[0][1].values[2], mf_values_[0][1].values[3]);
        alongside_target = forward_distance_.MembershipFunctions.AddTriangle(mf_values_[0][2].function_name, mf_values_[0][2].values[0], mf_values_[0][2].values[1], mf_values_[0][2].values[3]);
        in_front_of_target = forward_distance_.MembershipFunctions.AddTrapezoid(mf_values_[0][3].function_name, mf_values_[0][3].values[0], mf_values_[0][3].values[1], mf_values_[0][3].values[2], mf_values_[0][3].values[3]);
        very_in_front_of_target = forward_distance_.MembershipFunctions.AddTrapezoid(mf_values_[0][4].function_name, mf_values_[0][4].values[0], mf_values_[0][4].values[1], mf_values_[0][4].values[2], mf_values_[0][4].values[3]);

        vertical_distance_ = new LinguisticVariable("vertical_distance");
        very_below_target = vertical_distance_.MembershipFunctions.AddTrapezoid(mf_values_[1][0].function_name, mf_values_[1][0].values[0], mf_values_[1][0].values[1], mf_values_[1][0].values[2], mf_values_[1][0].values[3]);
        below_target = vertical_distance_.MembershipFunctions.AddTrapezoid(mf_values_[1][1].function_name, mf_values_[1][1].values[0], mf_values_[1][1].values[1], mf_values_[1][1].values[2], mf_values_[1][1].values[3]);
        same_height = vertical_distance_.MembershipFunctions.AddTriangle(mf_values_[1][2].function_name, mf_values_[1][2].values[0], mf_values_[1][2].values[1], mf_values_[1][2].values[3]);
        above_target = vertical_distance_.MembershipFunctions.AddTrapezoid(mf_values_[1][3].function_name, mf_values_[1][3].values[0], mf_values_[1][3].values[1], mf_values_[1][3].values[2], mf_values_[1][3].values[3]);
        very_above_target = vertical_distance_.MembershipFunctions.AddTrapezoid(mf_values_[1][4].function_name, mf_values_[1][4].values[0], mf_values_[1][4].values[1], mf_values_[1][4].values[2], mf_values_[1][4].values[3]);

        vertical_direction_ = new LinguisticVariable("vertical_direction");
        move_alot_down = vertical_direction_.MembershipFunctions.AddTrapezoid(mf_values_[2][0].function_name, mf_values_[2][0].values[0], mf_values_[2][0].values[1], mf_values_[2][0].values[2], mf_values_[2][0].values[3]);
        move_down = vertical_direction_.MembershipFunctions.AddTrapezoid(mf_values_[2][1].function_name, mf_values_[2][1].values[0], mf_values_[2][1].values[1], mf_values_[2][1].values[2], mf_values_[2][1].values[3]);
        stay_same_height = vertical_direction_.MembershipFunctions.AddTriangle(mf_values_[2][2].function_name, mf_values_[2][2].values[0], mf_values_[2][2].values[1], mf_values_[2][2].values[3]);
        move_up = vertical_direction_.MembershipFunctions.AddTrapezoid(mf_values_[2][3].function_name, mf_values_[2][3].values[0], mf_values_[2][3].values[1], mf_values_[2][3].values[2], mf_values_[2][3].values[3]);
        move_alot_up = vertical_direction_.MembershipFunctions.AddTrapezoid(mf_values_[2][4].function_name, mf_values_[2][4].values[0], mf_values_[2][4].values[1], mf_values_[2][4].values[2], mf_values_[2][4].values[3]);

        sideways_distance_ = new LinguisticVariable("sideways_distance");
        very_left_of_target = sideways_distance_.MembershipFunctions.AddTrapezoid(mf_values_[3][0].function_name, mf_values_[3][0].values[0], mf_values_[3][0].values[1], mf_values_[3][0].values[2], mf_values_[3][0].values[3]);
        left_of_target = sideways_distance_.MembershipFunctions.AddTrapezoid(mf_values_[3][1].function_name, mf_values_[3][1].values[0], mf_values_[3][1].values[1], mf_values_[3][1].values[2], mf_values_[3][1].values[3]);
        inline_with_target = sideways_distance_.MembershipFunctions.AddTriangle(mf_values_[3][2].function_name, mf_values_[3][2].values[0], mf_values_[3][2].values[1], mf_values_[3][2].values[3]);
        right_of_target = sideways_distance_.MembershipFunctions.AddTrapezoid(mf_values_[3][3].function_name, mf_values_[3][3].values[0], mf_values_[3][3].values[1], mf_values_[3][3].values[2], mf_values_[3][3].values[3]);
        very_right_of_target = sideways_distance_.MembershipFunctions.AddTrapezoid(mf_values_[3][4].function_name, mf_values_[3][4].values[0], mf_values_[3][4].values[1], mf_values_[3][4].values[2], mf_values_[3][4].values[3]);

        sideways_direction_ = new LinguisticVariable("sideways_direction");
        steer_alot_left = sideways_direction_.MembershipFunctions.AddTrapezoid(mf_values_[4][0].function_name, mf_values_[4][0].values[0], mf_values_[4][0].values[1], mf_values_[4][0].values[2], mf_values_[4][0].values[3]);
        steer_left = sideways_direction_.MembershipFunctions.AddTrapezoid(mf_values_[4][1].function_name, mf_values_[4][1].values[0], mf_values_[4][1].values[1], mf_values_[4][1].values[2], mf_values_[4][1].values[3]);
        stay_centred = sideways_direction_.MembershipFunctions.AddTriangle(mf_values_[4][2].function_name, mf_values_[4][2].values[0], mf_values_[4][2].values[1], mf_values_[4][2].values[3]);
        steer_right = sideways_direction_.MembershipFunctions.AddTrapezoid(mf_values_[4][3].function_name, mf_values_[4][3].values[0], mf_values_[4][3].values[1], mf_values_[4][3].values[2], mf_values_[4][3].values[3]);
        steer_alot_right = sideways_direction_.MembershipFunctions.AddTrapezoid(mf_values_[4][4].function_name, mf_values_[4][4].values[0], mf_values_[4][4].values[1], mf_values_[4][4].values[2], mf_values_[4][4].values[3]);

        vertical_engine_ = new FuzzyEngineFactory().Default();
        var vertical_rule_1 = Rule.If(vertical_distance_.Is(very_above_target)).Then(vertical_direction_.Is(move_alot_down));
        var vertical_rule_x = Rule.If(vertical_distance_.Is(above_target).And(forward_distance_.Is(alongside_target))).Then(vertical_direction_.Is(move_alot_down));
        var vertical_rule_2 = Rule.If(vertical_distance_.Is(above_target)).Then(vertical_direction_.Is(move_down));
        var vertical_rule_3 = Rule.If(vertical_distance_.Is(same_height)).Then(vertical_direction_.Is(stay_same_height));
        var vertical_rule_4 = Rule.If(vertical_distance_.Is(below_target)).Then(vertical_direction_.Is(move_up));
        var vertical_rule_5 = Rule.If(vertical_distance_.Is(below_target).And(forward_distance_.Is(alongside_target))).Then(vertical_direction_.Is(move_alot_up));
        var vertical_rule_y = Rule.If(vertical_distance_.Is(very_below_target)).Then(vertical_direction_.Is(move_alot_up));
        vertical_engine_.Rules.Add(vertical_rule_1, vertical_rule_2, vertical_rule_3, vertical_rule_4, vertical_rule_5, vertical_rule_x, vertical_rule_y);

        sideways_engine_ = new FuzzyEngineFactory().Default();
        var sideways_rule_1 = Rule.If(sideways_distance_.Is(very_right_of_target)).Then(sideways_direction_.Is(steer_alot_left));
        var side_test_rule = Rule.If(sideways_distance_.Is(right_of_target).And(forward_distance_.Is(alongside_target))).Then(sideways_direction_.Is(steer_alot_left));
        var sideways_rule_2 = Rule.If(sideways_distance_.Is(right_of_target)).Then(sideways_direction_.Is(steer_left));
        var sideways_rule_3 = Rule.If(sideways_distance_.Is(inline_with_target)).Then(sideways_direction_.Is(stay_centred));
        var sideways_rule_4 = Rule.If(sideways_distance_.Is(left_of_target)).Then(sideways_direction_.Is(steer_right));
        var side_test_rule2 = Rule.If(sideways_distance_.Is(left_of_target).And(forward_distance_.Is(alongside_target))).Then(sideways_direction_.Is(steer_alot_right));
        var sideways_rule_5 = Rule.If(sideways_distance_.Is(very_left_of_target)).Then(sideways_direction_.Is(steer_alot_right));
        sideways_engine_.Rules.Add(sideways_rule_1, sideways_rule_2, sideways_rule_3, sideways_rule_4, sideways_rule_5, side_test_rule, side_test_rule2);
    }

    private void FixedUpdate()
    {
        //Debug.Log(mf_values_[0].function.Name + " " + mf_values_[0].values[0]);
        switch(movement_)
        {
            case MovementType.Random:
                if(transform.position.x == target_position_.x && transform.position.y == target_position_.y)
                {
                    var new_missile = Instantiate(missile_, transform.position, Quaternion.identity);
                    new_missile.GetComponent<FuzzyMissile>().InitMissile(this, missile_forward_speed_, !transform.GetChild(0).gameObject.active);

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

    private void InitGraph()
    {
        membership_dropdown_ = membership_object_.GetComponent<TMP_Dropdown>();
        value1_ = value1_object_.GetComponent<TMP_InputField>();
        value2_ = value2_object_.GetComponent<TMP_InputField>();
        value3_ = value3_object_.GetComponent<TMP_InputField>();
        value4_ = value4_object_.GetComponent<TMP_InputField>();
        graph_ = graph_object_.GetComponent<SimplestPlot>();
        graph_.SetResolution(new Vector2(1200, 480));
        graph_.SeriesPlotY.Add(new SimplestPlot.SeriesClass());
        graph_.SeriesPlotY.Add(new SimplestPlot.SeriesClass());
        graph_.SeriesPlotY.Add(new SimplestPlot.SeriesClass());
        graph_.SeriesPlotY.Add(new SimplestPlot.SeriesClass());
        graph_.SeriesPlotY.Add(new SimplestPlot.SeriesClass());
        graph_.SeriesPlotY.Add(new SimplestPlot.SeriesClass());
        graph_.SeriesPlotY[0].MyColor = Color.red;
        graph_.SeriesPlotY[1].MyColor = Color.yellow;
        graph_.SeriesPlotY[2].MyColor = Color.green;
        graph_.SeriesPlotY[3].MyColor = Color.blue;
        graph_.SeriesPlotY[4].MyColor = Color.magenta;
        graph_.SeriesPlotY[5].MyColor = Color.black;

        graph_.SeriesPlotY[5].YValues = new float[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    }

    public void UpdateGraph(int linguistic_var)
    {
        currently_selected_linguistic_ = linguistic_var;
        for (int i = 0; i < 5; i++)
        {
            membership_dropdown_.options[i].text = mf_values_[linguistic_var][i].function_name;
        }
        membership_dropdown_.transform.GetChild(0).GetComponent<TMP_Text>().text = membership_dropdown_.options[0].text;
        DisplayValueInputs(0);

        float[] x_values = new float[20];
        float[] y_values = new float[20] { Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue };
        

        int index = 0;
        for(int line = 0; line < 5; line++)
        {
            for (int i = 0; i < 4; i++)
            {
                x_values[index] = mf_values_[linguistic_var][line].values[i];
                index++;
            }
        }
       // Array.Sort(x_values);
        for (int line = 0; line < 5; line++)
        {
            graph_.SeriesPlotY[line].YValues = new float[20];
            y_values.CopyTo(graph_.SeriesPlotY[line].YValues, 0);
            //for (int i = 0;  i < 4; i++)
            //{
            //    for(int x = 0; x < 20;  x++)
            //    {
            //        if (x_values[x] == mf_values_[linguistic_var][line].values[i])
            //        {
            //            graph_.SeriesPlotY[line].YValues[x] = (i == 0 || i == 3) ? 0 : 1;
            //            //if (x_values[x + 1] != x_values[x]) break;
            //        }
            //    }
            //}
            for (int i = 0; i <= x_values.Length - mf_values_[linguistic_var][line].values.Count; i++)
            {
                if (x_values.Skip(i).Take(mf_values_[linguistic_var][line].values.Count).SequenceEqual(mf_values_[linguistic_var][line].values))
                {
                    graph_.SeriesPlotY[line].YValues[i] = 0;
                    graph_.SeriesPlotY[line].YValues[i + 1] = 1;
                    graph_.SeriesPlotY[line].YValues[i + 2] = 1;
                    graph_.SeriesPlotY[line].YValues[i + 3] = 0;
                    break;
                }
            }
        }
        graph_.SeriesPlotX = x_values;
        graph_.UpdatePlot();
    }

    public void DisplayValueInputs(int membership_function)
    {
        currently_selected_membership_ = membership_function;
        if (membership_function == 2)
        {
            value4_object_.transform.parent.gameObject.SetActive(false);
            value1_.text = mf_values_[currently_selected_linguistic_][membership_function].values[0].ToString();
            value2_.text = mf_values_[currently_selected_linguistic_][membership_function].values[1].ToString();
            value3_.text = mf_values_[currently_selected_linguistic_][membership_function].values[3].ToString();
        }
        else
        {
            value4_object_.transform.parent.gameObject.SetActive(true);
            value1_.text = mf_values_[currently_selected_linguistic_][membership_function].values[0].ToString();
            value2_.text = mf_values_[currently_selected_linguistic_][membership_function].values[1].ToString();
            value3_.text = mf_values_[currently_selected_linguistic_][membership_function].values[2].ToString();
            value4_.text = mf_values_[currently_selected_linguistic_][membership_function].values[3].ToString();
        }
    }

    public void UpdateMembershipValues()
    {
        //if (currently_selected_membership_ == 2)
        //{
            float val1 = float.Parse(value1_.text);
            float val2 = float.Parse(value2_.text);
            float val3 = float.Parse(value3_.text);

            mf_values_[currently_selected_linguistic_][currently_selected_membership_].values[0] = val1;
            mf_values_[currently_selected_linguistic_][currently_selected_membership_].values[1] = val2;

        if (currently_selected_membership_ == 2)
        {
            mf_values_[currently_selected_linguistic_][currently_selected_membership_].values[2] = val2;
            mf_values_[currently_selected_linguistic_][currently_selected_membership_].values[3] = val3;
        }
        else
        {
            float val4 = float.Parse(value4_.text);
            mf_values_[currently_selected_linguistic_][currently_selected_membership_].values[2] = val3;
            mf_values_[currently_selected_linguistic_][currently_selected_membership_].values[3] = val4;
        }

        CreateFuzzyEngines();
        //}
       // else
       // {

           // string function_name = mf_values_[currently_selected_linguistic_][currently_selected_membership_].function_name;

            //if (currently_selected_linguistic_ == 0)
            //{
            //    forward_distance_.MembershipFunctions.Remove(mf_values_[currently_selected_linguistic_][currently_selected_membership_].function);
            //    mf_values_[currently_selected_linguistic_][currently_selected_membership_].function = forward_distance_.MembershipFunctions.AddTrapezoid(function_name, val1, val2, val3, val4);
            //}
            //else if (currently_selected_linguistic_ == 1)
            //{
            //    vertical_distance_.MembershipFunctions.Remove(mf_values_[currently_selected_linguistic_][currently_selected_membership_].function);
            //    mf_values_[currently_selected_linguistic_][currently_selected_membership_].function = vertical_distance_.MembershipFunctions.AddTrapezoid(function_name, val1, val2, val3, val4);
            //}
            //else if (currently_selected_linguistic_ == 2)
            //{
            //    vertical_direction_.MembershipFunctions.Remove(mf_values_[currently_selected_linguistic_][currently_selected_membership_].function);
            //    mf_values_[currently_selected_linguistic_][currently_selected_membership_].function = vertical_direction_.MembershipFunctions.AddTrapezoid(function_name, val1, val2, val3, val4);
            //}
            //else if (currently_selected_linguistic_ == 3)
            //{
            //    sideways_distance_.MembershipFunctions.Remove(mf_values_[currently_selected_linguistic_][currently_selected_membership_].function);
            //    mf_values_[currently_selected_linguistic_][currently_selected_membership_].function = sideways_distance_.MembershipFunctions.AddTrapezoid(function_name, val1, val2, val3, val4);
            //}
            //else if (currently_selected_linguistic_ == 4)
            //{
            //    sideways_direction_.MembershipFunctions.Remove(mf_values_[currently_selected_linguistic_][currently_selected_membership_].function);
            //    mf_values_[currently_selected_linguistic_][currently_selected_membership_].function = sideways_direction_.MembershipFunctions.AddTrapezoid(function_name, val1, val2, val3, val4);
            //}

            //mf_values_[currently_selected_linguistic_][currently_selected_membership_].values[0] = val1;
            //mf_values_[currently_selected_linguistic_][currently_selected_membership_].values[1] = val2;
            //mf_values_[currently_selected_linguistic_][currently_selected_membership_].values[2] = val3;
            //mf_values_[currently_selected_linguistic_][currently_selected_membership_].values[3] = val4;

       // }

        UpdateGraph(currently_selected_linguistic_);
    }
}
