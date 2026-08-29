using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(
    fileName = "Character",
    menuName = "Fangame/Character Definition"
)]
public sealed class CharacterDefinition :
    ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private string campaignId = "main";

    [SerializeField]
    private LocalizedString displayName;

    [SerializeField]
    private Sprite portrait;

    [SerializeField]
    private ShotTypeDefinition[] shotTypes;

    [Header("Gameplay")]
    [SerializeField]
    private Sprite playerSprite;

    [SerializeField, Min(0f)]
    private float normalSpeed = 5f;

    [SerializeField, Min(0f)]
    private float focusSpeed = 2f;

    public string Id => id;

    public string CampaignId => campaignId;

    public LocalizedString DisplayName =>
        displayName;

    public Sprite Portrait => portrait;

    public ShotTypeDefinition[] ShotTypes =>
        shotTypes;

    public Sprite PlayerSprite =>
        playerSprite;

    public float NormalSpeed =>
        normalSpeed;

    public float FocusSpeed =>
        focusSpeed;
}