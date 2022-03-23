using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI Instance;

    public Text LoadedText;
    public Text UnloadedText;
    public Image Crosshair;
    public Image DamageTaint;
    public Image DamageCover;
    public Slider HealthBar;
    public Image Cover;
    public Text MessageText;
    public GameObject PauseMenu;
    public GameObject ControllsMenu;

    [HideInInspector] public bool Paused;

    private bool restarting;
    private Color messageColor;
    private bool showingMessage;
    private bool pauseBlocked;

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this);

        Cover.color = Color.black;
        AudioListener.volume = 0;

        Invoke("ShowFirstMessage", 1);
    }

    private void Update()
    {
        if (Paused) return;

        if (DamageCover.color.a >= 0) DamageCover.color -= new Color(0.25f, 0, 0) * Time.deltaTime * 5;
        else DamageCover.color = new Color(0.25f, 0, 0, 0);

        if (restarting)
        {
            
            AudioListener.volume = AudioListener.volume > 0 ? AudioListener.volume - Time.deltaTime : 0;
            Cover.color += Color.black * Time.deltaTime;
            if (Cover.color.a >= 1)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        else if(!EndingTrigger.GameOver)
        {
            AudioListener.volume = AudioListener.volume < 1 ? AudioListener.volume + Time.deltaTime : 1;
            Cover.color = Cover.color.a >= 0 ? Cover.color - Color.black * Time.deltaTime * 2 : Color.clear;
        }

        if (showingMessage) MessageText.color = MessageText.color.a < 1 ? MessageText.color + messageColor * Time.deltaTime : messageColor;
        else
        {
            float alpha = MessageText.color.a - Time.deltaTime;
            MessageText.color = MessageText.color.a > 0 ? MessageText.color = new Color(messageColor.r, messageColor.g, messageColor.b, alpha) : Color.clear;
        }
        
        if (Input.GetButtonDown("Pause") && CanPause())
        {
            Paused = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
            PauseMenu.SetActive(true);
            MessageText.gameObject.SetActive(false);
        }
    }

    private bool CanPause() => !EndingTrigger.GameOver && !Player.Instance.Dead && !pauseBlocked;

    public void UpdateAmmoBox()
    {
        LoadedText.text = Gun.Instance.LoadedAmmo + "";
        UnloadedText.text = Gun.Instance.UnloadedAmmo + "";
        if (Player.Instance.GodMode) UnloadedText.text = "999";
    }

    public void UpdateHealthBar()
    {
        HealthBar.value = Player.Instance.Health;

        float alpha = (float)-Player.Instance.Health;
        alpha = alpha / 20f + 0.5f;

        DamageTaint.color = new Color(1, 1, 1, alpha);
    }

    public void Restart()
    {
        restarting = true;
    }

    private void ShowFirstMessage()
    {
        ShowMessage("Reach The Safe Zone.", Color.white);
    }

    public void ShowMessage(string message, Color color)
    {
        MessageText.color = new Color(color.r, color.g, color.b, 0);
        messageColor = color;
        MessageText.text = message;

        showingMessage = true;
        Invoke("StopMessage", 3);
    }

    private void StopMessage()
    {
        showingMessage = false;
    }

    public void HandlePauseResume()
    {
        MessageText.gameObject.SetActive(true);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        PauseMenu.SetActive(false);
        Paused = false;
    }

    public void HandleControllsResume()
    {
        PauseMenu.SetActive(true);
        ControllsMenu.SetActive(false);
    }

    public void HandleRestart()
    {
        HandlePauseResume();
        pauseBlocked = true;
        Restart();
    }

    public void HandleQuit()
    {
        Application.Quit();
    }

    public void HandleControlls()
    {
        ControllsMenu.SetActive(true);
        PauseMenu.SetActive(false);
    }
}