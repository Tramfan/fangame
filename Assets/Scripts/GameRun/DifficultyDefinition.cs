using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(
    fileName = "Difficulty",
    menuName = "Fangame/Difficulty Definition"
)]
public sealed class DifficultyDefinition :
    ScriptableObject
{
    [SerializeField]
    private GameDifficulty difficulty;

    [SerializeField]
    private LocalizedString displayName;

    public GameDifficulty Difficulty =>
        difficulty;

    public LocalizedString DisplayName =>
        displayName;
}