``TODO: simplify.``

## LocalAuthority

Unity networking is Server Authoritative. This is the go-to client-server architecture to prevent cheating.

For a client to command the server, a game object must call a ``[Command]`` function. Suppose you are making
a card game, and you want any player to be able to flip over a card. You might create a CardController class like:

```C#
public CardController : NetworkBehaviour
{
    public Sprite frontImage;
    public Sprite backImage;
    public Sprite currentImage;

    public void OnMouseOver()
    {
        if (Input.GetButtonDown("Jump")
        {
            CmdFlipOver();  // Client commands the server to run CmdFlipOver().
        }
    }

    [Command]
    public void CmdFlipOver()
    {
        RpcFlipOver();  // Server commands the clients to run RpcFlipOver().
    }

    [ClientRpc]
    public void RpcFlipOver()
    {
        currentImage = currentImage == frontImage ? backImage : frontImage;
    }
}
```

However, a ``[Command]`` can only be called from scripts attached to a single game object, the "player" game object.
So now, you actually need:

```C#
// Attach to the player game object.
public Player : NetworkBehaviour
{
    public Player Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    
    [Command]
    public void CmdFlipOver(CardController card)
    {
        card.RpcFlipOver();  // Server commands the clients to run RpcFlipOver().
    }
}

// Attach to the card object.
public CardController : NetworkBehaviour
{
    public Sprite frontImage;
    public Sprite backImage;
    public Sprite currentImage;

    public void OnMouseOver()
    {
        if (Input.GetButtonDown("Jump")
        {
            Player.Instance.CmdFlipOver(this); // Must go through player script to run a [Command].
        }
    }

    [ClientRpc]
    public void RpcFlipOver()
    {
        currentImage = currentImage == frontImage ? backImage : frontImage;
    }
}
```


This is where **LocalAuthority** comes in. It enables any game object to command the server.
Now you can simplify to:

```C#
public CardController : LocalAuthorityBehaviour
{
    public Sprite frontImage;
    public Sprite backImage;
    public Sprite currentImage;

    public void OnMouseOver()
    {
        if (Input.GetButtonDown("Jump")
        {
            SendCommand<NetIdMessage>((short)MsgType.FlipOver); // Client commands the server to run CmdFlipOver().
        }
    }

    private static void CmdFlipOver(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<NetIdMessage>();
        var obj = FindLocalComponent<CardController>(msg.netId);
        
        Action action = () => obj.FlipOver();
        
        obj.RunNetworkAction(action, netMsg, msg, ignoreSender: true);
    }

    public void FlipOver()
    {
        currentImage = currentImage == frontImage ? backImage : frontImage;
    }
    
    protected override void RegisterCallbacks()
    {
        RegisterCallback((short)MsgType.FlipOver, CmdFlipOver, registerClient: true);
    }
}
```
