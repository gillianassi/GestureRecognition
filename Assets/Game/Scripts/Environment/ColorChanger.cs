using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    [SerializeField] private Renderer myObject;

    public List<Color> colorList = new List<Color>();

   public void ChangeColor(int colorIdx)
    {
        if(colorIdx < colorList.Count)
        {
            myObject.material.color = colorList[colorIdx];
        }
    }
}
