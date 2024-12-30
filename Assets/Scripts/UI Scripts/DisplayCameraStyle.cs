using TMPro;
using UnityEngine;

public class DisplayCameraStyle : MonoBehaviour
{
    [SerializeField]
    public TMP_Text cameraStyleText;

    private CombinationController_RB player;

    private void Awake()
    {
        player = Object.FindFirstObjectByType<CombinationController_RB>();
    }

    private void Update()
    {
        cameraStyleText.text = player.currentStyle.ToString();
    }
}
