using System;
using UnityEngine;

[Serializable]
public struct DifficultyInt
{
    public int normal;
    public int hard;
    public int lunatic;

    public DifficultyInt(int all)
    {
        normal = all;
        hard = all;
        lunatic = all;
    }

    public DifficultyInt(int normal, int hard, int lunatic)
    {
        this.normal = normal;
        this.hard = hard;
        this.lunatic = lunatic;
    }

    public int Resolve(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Normal => normal,
            GameDifficulty.Hard => hard,
            GameDifficulty.Lunatic => lunatic,
            _ => hard
        };
    }

    public int ResolveMin(GameDifficulty difficulty, int minValue)
    {
        return Mathf.Max(minValue, Resolve(difficulty));
    }
}

[Serializable]
public struct DifficultyFloat
{
    public float normal;
    public float hard;
    public float lunatic;

    public DifficultyFloat(float all)
    {
        normal = all;
        hard = all;
        lunatic = all;
    }

    public DifficultyFloat(float normal, float hard, float lunatic)
    {
        this.normal = normal;
        this.hard = hard;
        this.lunatic = lunatic;
    }

    public float Resolve(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Normal => normal,
            GameDifficulty.Hard => hard,
            GameDifficulty.Lunatic => lunatic,
            _ => hard
        };
    }

    public float ResolveMin(GameDifficulty difficulty, float minValue)
    {
        return Mathf.Max(minValue, Resolve(difficulty));
    }

    public float ResolveClamped(
        GameDifficulty difficulty,
        float minValue,
        float maxValue
    )
    {
        return Mathf.Clamp(Resolve(difficulty), minValue, maxValue);
    }
}