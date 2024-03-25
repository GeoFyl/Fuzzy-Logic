using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


public class PlayerMissile : MonoBehaviour {
    [SerializeField]
    float forward_speed_ = 10;

	void Start()
	{
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = new Vector3(0, 0, forward_speed_);
        transform.rotation = Quaternion.LookRotation(rigidbody.velocity);
    }

    public void InitMissile(bool visuals_disabled)
    {
        GetComponent<MeshRenderer>().enabled = visuals_disabled;
        transform.GetChild(0).gameObject.SetActive(!visuals_disabled);
    }

    private void FixedUpdate()
    {
        if(transform.position.z > 65)
        {
            Destroy(gameObject);
        }
    }
}
