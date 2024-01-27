using TMPro;
using UnityEngine;

public class BTDebugUI : MonoBehaviour
{
    [SerializeField] BehaviourTree LinkedBT;
    [SerializeField] TextMeshProUGUI LinkedDebugText;

    // Start is called before the first frame update
    void Start()
    {
        LinkedDebugText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        LinkedDebugText.text = LinkedBT.GetDebugText();
    }
}
