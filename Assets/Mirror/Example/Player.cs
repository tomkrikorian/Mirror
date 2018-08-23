using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    [SyncVar] int count = 0;
    public TextMesh tm;

    void Update()
    {
        Debug.Log("Update -> isServer=" + isServer);
        if (isServer)
        {
            ++count;
        }

        tm.text = count.ToString();
    }
}
