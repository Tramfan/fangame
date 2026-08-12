using System;
using UnityEngine;

[Flags]
public enum PlayerInputButtons : byte
{
    None = 0,
    Up = 1 << 0,
    Down = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3,
    Focus = 1 << 4,
    Shoot = 1 << 5,
    Shield = 1 << 6
}

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public sealed class PlayerInputSource : MonoBehaviour
{
    [Header("Keys")]
    [SerializeField]
    private KeyCode primaryFocusKey =
        KeyCode.LeftShift;

    [SerializeField]
    private KeyCode secondaryFocusKey =
        KeyCode.RightShift;

    [SerializeField]
    private KeyCode shootKey = KeyCode.Z;

    [SerializeField]
    private KeyCode shieldKey = KeyCode.X;

    public PlayerInputButtons CurrentButtons
    {
        get;
        private set;
    }

    public bool IsReplayInput
    {
        get;
        private set;
    }

    public Vector2 Movement
    {
        get
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (IsHeld(PlayerInputButtons.Left))
            {
                horizontal -= 1f;
            }

            if (IsHeld(PlayerInputButtons.Right))
            {
                horizontal += 1f;
            }

            if (IsHeld(PlayerInputButtons.Down))
            {
                vertical -= 1f;
            }

            if (IsHeld(PlayerInputButtons.Up))
            {
                vertical += 1f;
            }

            return Vector2.ClampMagnitude(
                new Vector2(horizontal, vertical),
                1f
            );
        }
    }

    public bool FocusHeld =>
        IsHeld(PlayerInputButtons.Focus);

    public bool ShootHeld =>
        IsHeld(PlayerInputButtons.Shoot);

    public bool ShieldHeld =>
        IsHeld(PlayerInputButtons.Shield);

    private void Update()
    {
        if (IsReplayInput)
        {
            return;
        }

        CurrentButtons = ReadLiveInput();
    }

    private void OnDisable()
    {
        CurrentButtons =
            PlayerInputButtons.None;
    }

    public bool IsHeld(
        PlayerInputButtons button
    )
    {
        return
            (CurrentButtons & button) != 0;
    }

    public void SetReplayInput(
        PlayerInputButtons buttons
    )
    {
        IsReplayInput = true;
        CurrentButtons = buttons;
    }

    public void StopReplayInput()
    {
        IsReplayInput = false;
        CurrentButtons =
            PlayerInputButtons.None;
    }

    private PlayerInputButtons ReadLiveInput()
    {
        PlayerInputButtons buttons =
            PlayerInputButtons.None;

        float horizontal =
            Input.GetAxisRaw("Horizontal");

        float vertical =
            Input.GetAxisRaw("Vertical");

        if (horizontal < 0f)
        {
            buttons |= PlayerInputButtons.Left;
        }
        else if (horizontal > 0f)
        {
            buttons |= PlayerInputButtons.Right;
        }

        if (vertical < 0f)
        {
            buttons |= PlayerInputButtons.Down;
        }
        else if (vertical > 0f)
        {
            buttons |= PlayerInputButtons.Up;
        }

        if (Input.GetKey(primaryFocusKey) ||
            Input.GetKey(secondaryFocusKey))
        {
            buttons |= PlayerInputButtons.Focus;
        }

        if (Input.GetKey(shootKey))
        {
            buttons |= PlayerInputButtons.Shoot;
        }

        if (Input.GetKey(shieldKey))
        {
            buttons |= PlayerInputButtons.Shield;
        }

        return buttons;
    }
}