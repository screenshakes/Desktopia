using UnityEngine;

public class MoveCursor : MonoBehaviour
{
    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), -Input.GetAxis("Vertical"));
        if(input.magnitude > 0) Desktopia.Cursor.Move(input.normalized * Time.deltaTime * 500);

        if(Desktopia.Inputs.GetMouseButtonDown(0));
    }
}
