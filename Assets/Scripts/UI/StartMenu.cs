using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public bool isPaused = false;

    public GameObject pauseMenuUI;

    private void Start()
    {
        Resume();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        //Cursor.lockState = CursorLockMode.Locked;
       // Cursor.visible = false;
    }
    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
       // Cursor.lockState = CursorLockMode.None;
       // Cursor.visible = true;
    }

    public void Restart() 
    {         
        Debug.Log("Restarting game...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        Resume();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }


}

