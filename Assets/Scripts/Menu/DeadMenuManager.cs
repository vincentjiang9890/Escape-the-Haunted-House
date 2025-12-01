using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadMenuManager : MonoBehaviour
{
    public void RetryScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void MainMenuScreen()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
    }


    public void ExitGame()
    {
        Application.Quit();
    }
}
