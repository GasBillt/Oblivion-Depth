using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DeathManager : MonoBehaviour
{
    public bool Dead = false;
    public int deathCNT = 0;
    private AudioListener playerAudioListener;
    private float originalVolume;

    [Header("Death Settings")]
    public Image blackPanel;
    public float delay = 2f;
    public float fadeDuration = 1f;
    public bool isStart = false;

    // Ключ для сохранения счетчика смертей
    private const string DEATH_COUNT_KEY = "DeathCount";

    void Start()
    {
        // Загружаем сохраненное значение счетчика смертей
        deathCNT = PlayerPrefs.GetInt(DEATH_COUNT_KEY, 0);
        
        if (blackPanel != null)
        {
            blackPanel.color = new Color(0, 0, 0, 0);
            blackPanel.gameObject.SetActive(false);
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("MainPlayer");
        if (player != null)
        {
            playerAudioListener = player.GetComponent<AudioListener>();
        }
        
        originalVolume = AudioListener.volume;
    }

    void Update()
    {
        if (Dead) Death("TestPlace");
    }

    public void Death(string sceneToLoad)
    {
        // Увеличиваем счетчик только при первой активации смерти
        if (!Dead)
        {
            deathCNT++;
            PlayerPrefs.SetInt(DEATH_COUNT_KEY, deathCNT);
            PlayerPrefs.Save();
        }
        
        StartCoroutine(DeathSequence(sceneToLoad));
        Dead = true;
    }

    private IEnumerator DeathSequence(string sceneToLoad)
    {
        float volumeBeforeDeath = AudioListener.volume;
        
        float timer = 0f;
        while (timer < fadeDuration / 2)
        {
            timer += Time.deltaTime;
            AudioListener.volume = Mathf.Lerp(volumeBeforeDeath, 0f, timer / (fadeDuration / 2));
            yield return null;
        }
        AudioListener.volume = 0f;

        if (blackPanel != null)
        {
            blackPanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadeToBlack());
        }

        yield return new WaitForSeconds(delay);
        AudioListener.volume = originalVolume;
        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator FadeToBlack()
    {
        float timer = 0f;
        Color startColor = new Color(0, 0, 0, 0);
        Color endColor = Color.black;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            blackPanel.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            yield return null;
        }
        blackPanel.color = endColor;
    }

    // Опционально: метод для сброса счетчика
    public void ResetDeathCount()
    {
        deathCNT = 0;
        PlayerPrefs.SetInt(DEATH_COUNT_KEY, 0);
        PlayerPrefs.Save();
    }
}