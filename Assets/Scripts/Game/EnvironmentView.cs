using UnityEngine;
public class EnvironmentView : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] Material waterMaterial;
    [SerializeField] Material underWaterMaterial;


    public void ShowPollutionLevel(float level)
    {
    }

    public void ShowBiodiversityLevel(float level)
    {
    }

    public void ShowWaterQualityLevel(float level)
    {
        waterMaterial?.SetFloat("_MudAmount", 1 - level);
        underWaterMaterial?.SetFloat("_MudAmount", 1 - level);
    }

}
