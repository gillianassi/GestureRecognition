
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class PoseNameVisualiser : MonoBehaviour
{
    public List<ActiveStateSelector> Poses;
    public TMPro.TextMeshPro Text;

    // Start is called before the first frame update
    void Start()
    {
        foreach(var pose in Poses)
        {
            pose.WhenSelected += () => SetTextToPoseName(pose.gameObject.name);
            pose.WhenUnselected += () => SetTextToPoseName("");
        }
    }

    // Update is called once per frame
    public void SetTextToPoseName(string newText)
    {
        Text.text = newText;
    }
}
