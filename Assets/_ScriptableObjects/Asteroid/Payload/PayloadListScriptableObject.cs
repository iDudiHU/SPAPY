using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PayloadList", menuName = "ScriptableObjects/Asteroid/PayloadListScriptableObject", order = 2)]
public class PayloadListScriptableObject : ScriptableObject
{
    [SerializeField] private List<PayloadScriptableObject> payloadList = new List<PayloadScriptableObject>();
    public List<PayloadScriptableObject> PayloadList
    {
        get { return payloadList; }
        set { payloadList = value; }
    }
}
