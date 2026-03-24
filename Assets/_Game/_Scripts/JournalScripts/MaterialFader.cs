using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialFader : MonoBehaviour
{
    [Header("Assign your materials here")]
    [SerializeField] private List<Material> materials = new List<Material>();

    [Header("Fade Out")]
    [SerializeField, Range(0f, 1f)] private float fadeOutOpacity = 0f;
    [SerializeField, Range(0f, 1f)] private float fadeOutSmoothness = 0f;

    [Header("Fade In")]
    [SerializeField, Range(0f, 1f)] private float fadeInOpacity = 1f;
    [SerializeField, Range(0f, 1f)] private float fadeInSmoothness = 0.5f;

    private List<Color> originalColors = new List<Color>();
    private List<float> originalSmoothness = new List<float>();

    void Start()
    {
        // Cache the original Base Color and Smoothness values strictly for the OnDestroy reset
        foreach (Material material in materials)
        {
            if (material == null) continue;

            if (material.HasProperty("_BaseColor"))
                originalColors.Add(material.GetColor("_BaseColor"));
            else
                originalColors.Add(Color.white);

            if (material.HasProperty("_Smoothness"))
                originalSmoothness.Add(material.GetFloat("_Smoothness"));
            else
                originalSmoothness.Add(0f);
        }
    }


    public void FadeOut(float duration, Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateMaterials(fadeOutOpacity, fadeOutSmoothness, duration, onComplete));
    }

    public void FadeIn(float duration, Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateMaterials(fadeInOpacity, fadeInSmoothness, duration, onComplete));
    }

    private IEnumerator AnimateMaterials(float targetAlpha, float targetSmoothness, float duration, Action onComplete)
    {
        if (materials.Count == 0) yield break;

        // Grab current values so we can lerp smoothly from wherever we are right now
        List<float> startAlphas = new List<float>();
        List<float> startSmoothnesses = new List<float>();

        for (int i = 0; i < materials.Count; i++)
        {
            startAlphas.Add(materials[i].HasProperty("_BaseColor") ? materials[i].GetColor("_BaseColor").a : 1f);
            startSmoothnesses.Add(materials[i].HasProperty("_Smoothness") ? materials[i].GetFloat("_Smoothness") : 0f);
        }

        // Only run the timer loop if duration is greater than 0
        if (duration > 0f)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration); 

                for (int i = 0; i < materials.Count; i++)
                {
                    if (materials[i] == null) continue;

                    // Interpolate towards the Inspector targets
                    float currentAlpha = Mathf.Lerp(startAlphas[i], targetAlpha, t);
                    float currentSmoothness = Mathf.Lerp(startSmoothnesses[i], targetSmoothness, t);

                    if (materials[i].HasProperty("_BaseColor"))
                    {
                        Color updatedColor = materials[i].GetColor("_BaseColor"); 
                        updatedColor.a = currentAlpha;
                        materials[i].SetColor("_BaseColor", updatedColor);
                    }

                    if (materials[i].HasProperty("_Smoothness"))
                    {
                        materials[i].SetFloat("_Smoothness", currentSmoothness);
                    }
                }

                yield return null;
            }
        }

        // =========================================================================
        // THE FIX: GUARANTEE EXACT TARGET VALUES ARE APPLIED WHEN THE LOOP ENDS
        // =========================================================================
        for (int i = 0; i < materials.Count; i++)
        {
            if (materials[i] == null) continue;

            if (materials[i].HasProperty("_BaseColor"))
            {
                Color finalColor = materials[i].GetColor("_BaseColor"); 
                finalColor.a = targetAlpha;
                materials[i].SetColor("_BaseColor", finalColor);
            }

            if (materials[i].HasProperty("_Smoothness"))
            {
                materials[i].SetFloat("_Smoothness", targetSmoothness);
            }
        }

        onComplete?.Invoke();
    }

    // --- RESET MATERIALS ON DESTROY ---
    private void OnDestroy()
    {
        if (materials == null || originalColors == null || originalSmoothness == null) return;
        if (materials.Count == 0 || materials.Count != originalColors.Count) return;

        for (int i = 0; i < materials.Count; i++)
        {
            if (materials[i] == null) continue;

            if (materials[i].HasProperty("_BaseColor"))
                materials[i].SetColor("_BaseColor", originalColors[i]);

            if (materials[i].HasProperty("_Smoothness"))
                materials[i].SetFloat("_Smoothness", originalSmoothness[i]);
        }
    }
}