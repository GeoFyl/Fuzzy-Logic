using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chopper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.GetChild(0).Rotate(Vector3.forward, -720 * Time.deltaTime);
        transform.GetChild(1).Rotate(Vector3.right, -580 * Time.deltaTime);
    }
}
