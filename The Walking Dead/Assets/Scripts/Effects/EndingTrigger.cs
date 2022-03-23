using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingTrigger : MonoBehaviour
{
    public static bool GameOver;

    private float timer;

    void Start()
    {
        
    }

    void Update()
    {
        if (GameOver)
        {
            timer += Time.deltaTime;

            AudioListener.volume -= Time.deltaTime * 0.5f;
            if (timer < 2.5f) UI.Instance.Cover.color += Color.white * Time.deltaTime * 0.5f;

            if (timer > 2.5f && timer < 3.5f)
            {
                UI.Instance.Cover.color = Color.white;
                UI.Instance.ShowMessage("Safe Zone Reached.", Color.black);
                timer = 3.5f;
            }

            if (timer > 8)
            {
                float value = UI.Instance.Cover.color.r - Time.deltaTime;
                UI.Instance.Cover.color = new Color(value, value, value, 1);
            }

            if (timer > 9)
            {
                Application.Quit();
                GameOver = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Player.Instance.Dead || GameOver) return;
        UI.Instance.Cover.color = new Color(1, 1, 1, 0);
        UI.Instance.MessageText.transform.SetAsLastSibling();
        GameOver = true;
    }
}