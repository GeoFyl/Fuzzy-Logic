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

public class FuzzyBrain : MonoBehaviour
{
    enum MovementType { Random, Stationary };
    [SerializeField]
    MovementType movement_ = MovementType.Random;
    [SerializeField]
    float missiles_per_second = 2;
    float missile_fire_timer_ = 0;
    [SerializeField]
    float move_speed_ = 10;
    [SerializeField]
    float x_position_limit_ = 8;
    [SerializeField]
    float y_position_limit_ = 8;

    [SerializeField]
    GameObject hit_counter_;
    [SerializeField]
    GameObject miss_counter_;
    [SerializeField]
    GameObject downed_counter_;
    public int hits_, misses_, downed_;

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
    public TMP_InputField value1_, value2_, value3_, value4_;
    TMP_Dropdown membership_dropdown_;
    SimplestPlot graph_;
    int currently_selected_linguistic_;
    int currently_selected_membership_;

    Vector3 target_position_;

    [SerializeField]
    GameObject missile_;
    [SerializeField]
    float missile_forward_speed_ = 15;

    IFuzzyEngine sideways_engine_, vertical_engine_;
    LinguisticVariable sideways_distance_, sideways_direction_, forward_distance_, vertical_distance_, vertical_direction_, player_missile_sideways_distance_, player_missile_vertical_distance_, player_missile_forward_distance_;
    RefMembershipFunction very_behind_target= new RefMembershipFunction(), behind_target= new RefMembershipFunction(), alongside_target= new RefMembershipFunction(), in_front_of_target= new RefMembershipFunction(), very_in_front_of_target= new RefMembershipFunction(),
                        very_below_target= new RefMembershipFunction(), below_target= new RefMembershipFunction(), same_height= new RefMembershipFunction(), above_target= new RefMembershipFunction(), very_above_target= new RefMembershipFunction(),
                        move_alot_down= new RefMembershipFunction(), move_down= new RefMembershipFunction(), stay_same_height= new RefMembershipFunction(), move_up= new RefMembershipFunction(), move_alot_up= new RefMembershipFunction(),
                        very_left_of_target= new RefMembershipFunction(), left_of_target= new RefMembershipFunction(), inline_with_target= new RefMembershipFunction(), right_of_target= new RefMembershipFunction(), very_right_of_target= new RefMembershipFunction(),
                        steer_alot_left= new RefMembershipFunction(), steer_left= new RefMembershipFunction(), stay_centred= new RefMembershipFunction(), steer_right= new RefMembershipFunction(), steer_alot_right = new RefMembershipFunction();
    public List<List<RefMembershipFunction>> membership_functions_ = new List<List<RefMembershipFunction>>();

    [SerializeField]
    GameObject test_manager_;
    bool testing_ = false;
    int missiles_fired_, missiles_completed_;

    // Start is called before the first frame update
    void Start()
    {
        target_position_.x = Random.Range(-x_position_limit_, x_position_limit_);
        target_position_.y = Random.Range(0, y_position_limit_);
        target_position_.z = transform.position.z;

        // ---------- Setup Membership Function values -----------
        InitMembershipFuncs();
        CreateFuzzyEngines();

        InitGraph();
        DisplayValueInputs(0);
        UpdateGraph(0);
    }

    //Called by missile to defuzzify sideways
    public float DefuzzifySideways(float sideways, float forward, float pm_forward, float pm_side)
    {
        return (float)sideways_engine_.Defuzzify(new { sideways_distance = (double)sideways, forward_distance = (double)forward, pm_forward_distance = (double)pm_forward, pm_side_distance = (double)pm_side});
    }

    //Called by missile to defuzzify vertical
    public float DefuzzifyVertical(float vertical, float forward, float pm_forward, float pm_vertical)
    {
        return (float)vertical_engine_.Defuzzify(new { vertical_distance = (double)vertical, forward_distance = (double)forward, pm_forward_distance = (double)pm_forward, pm_vertical_distance = (double)pm_vertical });
    }

    public void Hit()
    {
        hits_++;
        hit_counter_.GetComponent<TMP_Text>().text = hits_.ToString();

        if(testing_)
        {
            missiles_completed_++;
            if(missiles_completed_ == 100)
            {
                testing_ = false;
                test_manager_.GetComponent<TestManager>().CompletedTest();
            }
        }
    }

    public void Miss()
    {
        misses_++;
        miss_counter_.GetComponent<TMP_Text>().text = misses_.ToString();

        if (testing_)
        {
            missiles_completed_++;
            if (missiles_completed_ == 100)
            {
                testing_ = false;
                test_manager_.GetComponent<TestManager>().CompletedTest();
            }
        }
    }

    public void ShotDown()
    {
        downed_++;
        downed_counter_.GetComponent<TMP_Text>().text = downed_.ToString();

        if (testing_)
        {
            missiles_completed_++;
            if (missiles_completed_ == 100)
            {
                testing_ = false;
                test_manager_.GetComponent<TestManager>().CompletedTest();
            }
        }
    }

    // Initialise fuzzy sets
    private void InitMembershipFuncs()
    {
        forward_distance_ = new LinguisticVariable("forward_distance");
        very_behind_target.function = forward_distance_.MembershipFunctions.AddTrapezoid("Far Behind", -60, -58.30882, -53.66272, -17.97618);
        behind_target.function = forward_distance_.MembershipFunctions.AddTrapezoid("Behind", -22.44128, -17.37739, -17.08515, 0.04424119);
        alongside_target.function = forward_distance_.MembershipFunctions.AddTrapezoid("Alongside", -8.626575, 7.284637, 8.053778, 8.626575);
        in_front_of_target.function = forward_distance_.MembershipFunctions.AddTrapezoid("In Front", 8.355507, 22.9482, 23.63193, 27.62806);
        very_in_front_of_target.function = forward_distance_.MembershipFunctions.AddTrapezoid("Far In Front", 25.56501, 58.26857, 59.63116, 60);
        membership_functions_.Add(new List<RefMembershipFunction>());
        membership_functions_[0].Add(very_behind_target);
        membership_functions_[0].Add(behind_target);
        membership_functions_[0].Add(alongside_target);
        membership_functions_[0].Add(in_front_of_target);
        membership_functions_[0].Add(very_in_front_of_target);

        vertical_distance_ = new LinguisticVariable("vertical_distance");
        very_below_target.function = vertical_distance_.MembershipFunctions.AddTrapezoid("Far Below", -25, -15, -10, -5);
        below_target.function = vertical_distance_.MembershipFunctions.AddTrapezoid("Below", -7, -5, -3, -0.1f);
        same_height.function = vertical_distance_.MembershipFunctions.AddTrapezoid("Same Height", -1, 0, 0, 1);
        above_target.function = vertical_distance_.MembershipFunctions.AddTrapezoid("Above", 0.1f, 3, 5, 7);
        very_above_target.function = vertical_distance_.MembershipFunctions.AddTrapezoid("Far Above", 5, 10, 15, 25);
        membership_functions_.Add(new List<RefMembershipFunction>());
        membership_functions_[1].Add(very_below_target);
        membership_functions_[1].Add(below_target);
        membership_functions_[1].Add(same_height);
        membership_functions_[1].Add(above_target);
        membership_functions_[1].Add(very_above_target);

        vertical_direction_ = new LinguisticVariable("vertical_direction");
        move_alot_down.function = vertical_direction_.MembershipFunctions.AddTrapezoid("Far Down", -20, -15, -10, -5);
        move_down.function = vertical_direction_.MembershipFunctions.AddTrapezoid("Down", -10, -5, -1, 0.1f);
        stay_same_height.function = vertical_direction_.MembershipFunctions.AddTrapezoid("None", -1f, 0, 0, 1f);
        move_up.function = vertical_direction_.MembershipFunctions.AddTrapezoid("Up", 0.1f, 1, 5, 10);
        move_alot_up.function = vertical_direction_.MembershipFunctions.AddTrapezoid("Far Up", 5, 10, 15, 20);
        membership_functions_.Add(new List<RefMembershipFunction>());
        membership_functions_[2].Add(move_alot_down);
        membership_functions_[2].Add(move_down);
        membership_functions_[2].Add(stay_same_height);
        membership_functions_[2].Add(move_up);
        membership_functions_[2].Add(move_alot_up);

        sideways_distance_ = new LinguisticVariable("sideways_distance");
        very_left_of_target.function = sideways_distance_.MembershipFunctions.AddTrapezoid("Far Left", -42, -30, -15, -10);
        left_of_target.function = sideways_distance_.MembershipFunctions.AddTrapezoid("Left", -15, -10, -5, -0.1f);
        inline_with_target.function = sideways_distance_.MembershipFunctions.AddTrapezoid("Inline", -1, 0, 0, 1);
        right_of_target.function = sideways_distance_.MembershipFunctions.AddTrapezoid("Right", 0.1f, 5, 10, 15);
        very_right_of_target.function = sideways_distance_.MembershipFunctions.AddTrapezoid("Far Right", 10, 15, 30, 42);
        membership_functions_.Add(new List<RefMembershipFunction>());
        membership_functions_[3].Add(very_left_of_target);
        membership_functions_[3].Add(left_of_target);
        membership_functions_[3].Add(inline_with_target);
        membership_functions_[3].Add(right_of_target);
        membership_functions_[3].Add(very_right_of_target);

        sideways_direction_ = new LinguisticVariable("sideways_direction");
        steer_alot_left.function = sideways_direction_.MembershipFunctions.AddTrapezoid("Far Left", -50, -49.26415, -23.02056, -10.16195);
        steer_left.function = sideways_direction_.MembershipFunctions.AddTrapezoid("Left", -10.6641, -8.564794, -4.956827, -0.243949);
        stay_centred.function = sideways_direction_.MembershipFunctions.AddTrapezoid("None", -0.8008431, 0.1482656, 0.5595238, 0.8008431);
        steer_right.function = sideways_direction_.MembershipFunctions.AddTrapezoid("Right", 0.7683588, 4.723279, 7.013867, 8.112687);
        steer_alot_right.function = sideways_direction_.MembershipFunctions.AddTrapezoid("Far Right", 7.229371, 39.46464, 41.2893, 50);
        membership_functions_.Add(new List<RefMembershipFunction>());
        membership_functions_[4].Add(steer_alot_left);
        membership_functions_[4].Add(steer_left);
        membership_functions_[4].Add(stay_centred);
        membership_functions_[4].Add(steer_right);
        membership_functions_[4].Add(steer_alot_right);

        player_missile_sideways_distance_ = new LinguisticVariable("pm_side_distance");
        player_missile_vertical_distance_ = new LinguisticVariable("pm_vertical_distance");
        player_missile_forward_distance_ = new LinguisticVariable("pm_forward_distance");
    }

    void CreateFuzzyEngines()
    {
        // Setup rules and create engine for sideways movement
        sideways_engine_ = new FuzzyEngineFactory().Default();

        var pm_sideways_rule_1 = Rule.If(player_missile_sideways_distance_.Is(right_of_target.function).Or(player_missile_sideways_distance_.Is(inline_with_target.function)).And(player_missile_forward_distance_.Is(alongside_target.function))).Then(sideways_direction_.Is(steer_right.function));
        var pm_sideways_rule_2 = Rule.If(player_missile_forward_distance_.Is(alongside_target.function).And(player_missile_sideways_distance_.Is(left_of_target.function))).Then(sideways_direction_.Is(steer_left.function));

        var sideways_rule_0 = Rule.If(sideways_distance_.Is(very_right_of_target.function)).Then(sideways_direction_.Is(steer_alot_left.function));
        var sideways_rule_1 = Rule.If(sideways_distance_.Is(very_right_of_target.function).And(forward_distance_.Is(very_in_front_of_target.function))).Then(sideways_direction_.Is(steer_left.function));
        var sideways_rule_2 = Rule.If(sideways_distance_.Is(right_of_target.function).And(forward_distance_.Is(alongside_target.function))).Then(sideways_direction_.Is(steer_alot_left.function));
        var sideways_rule_3 = Rule.If(sideways_distance_.Is(right_of_target.function)).Then(sideways_direction_.Is(steer_left.function));
        var sideways_rule_4 = Rule.If(sideways_distance_.Is(inline_with_target.function)).Then(sideways_direction_.Is(stay_centred.function));
        var sideways_rule_5 = Rule.If(sideways_distance_.Is(left_of_target.function)).Then(sideways_direction_.Is(steer_right.function));
        var sideways_rule_6 = Rule.If(sideways_distance_.Is(left_of_target.function).And(forward_distance_.Is(alongside_target.function))).Then(sideways_direction_.Is(steer_alot_right.function));
        var sideways_rule_7 = Rule.If(sideways_distance_.Is(very_left_of_target.function)).Then(sideways_direction_.Is(steer_alot_right.function));
        var sideways_rule_8 = Rule.If(sideways_distance_.Is(very_left_of_target.function).And(forward_distance_.Is(very_in_front_of_target.function))).Then(sideways_direction_.Is(steer_right.function));
        sideways_engine_.Rules.Add(sideways_rule_0, sideways_rule_1, sideways_rule_2, sideways_rule_3, sideways_rule_4, sideways_rule_5, sideways_rule_6, sideways_rule_7, sideways_rule_8, pm_sideways_rule_1, pm_sideways_rule_2);

        // Setup rules and create engine for vertical movement
        vertical_engine_ = new FuzzyEngineFactory().Default();

        var pm_vertical_rule_1 = Rule.If(player_missile_vertical_distance_.Is(above_target.function).Or(player_missile_vertical_distance_.Is(same_height.function)).And(player_missile_forward_distance_.Is(alongside_target.function))).Then(vertical_direction_.Is(move_up.function));
        var pm_vertical_rule_2 = Rule.If(player_missile_forward_distance_.Is(alongside_target.function).And(player_missile_vertical_distance_.Is(below_target.function))).Then(vertical_direction_.Is(move_down.function));

        var vertical_rule_0 = Rule.If(vertical_distance_.Is(very_above_target.function)).Then(vertical_direction_.Is(move_alot_down.function));
        var vertical_rule_1 = Rule.If(vertical_distance_.Is(very_above_target.function).And(forward_distance_.Is(very_in_front_of_target.function))).Then(vertical_direction_.Is(move_down.function));
        var vertical_rule_2 = Rule.If(vertical_distance_.Is(above_target.function).And(forward_distance_.Is(alongside_target.function))).Then(vertical_direction_.Is(move_alot_down.function));
        var vertical_rule_3 = Rule.If(vertical_distance_.Is(above_target.function)).Then(vertical_direction_.Is(move_down.function));
        var vertical_rule_4 = Rule.If(vertical_distance_.Is(same_height.function)).Then(vertical_direction_.Is(stay_same_height.function));
        var vertical_rule_5 = Rule.If(vertical_distance_.Is(below_target.function)).Then(vertical_direction_.Is(move_up.function));
        var vertical_rule_6 = Rule.If(vertical_distance_.Is(below_target.function).And(forward_distance_.Is(alongside_target.function))).Then(vertical_direction_.Is(move_alot_up.function));
        var vertical_rule_7 = Rule.If(vertical_distance_.Is(very_below_target.function)).Then(vertical_direction_.Is(move_alot_up.function));
        var vertical_rule_8 = Rule.If(vertical_distance_.Is(very_below_target.function).And(forward_distance_.Is(very_in_front_of_target.function))).Then(vertical_direction_.Is(move_up.function));
        vertical_engine_.Rules.Add(vertical_rule_0, vertical_rule_1, vertical_rule_2, vertical_rule_3, vertical_rule_4, vertical_rule_5, vertical_rule_6, vertical_rule_7, vertical_rule_8, pm_vertical_rule_1, pm_vertical_rule_2);
    }

    private void FixedUpdate()
    {
        // Control position of missile spawner 
        missile_fire_timer_ += Time.deltaTime;
        switch(movement_)
        {
            case MovementType.Random:
                if(!testing_ || missiles_fired_ < 100) { 
                    if (missile_fire_timer_ >= 1f / missiles_per_second)
                    {
                        missile_fire_timer_ = 0;
                        var new_missile = Instantiate(missile_, transform.position, Quaternion.identity);
                        new_missile.GetComponent<FuzzyMissile>().InitMissile(this, missile_forward_speed_, !transform.GetChild(0).gameObject.active);
                    }
                }
                if (transform.position.x == target_position_.x && transform.position.y == target_position_.y)
                {
                    target_position_.x = Random.Range(-x_position_limit_, x_position_limit_);
                    target_position_.y = Random.Range(0, y_position_limit_);
                }
                transform.position = Vector3.MoveTowards(transform.position, target_position_, move_speed_ * Time.deltaTime);

                // Smoothly tilt helicopter mesh according to direction
                float x_delta = target_position_.x - transform.position.x;
                float tiltAroundZ = (x_delta > 0 ? 1 : -1) * 15;
                Quaternion target = Quaternion.Euler(-79, 180, tiltAroundZ);
                transform.GetChild(0).localRotation = Quaternion.Slerp(transform.GetChild(0).localRotation, target, Time.deltaTime * 3);
                
                break;
            case MovementType.Stationary:
                if (!testing_ || missiles_fired_ < 100)
                {
                    if (missile_fire_timer_ >= 1f / missiles_per_second)
                    {
                        missile_fire_timer_ = 0;
                        var new_missile = Instantiate(missile_, transform.position, Quaternion.identity);
                        new_missile.GetComponent<FuzzyMissile>().InitMissile(this, missile_forward_speed_, !transform.GetChild(0).gameObject.active);
                    }
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

    // Updates the graph to display fuzzy sets
    public void UpdateGraph(int linguistic_var)
    {
        currently_selected_linguistic_ = linguistic_var;
        for (int i = 0; i < 5; i++)
        {
            membership_dropdown_.options[i].text = membership_functions_[linguistic_var][i].function.Name;
        }
        membership_dropdown_.transform.GetChild(0).GetComponent<TMP_Text>().text = membership_dropdown_.options[currently_selected_membership_].text;
        DisplayValueInputs(currently_selected_membership_);

        float[] x_values = new float[20];
        float[] y_values = new float[20] { Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue };
        

        int index = 0;
        for(int mf = 0; mf < 5; mf++)
        {
            TrapezoidMembershipFunction function = (TrapezoidMembershipFunction)membership_functions_[linguistic_var][mf].function;
            x_values[index] = (float)function.A;
            x_values[index+1] = (float)function.B;
            x_values[index+2] = (float)function.C;
            x_values[index+3] = (float)function.D;
            index += 4;
        }
        for (int line = 0; line < 5; line++)
        {
            graph_.SeriesPlotY[line].YValues = new float[20];
            y_values.CopyTo(graph_.SeriesPlotY[line].YValues, 0);

            graph_.SeriesPlotY[line].YValues[line * 4] = 0;
            graph_.SeriesPlotY[line].YValues[line * 4 + 1] = 1;
            graph_.SeriesPlotY[line].YValues[line * 4 + 2] = 1;
            graph_.SeriesPlotY[line].YValues[line * 4 + 3] = 0;
        }
        graph_.SeriesPlotX = x_values;
        graph_.UpdatePlot();
    }

    // Displays values of selected membership function
    public void DisplayValueInputs(int membership_function)
    {
        currently_selected_membership_ = membership_function;

        TrapezoidMembershipFunction function = (TrapezoidMembershipFunction)membership_functions_[currently_selected_linguistic_][membership_function].function;
        value1_.text = function.A.ToString();
        value2_.text = function.B.ToString();
        value3_.text = function.C.ToString();
        value4_.text = function.D.ToString();
    }

    //Called by menu to update to newly input values
    public void UpdateMembershipValues()
    {
        float val1 = float.Parse(value1_.text);
        float val2 = float.Parse(value2_.text);
        float val3 = float.Parse(value3_.text);
        float val4 = float.Parse(value4_.text);

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

        string func_name = membership_functions_[currently_selected_linguistic_][currently_selected_membership_].function.Name;
        membership_functions_[currently_selected_linguistic_][currently_selected_membership_].function = new TrapezoidMembershipFunction(func_name, val1, val2, val3, val4);
        lingustic.MembershipFunctions[currently_selected_membership_] = membership_functions_[currently_selected_linguistic_][currently_selected_membership_].function;

        CreateFuzzyEngines();
        UpdateGraph(currently_selected_linguistic_);
    }

    // Called to update a linguistic variables fuzzy set of membership functions all at once
    public void BatchUpdateMembershipValues(int linguistic_var, List<float[]> values)
    {
        currently_selected_linguistic_ = linguistic_var;
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

        for(int i = 0; i < 5; i++)
        {
            string func_name = membership_functions_[currently_selected_linguistic_][i].function.Name;
            membership_functions_[currently_selected_linguistic_][i].function = new TrapezoidMembershipFunction(func_name, values[i][0], values[i][1], values[i][2], values[i][3]);
            lingustic.MembershipFunctions[i] = membership_functions_[currently_selected_linguistic_][i].function;
        }

        CreateFuzzyEngines();
        UpdateGraph(currently_selected_linguistic_);
    }

    // Resets counters and switch to testing
    public void RunTest()
    {
        testing_ = true;
        missiles_fired_ = missiles_completed_ = 0;
        hits_ = misses_ = downed_ = 0;
        hit_counter_.GetComponent<TMP_Text>().text = "0";
        miss_counter_.GetComponent<TMP_Text>().text = "0";
        downed_counter_.GetComponent<TMP_Text>().text = "0";
    }
}
