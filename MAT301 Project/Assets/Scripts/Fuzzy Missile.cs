using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FLS;
using FLS.Rules;
using FLS.MembershipFunctions;

public class FuzzyMissile : MonoBehaviour {

	bool selected = false;
	GameObject player_;

	IFuzzyEngine sideways_engine_, forward_engine_;
	LinguisticVariable sideways_distance_, forward_distance_, sideways_direction_, forward_direction_; 

	void Start()
	{
		player_ = GameObject.FindWithTag("Player");

		// ---------- Setup Fuzzy Inference System -----------

		// Engine for sideways movement (x axis)
		sideways_engine_ = new FuzzyEngineFactory().Default();

		sideways_distance_ = new LinguisticVariable("sideways_distance");
		var left_of_target = sideways_distance_.MembershipFunctions.AddTrapezoid("left", -50, -50, -5, -1);
        var inline_with_target = sideways_distance_.MembershipFunctions.AddTrapezoid("none", -5, -0.5, 0.5, 5);
        var right_of_target = sideways_distance_.MembershipFunctions.AddTrapezoid("right", 1, 5, 50, 50);
        sideways_direction_ = new LinguisticVariable("sideways_direction");
		var steer_left = sideways_direction_.MembershipFunctions.AddTrapezoid("left", -50, -50, -5, -1);
        var stay_centred = sideways_direction_.MembershipFunctions.AddTrapezoid("none", -5, -0.5, 0.5, 5);
        var steer_right = sideways_direction_.MembershipFunctions.AddTrapezoid("right", 1, 5, 50, 50);
        var sideways_rule_1 = Rule.If(sideways_distance_.Is(right_of_target)).Then(sideways_direction_.Is(steer_left));
        var sideways_rule_2 = Rule.If(sideways_distance_.Is(left_of_target)).Then(sideways_direction_.Is(steer_right));
        var sideways_rule_3 = Rule.If(sideways_distance_.Is(inline_with_target)).Then(sideways_direction_.Is(stay_centred));
        sideways_engine_.Rules.Add(sideways_rule_1, sideways_rule_2, sideways_rule_3);

        // Engine for forward direction movement (z axis)
        forward_engine_ = new FuzzyEngineFactory().Default();

        forward_distance_ = new LinguisticVariable("forward_distance");
		var behind_target = forward_distance_.MembershipFunctions.AddTrapezoid("behind1", -50, -50, -5, -1);
        var alongside_target = forward_distance_.MembershipFunctions.AddTrapezoid("none1", -5, -0.5, 0.5, 5);
        var in_front_of_target = forward_distance_.MembershipFunctions.AddTrapezoid("infront1", 1, 5, 50, 50);
        forward_direction_ = new LinguisticVariable("forward_direction");
		var move_back = forward_direction_.MembershipFunctions.AddTrapezoid("back1", -50, -50, -5, -1);
        var stay_alongside = forward_direction_.MembershipFunctions.AddTrapezoid("none1", -5, -0.5, 0.5, 5);
        var move_forward = forward_direction_.MembershipFunctions.AddTrapezoid("forward1", 1, 5, 50, 50);
		var forward_rule_1 = Rule.If(forward_distance_.Is(in_front_of_target)).Then(forward_direction_.Is(move_back));
		var forward_rule_2 = Rule.If(forward_distance_.Is(behind_target)).Then(forward_direction_.Is(move_forward));
		var forward_rule_3 = Rule.If(forward_distance_.Is(alongside_target)).Then(forward_direction_.Is(stay_alongside));
		forward_engine_.Rules.Add(forward_rule_1, forward_rule_2, forward_rule_3);

    }

	void FixedUpdate()
	{
        if (!selected)
		{
			// Convert position of box to value between 0 and 100
			double sideways_result = sideways_engine_.Defuzzify(new { sideways_distance = (double)transform.position.x - player_.transform.position.x});
			double forward_result = forward_engine_.Defuzzify(new { forward_distance = (double)transform.position.z - player_.transform.position.z});

			Rigidbody rigidbody = GetComponent<Rigidbody>();
			rigidbody.AddForce(new Vector3((float)sideways_result, 0f, (float)forward_result));
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit)){
				if (hit.rigidbody.gameObject == gameObject)
				{
					selected = true;
				}
			}
		}

		if(Input.GetMouseButton(0) && selected)
		{
			float distanceToScreen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
			Vector3 curPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToScreen));
			transform.position = new Vector3(curPosition.x, Mathf.Max(0.5f, curPosition.y), transform.position.z);
		}

		if(Input.GetMouseButtonUp(0))
		{
			selected = false;
		}
	}
}
