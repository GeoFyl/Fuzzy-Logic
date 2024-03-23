using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
using FLS;
using FLS.Rules;
using FLS.MembershipFunctions;

public class MembershipFunctionValues
{
    public MembershipFunctionValues(string name, params float[] vals)
    {
        function_name = name;
        foreach (var val in vals) { 
            values.Add(val);
        }
       
    }

    public string function_name;
    public List<float> values = new List<float>();

    //IMembershipFunction function;
}
