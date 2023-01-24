using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectController : MonoBehaviour
{
    public GameObject Object;

    private float currentTime = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }


    public void ToggleGameObject()
    {
        if (Object == null)
        {
            return;
        }

        Object.SetActive(!Object.activeSelf);
    }

    public void ActivateGameObject()
    {
        if (Object == null) 
        { 
            return; 
        }

        Object.SetActive(true);
    }
    public void DeactivateGameObject()
    {
        if (Object == null)
        {
            return;
        }

        Object.SetActive(false);
    }
}
