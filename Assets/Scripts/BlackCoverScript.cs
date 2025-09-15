using Unity.VisualScripting;
using UnityEngine;

public class BlackCoverScript : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;

    private Color targetColor = Color.black.WithAlpha(240 / 255f);
    private Color startColor;
    private float transitionDuration = 1f;
    private float transitionTimer = 0f;
    private bool isTransitioning = false;

    public void Start()
    {
        startColor = spriteRenderer.color;
        SetTargetColor(Color.black.WithAlpha(240 / 255f));
    }

    public void Update()
    {
        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;

            float progress = Mathf.Clamp01(transitionTimer / transitionDuration);

            float easedProgress = EaseOutCubic(progress);

            spriteRenderer.color = Color.Lerp(startColor, targetColor, easedProgress);

            if (progress >= 1f)
            {
                isTransitioning = false;
                spriteRenderer.color = targetColor;
            }
        }
    }

    public void SetTargetColor(Color color)
    {
        startColor = spriteRenderer.color;
        targetColor = color;
        transitionTimer = 0f;
        isTransitioning = true;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}