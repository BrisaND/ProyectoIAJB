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

        statusText.text = $"<b>CAZADOR</b>\n" +
                          $"Estado: {hunter.CurrentStateName}\n" +
                          $"Energía: {Mathf.CeilToInt(hunter.CurrentEnergy)} / {hunter.maxEnergy}";
    }
}