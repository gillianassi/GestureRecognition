using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

public class PointVisualiserManager : MonoBehaviour
{

    public GameObject DebugVisualisation;

    private List<GameObject> currentDebugVisualisation = new List<GameObject>();


    public void AddPoint(Vector3 position)
    {
        if (DebugVisualisation)
        {
            currentDebugVisualisation.Add(Instantiate(DebugVisualisation, position, Quaternion.identity));
        }
    }
    public void RemoveAllPoints()
    {
        for (int i = 0; i < currentDebugVisualisation.Count; i++)
        {
            Destroy(currentDebugVisualisation[i]);
        }
        currentDebugVisualisation.Clear();
    }
}
