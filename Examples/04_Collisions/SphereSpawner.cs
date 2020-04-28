using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SphereSpawner : MonoBehaviour
{
    [SerializeField] GameObject sphere;
    List<GameObject> spheres;

    void Start()
    {
        Screen.SetResolution(Screen.resolutions[Screen.resolutions.Length - 1].width, Screen.resolutions[Screen.resolutions.Length - 1].height, true);
        Desktopia.Windows.Main.SetSize(Screen.resolutions[Screen.resolutions.Length - 1].width, Screen.resolutions[Screen.resolutions.Length - 1].height);
        Desktopia.Colliders.RecalculateScreenToWorld();
        spheres = new List<GameObject>();
        Desktopia.Inputs.AddOnKeyDown(KeyCode.Space, Spawn);
    }

    void Spawn()
    {
        spheres.Add(Instantiate(sphere, Desktopia.Colliders.ScreenToWorldPosition(Desktopia.Cursor.Position), Quaternion.identity));
    }
}
