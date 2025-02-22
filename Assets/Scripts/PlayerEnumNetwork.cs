using Fusion;
using UnityEngine;

public enum EInputButton
{
    Jump,
    Sprint,
}

public struct GameplayInput : INetworkInput
{
    public Vector2 LookRotation;
    public Vector2 MoveDirection;
    public NetworkButtons Buttons;
}
