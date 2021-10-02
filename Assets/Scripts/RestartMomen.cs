using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartMomen : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene("Gameplay");
    }
}
