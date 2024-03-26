using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;


public class FuzzyMissile : MonoBehaviour {

	GameObject player_;
    Camera camera_;
    FuzzyBrain brain_;
    float forward_speed_;

	void Start()
	{
		player_ = GameObject.FindWithTag("Player");
        camera_ = Camera.main;
    }

    public void InitMissile(FuzzyBrain brain, float speed, bool visuals_disabled)
    {
        brain_ = brain;
        forward_speed_ = speed;

        GetComponent<MeshRenderer>().enabled = visuals_disabled;
        transform.GetChild(0).gameObject.SetActive(!visuals_disabled);
    }

    void FixedUpdate()
    {
        // Counts as a miss when goes out of camera viewport
        Vector3 viewpoint = camera_.WorldToViewportPoint(transform.position);
        if(viewpoint.x < 0 || viewpoint.x > 1 || viewpoint.y < 0 || viewpoint.y > 1)
        {
            brain_.Miss();
            Destroy(gameObject);
        }

        // Find closest player missile
        GameObject[] player_missiles = GameObject.FindGameObjectsWithTag("PlayerMissile");
        GameObject closest_missile = null;
        float distance = float.MaxValue;
        float pm_forward_dist = float.MaxValue;
        foreach (var missile in player_missiles)
        {
            float new_dist = Vector3.Distance(missile.transform.position, transform.position);
            if (new_dist < distance && transform.position.z - missile.transform.position.z >= 0)
            {
                distance = new_dist;
                closest_missile = missile;
            }
        }
       
        float pm_side_dist = float.MaxValue;
        float pm_vertical_dist = float.MaxValue;
        if (closest_missile != null)
        {
            pm_forward_dist = transform.position.z - closest_missile.transform.position.z;
            pm_side_dist = transform.position.x - closest_missile.transform.position.x;
            pm_vertical_dist = transform.position.y - closest_missile.transform.position.y;
        }

        // Convert position of player to value and use fuzzy inference to find new velocity
        float target_forward_distance = transform.position.z - player_.transform.position.z;
        float sideways_result = brain_.DefuzzifySideways(transform.position.x - player_.transform.position.x, target_forward_distance, pm_forward_dist, pm_side_dist);
        float vertical_result = brain_.DefuzzifyVertical(transform.position.y - player_.transform.position.y, target_forward_distance, pm_forward_dist, pm_vertical_dist);
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = new Vector3(sideways_result, vertical_result, -forward_speed_);

        transform.rotation = Quaternion.LookRotation(rigidbody.velocity == Vector3.zero ? transform.forward : rigidbody.velocity);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == player_)
        {
            //Debug.Log("Hit player!");
            brain_.Hit();
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "PlayerMissile")
        {
            //Debug.Log("Missile shot down");
            brain_.ShotDown();
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update () {
	}
}
