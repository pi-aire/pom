using UnityEngine;
using System.Collections;

public class LifeController : MonoBehaviour
{
    public TextMesh text;

    public void setLifePoint(int l)
    {
        text.text = l.ToString();
        text.color = new UnityEngine.Color(1,0,0);
    }

    public void setTowerHeight(int h)
    {
        text.text = $"T{h}";
        text.color = new UnityEngine.Color(0,0.2f,0.8f);
    }

    public void viewBurn(int b)
    {
        //text.text = text.text + $" Burn:{b} ";
    }
}
