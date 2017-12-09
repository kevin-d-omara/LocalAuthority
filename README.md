# LocalAuthority


## Introduction
Unity does not allow sending `[Command]` or `[ClientRpc]` methods from non player-controlled game objects.
Local Authority makes this possible.

It is perfect for interactive games where objects, rather than a character, are being controlled.

Furthermore, multiplayer games must have client-side prediction, entity interpolation, and server reconciliation to be responsive and enjoyable.
These features are not built-in to Unity.
I'm experimenting with adding them to Local Authority.
Currently, I have basic client-side prediction working.

See Gabriel Gambetta's [blog series](http://www.gabrielgambetta.com/client-server-game-architecture.html) for an excellent overview of client-server game architecture.

*This project is still under active development*

## Features
- Send a Command or RPC method from **any** game object, not just the player.
  - Decouple your code and improve readability!
  - The server is still authoritative (all Command/RPCs pass through it) so cheating may be prevented.
- Attribute based, just like UNet. Tack `[MessageRpc]` or `[MessageCommand]` above your method and you're done!
  - Invoke it with `SendCallback(nameof(MyMethod))`.
- Client-side prediction is built-in, so you can enjoy a responsive game for no extra work!
  - Enable it in the attribute: `[MessageRpc(ClientSidePrediction = true)]`.

[![Demo Video](<https://user-images.githubusercontent.com/11803661/32900831-3c5396fa-caa3-11e7-9ff8-9f7a6ac52322.png>)](https://www.youtube.com/watch?v=owCHkt8GL-0)

## Examples
Example code is available in the `Assets/Scripts/Examples/` folder. The `Offline` and `Online` scenes use the sample code.

Here's a snippet to show the power of Local Authority:
```csharp
public class PlayingCard : LocalAuthorityBehaviour
{
    public Sprite frontImage;
    public Sprite backImage;
    public bool isShowingFront;

    public void OnMouseOver()
    {
        if (Input.GetButtonDown("Jump")
        {
            SendCallback(nameof(RpcFlipOver)); // Invoke the Rpc on all clients.
        }
    }

    [MessageRpc(ClientSidePrediction = true)] // Make this method an Rpc and enable client-side prediction.
    public void RpcFlipOver()
    {
        var imageToShow = isShowingFront ? frontImage : backImage;
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = imageToShow;
        isShowingFront = !isShowingFront;
    }
}
```
