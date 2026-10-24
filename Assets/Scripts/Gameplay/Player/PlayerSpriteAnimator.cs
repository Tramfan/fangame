using UnityEngine;

[DefaultExecutionOrder(-400)]
[DisallowMultipleComponent]
public sealed class PlayerSpriteAnimator : MonoBehaviour
{
    [SerializeField]
    private PlayerInputSource inputSource;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField, Min(1)]
    private int idleFrameTicks = 6;

    [SerializeField, Min(1)]
    private int turnFrameTicks = 3;

    private CharacterDefinition character;
    private int idleFrame;
    private int idleTicks;
    private int turn;
    private int turnTicks;

    private void Awake()
    {
        if (inputSource == null)
        {
            inputSource =
                GetComponentInParent<PlayerInputSource>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (inputSource == null || spriteRenderer == null)
        {
            Debug.LogError(
                "Player Sprite Animator requires an " +
                "Input Source and Sprite Renderer.",
                this
            );

            enabled = false;
        }
    }

    private void OnEnable()
    {
        ResetAnimation();
    }

    public void Configure(
        CharacterDefinition newCharacter,
        SpriteRenderer newRenderer
    )
    {
        character = newCharacter;
        spriteRenderer = newRenderer;

        ResetAnimation();
    }

    private void ResetAnimation()
    {
        idleFrame = 0;
        idleTicks = 0;
        turn = 0;
        turnTicks = 0;

        ApplyFrame();
    }

    private void FixedUpdate()
    {
        if (character == null ||
            inputSource == null ||
            spriteRenderer == null)
        {
            return;
        }

        idleTicks++;

        if (idleTicks >= Mathf.Max(1, idleFrameTicks))
        {
            idleTicks = 0;

            idleFrame = (idleFrame + 1) %
                Mathf.Max(
                    1,
                    FrameCount(character.IdleSprites)
                );
        }

        float horizontal = inputSource.Movement.x;
        int desiredTurn = 0;

        if (horizontal < -0.01f)
        {
            desiredTurn =
                -FrameCount(character.TurnLeftSprites);
        }
        else if (horizontal > 0.01f)
        {
            desiredTurn =
                FrameCount(character.TurnRightSprites);
        }

        if (turn != desiredTurn)
        {
            turnTicks++;

            if (turnTicks >= Mathf.Max(1, turnFrameTicks))
            {
                turnTicks = 0;
                turn += desiredTurn > turn ? 1 : -1;
            }
        }
        else
        {
            turnTicks = 0;
        }

        ApplyFrame();
    }

    private void ApplyFrame()
    {
        if (character == null || spriteRenderer == null)
        {
            return;
        }

        Sprite[] frames = turn < 0
            ? character.TurnLeftSprites
            : turn > 0
                ? character.TurnRightSprites
                : character.IdleSprites;

        Sprite sprite = character.PlayerSprite;
        int count = FrameCount(frames);

        if (count > 0)
        {
            int index = turn == 0
                ? idleFrame % count
                : Mathf.Clamp(
                    Mathf.Abs(turn) - 1,
                    0,
                    count - 1
                );

            if (frames[index] != null)
            {
                sprite = frames[index];
            }
        }

        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    private static int FrameCount(Sprite[] frames)
    {
        return frames == null ? 0 : frames.Length;
    }
}