using Unity.VisualScripting;
using UnityEngine;

public class BlackCoverScript : MonoBehaviour
{
   
    public SpriteRenderer spriteRenderer;

    private Color targetColor = Color.black.WithAlpha(240 / 255f);
    private Color startColor;
    private float transitionProgress = 1f;
    private float transitionSpeed = 0.5f;

    public void Start()
    {
        startColor = spriteRenderer.color;
    }

    public void Update()
    {
        if (transitionProgress < 1f)
        {
            transitionProgress += Time.deltaTime * transitionSpeed;
            transitionProgress = Mathf.Clamp01(transitionProgress);
            
            float easedProgress = EaseOutCubic(transitionProgress);
            spriteRenderer.color = Color.Lerp(startColor, targetColor, easedProgress);
        }
    }

    public void SetTargetColor(Color color)
    {
        if (targetColor != color)
        {
            startColor = spriteRenderer.color;
            targetColor = color;
            transitionProgress = 0f;
        }
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

}
