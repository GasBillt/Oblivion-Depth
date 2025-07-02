using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DeathManager : MonoBehaviour
{
    public bool Dead = false;
    public int deathCNT = 0;
    private AudioListener playerAudioListener;
    private float originalVolume; // Сохраняем оригинальную громкость

    [Header("Death Settings")]
    public Image blackPanel;
    public float delay = 2f;       // Задержка после затемнения
    public float fadeDuration = 1f;  // Длительность затемнения
    public bool isStart = false;

    void Start()
    {
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
        
        // Сохраняем оригинальную громкость
        originalVolume = AudioListener.volume;
    }

    void Update()
    {
        if (Dead) Death("TestPlace");
    }

    public void Death(string sceneToLoad)
    {
        StartCoroutine(DeathSequence(sceneToLoad));
        Dead = true;
    }

    private IEnumerator DeathSequence(string sceneToLoad)
    {
        // Сохраняем оригинальную громкость перед изменением
        float volumeBeforeDeath = AudioListener.volume;
        
        // Плавное снижение громкости
        float timer = 0f;
        while (timer < fadeDuration / 2)
        {
            timer += Time.deltaTime;
            AudioListener.volume = Mathf.Lerp(volumeBeforeDeath, 0f, timer / (fadeDuration / 2));
            yield return null;
        }
        AudioListener.volume = 0f;

        // Запускаем визуальный эффект
        if (blackPanel != null)
        {
            blackPanel.gameObject.SetActive(true);
            yield return StartCoroutine(FadeToBlack());
        }

        // Задержка перед загрузкой сцены
        yield return new WaitForSeconds(delay);

        // Восстанавливаем громкость
        AudioListener.volume = originalVolume;
        
        // Загружаем сцену
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
}