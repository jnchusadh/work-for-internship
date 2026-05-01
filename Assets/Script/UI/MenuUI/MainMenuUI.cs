using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public GameObject mainMenuPanel;

    public Vector3 spawnPos = new Vector3(5.4f, -5.6f, 0f);

    private void Start()
    {
        ShowMainMenu();
    }

    public void OnStartButtonClicked()
    {
        HideMainMenu();
        LoadFarmScene();
    }

    public void OnEndButtonClicked()
    {
        Application.Quit();
    }

    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    private void HideMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }

    private void LoadFarmScene()
    {
        if (SceneChangeManager.Instance != null)
        {
            SceneChangeManager.Instance.FadeAndLoadScene(
                SceneName.Scene1_Farm.ToString(),
                spawnPos
            );
        }
        else
        {
            SceneManager.LoadScene("Scene1_Farm", LoadSceneMode.Additive);
        }
    }
}