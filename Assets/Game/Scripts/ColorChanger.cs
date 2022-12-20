using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    [SerializeField] private Material myMaterial;

    public List<Color> colorList = new List<Color>();

   public void ChangeColor(int colorIdx)
    {
        if(colorIdx < colorList.Count)
        {
            myMaterial.color = colorList[colorIdx];
        }
    }
}
