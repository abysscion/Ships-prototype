using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Realizes UI logic
/// </summary>
public class UIController : MonoBehaviour
{
    public CanvasGroup gameOverGroup;
    public TMP_Text enemiesDestroyedCounterTxt;
    public TMP_Text cannonBallsCounterTxt;
    public Image cannonballLoaderImg;
    
    private PlayerShipController _playerShip;

    private void Start()
    {
        var gcInst = GameController.Instance;

        gameOverGroup.alpha = 0;
        gameOverGroup.blocksRaycasts = false;
        gameOverGroup.interactable = false;
        cannonballLoaderImg.fillAmount = 0;
        _playerShip = gcInst.playerShip;
        cannonBallsCounterTxt.text = $"{_playerShip.CannonballsAmount}";
        enemiesDestroyedCounterTxt.text = $"{gcInst.DestroyedEnemies}";

        Subscribe();
        StartCoroutine(AnimateCannonballLoader());
    }

    private void OnDestroy()
    {
        Unsubcribe();
    }

    public void OnButtonClick_RestartGame()
    {
        GameController.Instance.RestartGame();
    }

    private IEnumerator AnimateCannonballLoader()
    {
        var timeLeft = _playerShip.cannonballsIncomeDelay;

        while (timeLeft >= 0)
        {
            var curDelta = 1 - timeLeft / _playerShip.cannonballsIncomeDelay;

            cannonballLoaderImg.fillAmount = Mathf.Lerp(0, 1, curDelta);
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        cannonballLoaderImg.fillAmount = 1;
    }

    private void Subscribe()
    {
        GameController.Instance.OnDestroyedEnemiesCounterChanged += OnDestroyedEnemiesCounterChanged;
        _playerShip.OnCannonballAmountChanged += OnPlayerCannonballAmountChanged;
        _playerShip.OnCannonballLoad += OnPlayerCannonballLoaded;
        _playerShip.OnPlayerDeath += OnPlayerDeathAction;
    }

    private void Unsubcribe()
    {
        GameController.Instance.OnDestroyedEnemiesCounterChanged -= OnDestroyedEnemiesCounterChanged;
    }

    private void OnPlayerDeathAction()
    {
        gameOverGroup.alpha = 1;
        gameOverGroup.blocksRaycasts = true;
        gameOverGroup.interactable = true;
    }

    private void OnPlayerCannonballLoaded()
    {
        cannonballLoaderImg.fillAmount = 0;
        StartCoroutine(AnimateCannonballLoader());
    }

    private void OnPlayerCannonballAmountChanged(int newAmount)
    {
        cannonBallsCounterTxt.text = $"{newAmount}";
    }

    private void OnDestroyedEnemiesCounterChanged(int newAmount)
    {
        enemiesDestroyedCounterTxt.text = $"{newAmount}";
    }
}
