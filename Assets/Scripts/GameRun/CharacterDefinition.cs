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

    public string Id => id;

    public string CampaignId => campaignId;

    public LocalizedString DisplayName =>
        displayName;

    public Sprite Portrait => portrait;

    public ShotTypeDefinition[] ShotTypes =>
        shotTypes;
}