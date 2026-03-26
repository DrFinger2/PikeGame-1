using UnityEngine;

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RandomSFXPlayer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip[] clips;

    [Header("Timing (seconds)")]
    [SerializeField] private float minDelay = 20f;
    [SerializeField] private float maxDelay = 60f; // must not exceed 60

    private AudioSource audioSource;
    private Coroutine playRoutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        maxDelay = Mathf.Min(maxDelay, 60f);
    }

    private void OnEnable()
    {
        playRoutine = StartCoroutine(PlayLoop());
    }

    private void OnDisable()
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);
    }

    private IEnumerator PlayLoop()
    {
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            PlayRandomClip();

            // Enforce hard cooldown (no more than once every 2 minutes)
            yield return new WaitForSeconds(60f);
        }
    }

    private void PlayRandomClip()
    {
        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }
}

