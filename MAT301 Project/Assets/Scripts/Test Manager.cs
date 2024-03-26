using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using FLS;
using FLS.Rules;
using FLS.MembershipFunctions;
using JetBrains.Annotations;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

public class TestManager : MonoBehaviour
{
    StreamWriter writer;

    [SerializeField]
    GameObject player_;

    [SerializeField]
    GameObject brain_object_;
    FuzzyBrain brain_;

    [SerializeField]
    GameObject pause_menu_;

    // outer list is linguistic, middle list is list of variations, inner is list of member function values
    List<List<List<float[]>>> membership_function_variations = new List<List<List<float[]>>>();

    int linguistic_index_ = 0, variation_index_ = 0;

    // { value, variation index }
    int[] highest_hits_ = new int[2] { int.MinValue, 0 }, lowest_misses_ = new int[2] { int.MaxValue, 0 }, lowest_downed_ = new int[2] { int.MaxValue, 0 }, lowest_misses_and_downed_ = new int[2] { int.MaxValue, 0 };

    bool running_single_test_ = true;

    // Start is called before the first frame update
    void Start()
    {
        brain_ = brain_object_.GetComponent<FuzzyBrain>();
    }

    public void BeginBatchTesting()
    {
        player_.GetComponent<Jet>().BeginTesting();

        running_single_test_ = false;

        int index = 0;
        foreach (var func_list in brain_.membership_functions_)
        {
            membership_function_variations.Add(new List<List<float[]>>());
            membership_function_variations[index].Add(new List<float[]>());
            foreach (var function in func_list)
            {
                TrapezoidMembershipFunction mf_func = (TrapezoidMembershipFunction)function.function;
                membership_function_variations[index][0].Add(new float[4] { (float)mf_func.A, (float)mf_func.B, (float)mf_func.C, (float)mf_func.D });
            }
            index++;
        }
        for (int i = 0; i < membership_function_variations.Count; i++)
        {
            for(int j = 1; j < 10; j++)
            {
                membership_function_variations[i].Add(new List<float[]>());
                foreach (float[] mf in membership_function_variations[i][0])
                {
                    membership_function_variations[i][j].Add(new float[4] { mf[0], mf[1], mf[2], mf[3] });
                }

                // Generate middle trapezoid
                float mf_range = membership_function_variations[i][j][2][3] - membership_function_variations[i][j][2][0];
                float offset = Random.Range(-mf_range / 2f, mf_range / 2f);
                membership_function_variations[i][j][2][0] -= offset;
                membership_function_variations[i][j][2][3] += offset;
                membership_function_variations[i][j][2][1] = Random.Range(membership_function_variations[i][j][2][0], membership_function_variations[i][j][2][3]);
                membership_function_variations[i][j][2][2] = Random.Range(membership_function_variations[i][j][2][1], membership_function_variations[i][j][2][3]);

                // Generate middle left trapezoid
                mf_range = membership_function_variations[i][j][1][3] - membership_function_variations[i][j][1][0];
                offset = Random.Range(-mf_range / 2f, mf_range / 2f);
                membership_function_variations[i][j][1][0] += offset;
                membership_function_variations[i][j][1][3] = Random.Range(membership_function_variations[i][j][2][0], membership_function_variations[i][j][2][1]);
                membership_function_variations[i][j][1][2] = Random.Range(membership_function_variations[i][j][1][0], membership_function_variations[i][j][1][3]);
                membership_function_variations[i][j][1][1] = Random.Range(membership_function_variations[i][j][1][0], membership_function_variations[i][j][1][2]);

                // Generate middle right trapezoid
                mf_range = membership_function_variations[i][j][3][3] - membership_function_variations[i][j][3][0];
                offset = Random.Range(-mf_range / 2f, mf_range / 2f);
                membership_function_variations[i][j][3][0] = Random.Range(membership_function_variations[i][j][2][2], membership_function_variations[i][j][2][3]);
                membership_function_variations[i][j][3][3] += offset;
                membership_function_variations[i][j][3][1] = Random.Range(membership_function_variations[i][j][3][0], membership_function_variations[i][j][3][3]);
                membership_function_variations[i][j][3][2] = Random.Range(membership_function_variations[i][j][3][1], membership_function_variations[i][j][3][3]);

                // Generate rightmost trapezoid
                membership_function_variations[i][j][4][0] = Random.Range(membership_function_variations[i][j][3][2], membership_function_variations[i][j][3][3]);
                membership_function_variations[i][j][4][1] = Random.Range(membership_function_variations[i][j][4][0], membership_function_variations[i][j][4][3]);
                membership_function_variations[i][j][4][2] = Random.Range(membership_function_variations[i][j][4][1], membership_function_variations[i][j][4][3]);

                // Generate leftmost trapezoid
                membership_function_variations[i][j][0][3] = Random.Range(membership_function_variations[i][j][1][0], membership_function_variations[i][j][1][1]);
                membership_function_variations[i][j][0][2] = Random.Range(membership_function_variations[i][j][0][0], membership_function_variations[i][j][0][3]);
                membership_function_variations[i][j][0][1] = Random.Range(membership_function_variations[i][j][0][0], membership_function_variations[i][j][0][2]);

            }
        }

        writer = new StreamWriter("Assets/Results.txt", true);
        writer.WriteLine("========= Results =========");
        writer.WriteLine("");
        writer.WriteLine("----- Linguistic " + linguistic_index_ + ": ------");
        writer.Close();

        RunBatchTest();
    }

    public void RunBatchTest()
    {
        pause_menu_.GetComponent<PauseMenu>().Pause();
        var missiles = GameObject.FindGameObjectsWithTag("Missile");
        foreach (var m in missiles)
        {
            Destroy(m);
        }

        brain_.BatchUpdateMembershipValues(linguistic_index_, membership_function_variations[linguistic_index_][variation_index_]);

        brain_.RunTest();
        pause_menu_.GetComponent<PauseMenu>().Resume();
    }

    public void RunSingleTest()
    {
        running_single_test_ = true;
        player_.GetComponent<Jet>().BeginTesting();
        var missiles = GameObject.FindGameObjectsWithTag("Missile");
        foreach (var m in missiles)
        {
            Destroy(m);
        }
        brain_.RunTest();
        pause_menu_.GetComponent<PauseMenu>().Resume();
    }

    public void CompletedTest()
    {
        pause_menu_.GetComponent<PauseMenu>().Pause();

        if (!running_single_test_)
        {
            writer = new StreamWriter("Assets/Results.txt", true);
            writer.WriteLine("");
            writer.WriteLine("Membership Functions:");

            foreach (var membership_function in membership_function_variations[linguistic_index_][variation_index_])
            {
                writer.WriteLine(membership_function[0] + ", " + membership_function[1] + ", " + membership_function[2] + ", " + membership_function[3]);
            }
            writer.WriteLine("Hits: " + brain_.hits_ + ", Misses: " + brain_.misses_ + ", Downed: " + brain_.downed_);

            if (brain_.hits_ > highest_hits_[0])
            {
                highest_hits_[0] = brain_.hits_;
                highest_hits_[1] = variation_index_;
            }
            if (brain_.misses_ < lowest_misses_[0])
            {
                lowest_misses_[0] = brain_.misses_;
                lowest_misses_[1] = variation_index_;
            }
            if (brain_.downed_ < lowest_downed_[0])
            {
                lowest_downed_[0] = brain_.downed_;
                lowest_downed_[1] = variation_index_;
            }
            if (brain_.misses_ + brain_.downed_ < lowest_misses_and_downed_[0])
            {
                lowest_misses_and_downed_[0] = brain_.misses_ + brain_.downed_;
                lowest_misses_and_downed_[1] = variation_index_;
            }

            bool b_continue = true;

            variation_index_++;
            if (variation_index_ > membership_function_variations[linguistic_index_].Count - 1)
            {
                Debug.Log("Variation done testing");
                variation_index_ = 1;
                writer.WriteLine("--- Best Variants ---");
                writer.WriteLine("Most hits: " + highest_hits_[1] + ", Least misses: " + lowest_misses_[1] + ", Least downed: " + lowest_downed_[1] + ", Least misses and downed: " + lowest_misses_and_downed_[1]);

                highest_hits_ = new int[2] { int.MinValue, 0 };
                lowest_misses_ = lowest_downed_ = lowest_misses_and_downed_ = new int[2] { int.MaxValue, 0 };

                // reset to default
                brain_.BatchUpdateMembershipValues(linguistic_index_, membership_function_variations[linguistic_index_][0]);

                linguistic_index_++;
                if (linguistic_index_ > membership_function_variations.Count - 1)
                {
                    writer.WriteLine("");
                    writer.WriteLine("======= Finished =======");
                    b_continue = false;
                    player_.GetComponent<Jet>().EndTesting();
                }
                else
                {
                    Debug.Log("======== LINGUISTIC DONE =========");
                    writer.WriteLine("");
                    writer.WriteLine("----- Linguistic " + linguistic_index_ + ": ------");
                }
            }
            writer.Close();

            if (b_continue)
            {
                RunBatchTest();
            }
        }
        else
        {
            player_.GetComponent<Jet>().EndTesting();
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
