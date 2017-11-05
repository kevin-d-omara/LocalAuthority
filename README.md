# LocalAuthority


## Introduction
Unity built-in networking has a number of drawbacks and limitations:
- `[Command]` and `[ClientRpc]` methods may only be invoked from code attached to the "player" game object.
- Client-side prediction is not supported, yet mandatory for a responsive multiplayer game.

**Local Authority** fixes these issues and improves flexibility.


## Features
- Send a Command or RPC method from **any** game object, not just the player.
  - Decouple your code and improve readability!
  - The server is still authoritative (all Command/RPCs pass through it) so cheating may be prevented.
- Client-side prediction is built-in (and optional), so you can enjoy a responsive, yet server-authoritative,
  game for no extra work!
- Command and RPCs are treated the same, except RPCs also execute on clients. There's no more need to wrap
  RPC calls in a Command.


## Examples

```csharp
public CardController : NetworkBehaviour
{
    public Sprite frontImage;
    public Sprite backImage;
    public Sprite currentImage;

    public void OnMouseOver()
    {
        if (Input.GetButtonDown("Jump")
        {
            InvokeRpc(nameof(FlipOver));
        }
    }

    [MessageRpc((short)MsgType.FlipOver, Predicted = true)]
    public void FlipOver()
    {
        currentImage = currentImage == frontImage ? backImage : frontImage;
        GetComponent<SpriteRenderer>().sprite = currentImage;
    }
}
```
