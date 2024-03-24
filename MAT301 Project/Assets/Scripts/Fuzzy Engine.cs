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
using Unity.VisualScripting;

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
    RefMembershipFunction very_behind_target= new RefMembershipFunction(), behind_target= new RefMembershipFunction(), alongside_target= new RefMembershipFunction(), in_front_of_target= new RefMembershipFunction(), very_in_front_of_target= new RefMembershipFunction(),
                        very_below_target= new RefMembershipFunction(), below_target= new RefMembershipFunction(), same_height= new RefMembershipFunction(), above_target= new RefMembershipFunction(), very_above_target= new RefMembershipFunction(),
                        move_alot_down= new RefMembershipFunction(), move_down= new RefMembershipFunction(), stay_same_height= new RefMembershipFunction(), move_up= new RefMembershipFunction(), move_alot_up= new RefMembershipFunction(),
                        very_left_of_target= new RefMembershipFunction(), left_of_target= new RefMembershipFunction(), inline_with_target= new RefMembershipFunction(), right_of_target= new RefMembershipFunction(), very_right_of_target= new RefMembershipFunction(),
                        steer_alot_left= new RefMembershipFunction(), steer_left= new RefMembershipFunction(), stay_centred= new RefMembershipFunction(), steer_right= new RefMembershipFunction(), steer_alot_right = new RefMembershipFunction();

    List<List<RefMembershipFunction>> membership_functions_ = new List<List<RefMembershipFunction>>();
    //List<List<MembershipFunctionValues>> mf_values_ = new List<List<MembershipFunctionValues>>(); 

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
        //mf_values_.Add(new List<MembershipFunctionValues>());
        //mf_values_[0].Add(new MembershipFunctionValues("Far Behind", -65, -30, -20, -15));
        //mf_values_[0].Add(new MembershipFunctionValues("Behind", -20, -15, -10, -4));
        //mf_values_[0].Add(new MembershipFunctionValues("Alongside", -5, 0, 0, 5));
        //mf_values_[0].Add(new MembershipFunctionValues("In Front", 4, 10, 15, 20));
        //mf_values_[0].Add(new MembershipFunctionValues("Far In Front", 15, 20, 30, 65));

        //mf_values_.Add(new List<MembershipFunctionValues>());
        //mf_values_[1].Add(new MembershipFunctionValues("Far Below", -15, -8, -7, -5));
        //mf_values_[1].Add(new MembershipFunctionValues("Below", -7, -5, -3, -0.5f));
        //mf_values_[1].Add(new MembershipFunctionValues("None", -1, 0, 0, 1));
        //mf_values_[1].Add(new MembershipFunctionValues("Above", 0.5f, 3, 5, 7));
        //mf_values_[1].Add(new MembershipFunctionValues("Far Above", 5, 7, 8, 15));

        //mf_values_.Add(new List<MembershipFunctionValues>());
        //mf_values_[2].Add(new MembershipFunctionValues("Far Down", -20, -15, -10, -5));
        //mf_values_[2].Add(new MembershipFunctionValues("Down", -10, -5, -1, 0));
        //mf_values_[2].Add(new MembershipFunctionValues("None", -0.5f, 0, 0, 0.5f));
        //mf_values_[2].Add(new MembershipFunctionValues("Up", 0, 1, 5, 10));
        //mf_values_[2].Add(new MembershipFunctionValues("Far Up", 5, 10, 15, 20));

        //mf_values_.Add(new List<MembershipFunctionValues>());
        //mf_values_[3].Add(new MembershipFunctionValues("Far Left", -30, -30, -15, -10));
        //mf_values_[3].Add(new MembershipFunctionValues("Left", -15, -10, -5, -0.5f));
        //mf_values_[3].Add(new MembershipFunctionValues("None", -1, 0, 0, 1));
        //mf_values_[3].Add(new MembershipFunctionValues("Right", 0.5f, 5, 10, 15));
        //mf_values_[3].Add(new MembershipFunctionValues("Far Right", 10, 15, 30, 30));

        //mf_values_.Add(new List<MembershipFunctionValues>());
        //mf_values_[4].Add(new MembershipFunctionValues("Far Left", -50, -15, -10, -5));
        //mf_values_[4].Add(new MembershipFunctionValues("Left", -10, -5, -1, 0));
        //mf_values_[4].Add(new MembershipFunctionValues("None", -0.5f, 0, 0, 0.5f));
        //mf_values_[4].Add(new MembershipFunctionValues("Right", 0, 1, 5, 10));
        //mf_values_[4].Add(new MembershipFunctionValues("Far Right", 5, 10, 15, 50));

        forward_distance_ = new LinguisticVariable("forward_distance");
        very_behind_target.function = forward_distance_.MembershipFunctions.AddTrapezoid("Far Behind", -65, -30, -20, -15);
        behind_target.function = forward_distance_.MembershipFunctions.AddTrapezoid("Behind", -20, -15, -10, -4);
        //alongside_target.function = forward_distance_.MembershipFunctions.AddTriangle("Alongside", -5, 0, 5);
        alongside_target.function = forward_distance_.MembershipFunctions.AddTrapezoid("Alongside", -5, 0, 0, 5);
        in_front_of_target.function = forward_distance_.MembershipFunctions.AddTrapezoid("In Front", 4, 10, 15, 20);
        very_in_front_of_target.function = forward_distance_.MembershipFunctions.AddTrapezoid("Far In Front", 15, 20, 30, 65);
        membership_functions_.Add(new List<RefMembershipFunction>());
        membership_functions_[0].Add(very_behind_target);
        membership_functions_[0].Add(behind_target);
        membership_functions_[0].Add(alongside_target);
        membership_functions_[0].Add(in_front_of_target);
        membership_functions_[0].Add(very_in_front_of_target);

        vertical_distance_ = new LinguisticVariable("vertical_distance");
        very_below_target.function = vertical_distance_.MembershipFunctions.AddTrapezoid("Far Below", -15, -8, -7, -5);
        below_target.function = vertical_distance_.MembershipFunctions.AddTrapezoid("Below", -7, -5, -3, -0.5f);
        //same_height.function = vertical_distance_.MembershipFunctions.AddTriangle("None", -1, 0, 1);
        same_height.function = vertical_distance_.MembershipFunctions.AddTrapezoid("None", -1, 0, 0, 1);
        above_target.function = vertical_distance_.MembershipFunctions.AddTrapezoid("Above", 0.5f, 3, 5, 7);
        very_above_target.function = vertical_distance_.MembershipFunctions.AddTrapezoid("Far Above", 5, 7, 8, 15);
        membership_functions_.Add(new List<RefMembershipFunction>());
        membership_functions_[1].Add(very_below_target);
        membership_functions_[1].Add(below_target);
        membership_functions_[1].Add(same_height);
        membership_functions_[1].Add(above_target);
        membership_functions_[1].Add(very_above_target);

        vertical_direction_ = new LinguisticVariable("vertical_direction");
        move_alot_down.function = vertical_direction_.MembershipFunctions.AddTrapezoid("Far Down", -20, -15, -10, -5);
        move_down.function = vertical_direction_.MembershipFunctions.AddTrapezoid("Down", -10, -5, -1, 0);
        //stay_same_height.function = vertical_direction_.MembershipFunctions.AddTriangle("None", -0.5f, 0, 0.5f);
        stay_same_height.function = vertical_direction_.MembershipFunctions.AddTrapezoid("None", -0.5f, 0, 0, 0.5f);
        move_up.function = vertical_direction_.MembershipFunctions.AddTrapezoid("Up", 0, 1, 5, 10);
        move_alot_up.function = vertical_direction_.MembershipFunctions.AddTrapezoid("Far Up", 5, 10, 15, 20);
        membership_functions_.Add(new List<RefMembershipFunction>());
        membership_functions_[2].Add(move_alot_down);
        membership_functions_[2].Add(move_down);
        membership_functions_[2].Add(stay_same_height);
        membership_functions_[2].Add(move_up);
        membership_functions_[2].Add(move_alot_up);

        sideways_distance_ = new LinguisticVariable("sideways_distance");
        very_left_of_target.function = sideways_distance_.MembershipFunctions.AddTrapezoid("Far Left", -30, -30, -15, -10);
        left_of_target.function = sideways_distance_.MembershipFunctions.AddTrapezoid("Left", -15, -10, -5, -0.5f);
        //inline_with_target.function = sideways_distance_.MembershipFunctions.AddTriangle("None", -1, 0, 1);
        inline_with_target.function = sideways_distance_.MembershipFunctions.AddTrapezoid("None", -1, 0, 0, 1);
        right_of_target.function = sideways_distance_.MembershipFunctions.AddTrapezoid("Right", 0.5f, 5, 10, 15);
        very_right_of_target.function = sideways_distance_.MembershipFunctions.AddTrapezoid("Far Right", 10, 15, 30, 30);
        membership_functions_.Add(new List<RefMembershipFunction>());
        membership_functions_[3].Add(very_left_of_target);
        membership_functions_[3].Add(left_of_target);
        membership_functions_[3].Add(inline_with_target);
        membership_functions_[3].Add(right_of_target);
        membership_functions_[3].Add(very_right_of_target);

        sideways_direction_ = new LinguisticVariable("sideways_direction");
        steer_alot_left.function = sideways_direction_.MembershipFunctions.AddTrapezoid("Far Left", -50, -15, -10, -5);
        steer_left.function = sideways_direction_.MembershipFunctions.AddTrapezoid("Left", -10, -5, -1, 0);
        //stay_centred.function = sideways_direction_.MembershipFunctions.AddTriangle("None", -0.5f, 0, 0.5f);
        stay_centred.function = sideways_direction_.MembershipFunctions.AddTrapezoid("None", -0.5f, 0, 0, 0.5f);
        steer_right.function = sideways_direction_.MembershipFunctions.AddTrapezoid("Right", 0, 1, 5, 10);
        steer_alot_right.function = sideways_direction_.MembershipFunctions.AddTrapezoid("Far Right", 5, 10, 15, 50);
        membership_functions_.Add(new List<RefMembershipFunction>());
        membership_functions_[4].Add(steer_alot_left);
        membership_functions_[4].Add(steer_left);
        membership_functions_[4].Add(stay_centred);
        membership_functions_[4].Add(steer_right);
        membership_functions_[4].Add(steer_alot_right);
    }

    void CreateFuzzyEngines()
    {
        vertical_engine_ = new FuzzyEngineFactory().Default();
        var vertical_rule_1 = Rule.If(vertical_distance_.Is(very_above_target.function)).Then(vertical_direction_.Is(move_alot_down.function));
        var vertical_rule_x = Rule.If(vertical_distance_.Is(above_target.function).And(forward_distance_.Is(alongside_target.function))).Then(vertical_direction_.Is(move_alot_down.function));
        var vertical_rule_2 = Rule.If(vertical_distance_.Is(above_target.function)).Then(vertical_direction_.Is(move_down.function));
        var vertical_rule_3 = Rule.If(vertical_distance_.Is(same_height.function)).Then(vertical_direction_.Is(stay_same_height.function));
        var vertical_rule_4 = Rule.If(vertical_distance_.Is(below_target.function)).Then(vertical_direction_.Is(move_up.function));
        var vertical_rule_5 = Rule.If(vertical_distance_.Is(below_target.function).And(forward_distance_.Is(alongside_target.function))).Then(vertical_direction_.Is(move_alot_up.function));
        var vertical_rule_y = Rule.If(vertical_distance_.Is(very_below_target.function)).Then(vertical_direction_.Is(move_alot_up.function));
        vertical_engine_.Rules.Add(vertical_rule_1, vertical_rule_2, vertical_rule_3, vertical_rule_4, vertical_rule_5, vertical_rule_x, vertical_rule_y);

        sideways_engine_ = new FuzzyEngineFactory().Default();
        var sideways_rule_1 = Rule.If(sideways_distance_.Is(very_right_of_target.function)).Then(sideways_direction_.Is(steer_alot_left.function));
        var side_test_rule = Rule.If(sideways_distance_.Is(right_of_target.function).And(forward_distance_.Is(alongside_target.function))).Then(sideways_direction_.Is(steer_alot_left.function));
        var sideways_rule_2 = Rule.If(sideways_distance_.Is(right_of_target.function)).Then(sideways_direction_.Is(steer_left.function));
        var sideways_rule_3 = Rule.If(sideways_distance_.Is(inline_with_target.function)).Then(sideways_direction_.Is(stay_centred.function));
        var sideways_rule_4 = Rule.If(sideways_distance_.Is(left_of_target.function)).Then(sideways_direction_.Is(steer_right.function));
        var side_test_rule2 = Rule.If(sideways_distance_.Is(left_of_target.function).And(forward_distance_.Is(alongside_target.function))).Then(sideways_direction_.Is(steer_alot_right.function));
        var sideways_rule_5 = Rule.If(sideways_distance_.Is(very_left_of_target.function)).Then(sideways_direction_.Is(steer_alot_right.function));
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
            membership_dropdown_.options[i].text = membership_functions_[linguistic_var][i].function.Name;
        }
        membership_dropdown_.transform.GetChild(0).GetComponent<TMP_Text>().text = membership_dropdown_.options[0].text;
        DisplayValueInputs(0);

        float[] x_values = new float[20];
        float[] y_values = new float[20] { Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue };
        

        int index = 0;
        for(int mf = 0; mf < 5; mf++)
        {
            //for (int i = 0; i < 4; i++)
            //{
            TrapezoidMembershipFunction function = (TrapezoidMembershipFunction)membership_functions_[linguistic_var][mf].function;
            x_values[index] = (float)function.A;
            x_values[index+1] = (float)function.B;
            x_values[index+2] = (float)function.C;
            x_values[index+3] = (float)function.D;
            index += 4;

            //}
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
            //float[] values = new float[4] {}
            //for (int i = 0; i <= x_values.Length - 4; i++)
            //{
            //    if (x_values.Skip(i).Take(4).SequenceEqual(mf_values_[linguistic_var][line].values))
            //    {
                    graph_.SeriesPlotY[line].YValues[line * 4] = 0;
                    graph_.SeriesPlotY[line].YValues[line * 4 + 1] = 1;
                    graph_.SeriesPlotY[line].YValues[line * 4 + 2] = 1;
                    graph_.SeriesPlotY[line].YValues[line * 4 + 3] = 0;
                    //break;
            //    }
            //}
        }
        graph_.SeriesPlotX = x_values;
        graph_.UpdatePlot();
    }

    public void DisplayValueInputs(int membership_function)
    {
        currently_selected_membership_ = membership_function;
        //if (membership_function == 2)
        //{
        //    value4_object_.transform.parent.gameObject.SetActive(false);
        //    TriangleMembershipFunction function = (TriangleMembershipFunction)membership_functions_[currently_selected_linguistic_][membership_function].function;
        //    value1_.text = function.A.ToString();
        //    value2_.text = function.B.ToString();
        //    value3_.text = function.C.ToString();
        //}
        //else
        //{
            //value4_object_.transform.parent.gameObject.SetActive(true);
            TrapezoidMembershipFunction function = (TrapezoidMembershipFunction)membership_functions_[currently_selected_linguistic_][membership_function].function;
            value1_.text = function.A.ToString();
            value2_.text = function.B.ToString();
            value3_.text = function.C.ToString();
            value4_.text = function.D.ToString();
        //}
    }

    public void UpdateMembershipValues()
    {
        //if (currently_selected_membership_ == 2)
        //{
        float val1 = float.Parse(value1_.text);
        float val2 = float.Parse(value2_.text);
        float val3 = float.Parse(value3_.text);

        ref LinguisticVariable lingustic = ref forward_distance_;

        switch (currently_selected_linguistic_)
        {
            case 1:
                lingustic = ref vertical_distance_;
                break;
            case 2:
                lingustic = ref vertical_direction_;
                break;
            case 3:
                lingustic = ref sideways_distance_; 
                break;
            case 4:
                lingustic = ref sideways_direction_;
                break;
            default: break;
        }

        //if (currently_selected_membership_ == 2)
        //{
        //    // TriangleMembershipFunction new_function;
        //    string func_name = membership_functions_[currently_selected_linguistic_][currently_selected_membership_].function.Name;
        //    membership_functions_[currently_selected_linguistic_][currently_selected_membership_].function = new TriangleMembershipFunction(func_name, val1, val2, val3);
        //    lingustic.MembershipFunctions[2] = membership_functions_[currently_selected_linguistic_][currently_selected_membership_].function;
        //}
        //else
        //{
            float val4 = float.Parse(value4_.text);
            //mf_values_[currently_selected_linguistic_][currently_selected_membership_].values[2] = val3;
            //mf_values_[currently_selected_linguistic_][currently_selected_membership_].values[3] = val4;

            string func_name = membership_functions_[currently_selected_linguistic_][currently_selected_membership_].function.Name;
            membership_functions_[currently_selected_linguistic_][currently_selected_membership_].function = new TrapezoidMembershipFunction(func_name, val1, val2, val3, val4);
            lingustic.MembershipFunctions[currently_selected_membership_] = membership_functions_[currently_selected_linguistic_][currently_selected_membership_].function;
        //}

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
