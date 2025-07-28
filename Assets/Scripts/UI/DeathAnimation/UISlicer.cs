using System.Collections.Generic;
using UnityEngine;

public static class UISlicer
{
    public static List<Sprite> SliceSprite(Sprite original, int columns, int rows)
    {
        List<Sprite> slices = new();
        Texture2D texture = original.texture;

        int width = texture.width / columns;
        int height = texture.height / rows;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Rect rect = new(x * width, y * height, width, height);
                Sprite slice = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), original.pixelsPerUnit);
                slices.Add(slice);
            }
        }

        return slices;
    }
}
