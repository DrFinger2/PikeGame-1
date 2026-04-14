
using UnityEngine;

public class EnvironmentView : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private Renderer waterRenderer;
    [SerializeField] private Renderer[] underWaterRenderers;


    public void ShowPollutionLevel(float level)
    {
    }

    public void ShowBiodiversityLevel(float level)
    {
    }


    public void ShowWaterQualityLevel(float level)
    {
        float mudValue = 1.0f - level;

        if (waterRenderer != null)
        {
            waterRenderer.material.SetFloat("_MudAmount", mudValue);
        }

        if (underWaterRenderers != null)
        {
            foreach (Renderer ren in underWaterRenderers)
            {
                if (ren != null)
                {
                    ren.material.SetFloat("_MudAmount", mudValue);
                }
            }
        }
    }
    
    
}
