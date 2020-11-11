using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class DragDrop : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.RawImage image;
    
    void Start()
    {
        Desktopia.Files.DragDrop.AddCallback(OnDragDrop);
    }

    void OnDragDrop(string[] files, Vector2 point)
    {
        var bytes = File.ReadAllBytes(files[0]);
        var texture = new Texture2D(2,2);
        texture.LoadImage(bytes);
        image.texture = texture;
    }
}
