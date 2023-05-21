using UnityEngine;

[CreateAssetMenu(fileName = "NewColorData", menuName = "ScriptableObjects/Color/ColorData", order = 1)]
public class ColorData : ScriptableObject
{
    [ColorUsage(false, true)]
    public Color hdrColor;
    public Color[] colors;
}
