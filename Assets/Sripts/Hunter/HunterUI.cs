using UnityEngine;
using TMPro;
public class HunterUI : MonoBehaviour
{
    [Header("Referencias")]
    public HunterFSM hunter;
    public TextMeshProUGUI statusText;

    void Update()
    {
        if (hunter == null || statusText == null) return;

        statusText.text = $"<b>HUNTER</b>\n" +
                          $"State: {hunter.CurrentStateName}\n" +
                          $"Energy: {Mathf.CeilToInt(hunter.CurrentEnergy)} / {hunter.maxEnergy}";
    }
}