using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    [SerializeField] private Renderer myObject;

    public List<Color> colorList = new List<Color>();
    public List<Color> UnselectColorList = new List<Color>();

    private int CurrentColor = 0;

    public void ChangeColor(int colorIdx)
    {
        CurrentColor = colorIdx;
        if (CurrentColor < colorList.Count)
        {
            myObject.material.color = colorList[CurrentColor];
        }
    }

    public void Unselect()
    {
        if(CurrentColor < UnselectColorList.Count)
        {
            myObject.material.color = UnselectColorList[CurrentColor];
        }
    }
}
