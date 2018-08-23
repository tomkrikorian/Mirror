using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SyncVar] int count = 0;
    public TextMesh tm;

    void Update()
    {
        if (isServer)
        {
            ++count;
        }

        tm.text = count.ToString();
    }
}
