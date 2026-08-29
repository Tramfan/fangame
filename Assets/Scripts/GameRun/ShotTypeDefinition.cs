using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(
    fileName = "ShotType",
    menuName = "Fangame/Shot Type Definition"
)]
public sealed class ShotTypeDefinition :
    ScriptableObject
{
    [SerializeField]
    private string id;

    [SerializeField]
    private LocalizedString displayName;

    public string Id => id;

    public LocalizedString DisplayName =>
        displayName;
}