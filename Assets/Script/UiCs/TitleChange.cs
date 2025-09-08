using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleChange : MonoBehaviour
{
    public void StartButton()
    {
        Debug.Log("Start");
        SceneManager.LoadScene("GameStage");
    }
}
