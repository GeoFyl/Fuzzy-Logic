using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    Material skybox_mat_;
    [SerializeField]
    List<GameObject> visualObjects;
    [SerializeField]
    List<GameObject> simpleObjects;

    public bool paused = false;

    public void DisableVisuals(bool disabled)
    {
        if (disabled)
        {
            RenderSettings.skybox = null;
        }
        else
        {
            RenderSettings.skybox = skybox_mat_;
        }

        foreach (GameObject go in visualObjects)
        {
            go.SetActive(!disabled);
        }
        foreach (GameObject go in simpleObjects)
        {
            go.GetComponent<MeshRenderer>().enabled = disabled;
        }

        GameObject[] missiles = GameObject.FindGameObjectsWithTag("Missile").Concat(GameObject.FindGameObjectsWithTag("PlayerMissile")).ToArray();
        foreach (GameObject missile in missiles) {
            missile.GetComponent<MeshRenderer>().enabled = disabled;
            missile.transform.GetChild(0).gameObject.SetActive(!disabled);
        }
    }

    public void Resume()
    {
        paused = false;
        transform.GetChild(0).gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    private void Start()
    {
        DisableVisuals(true);
        Resume();
    }

    public void Pause()
    {
        paused = true;
        transform.GetChild(0).gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) 
        {
            if (paused) Resume();
            else Pause();
        }
    }
}
