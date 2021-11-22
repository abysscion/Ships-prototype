using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singlton class, that realizes global game settings and parameters. Provides methods to control game flow.
/// </summary>
public class GameController : MonoBehaviour
{
    /// <summary>
    /// Current instance
    /// </summary>
    public static GameController Instance { get; private set; }

    /// <summary>
    /// Link to a player ship controller
    /// </summary>
    public PlayerShipController playerShip;

    /// <summary>
    /// Calls when enemies counter changed
    /// </summary>
    public System.Action<int> OnDestroyedEnemiesCounterChanged;

    private int _destroyedEnemies;

    /// <summary>
    /// Destroyed enemies counter
    /// </summary>
    public int DestroyedEnemies 
    {
        get => _destroyedEnemies;
        set
        {
            _destroyedEnemies = value;
            OnDestroyedEnemiesCounterChanged?.Invoke(_destroyedEnemies);
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    /// <summary>
    /// Restarts game via loading same scene again
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        DestroyedEnemies = 0;
        playerShip = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerShipController>();
        playerShip.OnPlayerDeath += OnPlayerDeathAction;
        Time.timeScale = 1;
    }

    private void OnPlayerDeathAction()
    {
        Time.timeScale = 0;
    }
}
