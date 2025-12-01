using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;

    [SerializeField] private AudioSource bgmAudioSource = default;
    [SerializeField] private AudioClip bgmAudio21 = default;
    [SerializeField] private AudioClip bgmAudio45 = default;

    void Start()
    {
        bgmAudioSource.volume = 0.2f;

        bgmAudioSource.PlayOneShot(bgmAudio21);
        bgmAudioSource.PlayOneShot(bgmAudio45);


        StartCoroutine(PlayAudioAfterDelay(bgmAudio21, 28f));
        StartCoroutine(PlayAudioAfterDelay(bgmAudio45, 54f));
    }

    IEnumerator PlayAudioAfterDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        bgmAudioSource.PlayOneShot(clip);
    }



    public void UpdateText(string promptMessage)
    {
        promptText.text = promptMessage;
    }
}
