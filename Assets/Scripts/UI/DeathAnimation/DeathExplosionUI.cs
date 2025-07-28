using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathExplosionUI : MonoBehaviour
{
    [Header("Prefab and Canvas")]
    public GameObject slicePrefab;
    public RectTransform canvasTransform;

    [Header("Slice Settings")]
    public int columns = 4;
    public int rows = 4;
    public float minSpeed = 50f;
    public float maxSpeed = 100f;

    public void Explode(Image sourceImage)
    {
        Sprite original = sourceImage.sprite;
        if (original == null) return;

        Vector2 originalSize = sourceImage.rectTransform.sizeDelta;
        Vector2 pieceSize = new Vector2(originalSize.x / columns, originalSize.y / rows);

        List<Sprite> slices = UISlicer.SliceSprite(original, columns, rows);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int index = y * columns + x;
                Vector2 offset = new Vector2(x * pieceSize.x, y * pieceSize.y) - originalSize / 2;

                GameObject piece = Instantiate(slicePrefab, sourceImage.transform.position + (Vector3)offset, Quaternion.identity, canvasTransform);
                piece.GetComponent<RectTransform>().sizeDelta = pieceSize;

                var script = piece.GetComponent<SlicePieceUI>();
                Vector2 dir = offset.normalized + Random.insideUnitCircle * 0.5f;
                float speed = Random.Range(minSpeed, maxSpeed);
                script.Initialize(slices[index], dir, speed);
            }
        }

        sourceImage.enabled = false;
    }
}
