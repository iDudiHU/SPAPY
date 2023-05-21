using UnityEngine;
using TMPro;

public class OnScreenCounter : MonoBehaviour
{
    public TextMeshProUGUI _itemCounter;
    public int Counter { get; set; } = 0;

    void Update()
    {
        _itemCounter.SetText($"Boxes Collected - {Counter}");
    }
}
