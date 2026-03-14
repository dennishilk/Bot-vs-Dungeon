using UnityEngine;

public class ModeButton : MonoBehaviour
{
    [SerializeField] private BuildModeController buildModeController;
    [SerializeField] private int tileTypeValue;

    public void OnClick()
    {
        buildModeController.SelectTileType(tileTypeValue);
    }
}
