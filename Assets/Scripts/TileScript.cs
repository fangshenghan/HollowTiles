using UnityEngine;

public class TileScript : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;

    private Color targetColor = Color.white;

    internal Vector2Int gridIndex;

    public void Awake()
    {
        targetColor = spriteRenderer.color;
    }

    public void Update()
    {
        if (spriteRenderer.color != targetColor)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime);
        }
    }

    public void SetTargetColor(Color color)
    {
        targetColor = color;
    }

}
