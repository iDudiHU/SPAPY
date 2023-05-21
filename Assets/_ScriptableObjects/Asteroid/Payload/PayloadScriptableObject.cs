using UnityEngine;

[CreateAssetMenu(fileName = "PayloadData", menuName = "ScriptableObjects/Asteroid/PayloadScriptableObject", order = 1)]
public class PayloadScriptableObject : ScriptableObject
{
    public GameObject asteroid;
    public float weight;
    public int minSpawns;
    public int maxSpawns;
    public Texture2D[] emissionTextures;
    public ColorData colors;
    public Material baseMaterial;
}
