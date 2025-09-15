using Unity.VisualScripting;
using UnityEngine;

public class WarningScript : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;

    private float startAlpha;
    private float fadeProgress = 0f;
    private float fadeSpeed = 1f;

    public void Awake()
    {
        startAlpha = spriteRenderer.color.a;
    }

    public void Update()
    {
        fadeProgress += Time.deltaTime * fadeSpeed;
        fadeProgress = Mathf.Clamp01(fadeProgress);

        float easedProgress = EaseInCubic(fadeProgress);
        float currentAlpha = Mathf.Lerp(startAlpha, 0f, easedProgress);

        Color color = spriteRenderer.color;
        color.a = currentAlpha;
        spriteRenderer.color = color;

        if (currentAlpha < 0.01f)
        {
            Destroy(gameObject);
        }
    }

    private float EaseInCubic(float t)
    {
        return t * t * t;
    }

}