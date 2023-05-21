using UnityEngine;

public class RandomObjectColor : MonoBehaviour
{
    private void Awake()
    {
        this.GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0.0f, 1.0f, 0.75f, 1.0f, 0.5f, 1.0f);
        // GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0.0f, 1.0f, 0.75f, 1.0f, 0.5f, 1.0f);
        // Either or of the above will work just fine. I think I prefer the 'this' because, to me, it is a bit more verbose.
    }
}
