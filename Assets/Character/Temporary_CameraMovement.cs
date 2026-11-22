using UnityEngine;
using UnityEngine.UIElements;

public class Temporary_CameraMovement : MonoBehaviour
{
// Source - https://stackoverflow.com/a/71234989
// Posted by Lemoncake123, modified by community. See post 'Timeline' for change history
// Retrieved 2026-11-13, License - CC BY-SA 4.0

    public Transform player;
    public Vector3 offset = new Vector3(0f, 0f, -8f);


    void Update()
    {
        //get the players position and add it with offset, then store it to transform.position aka the cameras position
        transform.position = player.position + offset;
    }
}


