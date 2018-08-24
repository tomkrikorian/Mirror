using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SyncVar] int count = 0;
    public TextMesh tm;


    public override void OnStartServer()
    {
        Debug.Log("Hello from player 11");
        base.OnStartServer();
    }

    void Update()
    {
        if (isServer)
        {
            ++count;
        }

        tm.text = count.ToString();
    }
}
