using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BuildModeController buildModeController;
    [SerializeField] private SimulationManager simulationManager;

    private void Start()
    {
        buildModeController.SetBuildMode(true);
    }

    public void OnRunButtonPressed()
    {
        simulationManager.StartSimulation();
    }
}
