using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameData 
{
    public int retries;
    public LevelData[] levels;
    public string createdAt;
    public string updatedAt;

    public static GameData FromJson(string json)
    {
        return JsonUtility.FromJson<GameData>(json);
    }

    public virtual string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public virtual int TotalStars()
    {
        return levels.Aggregate(0, (acc, level) =>
        {
            var total = level.CollectedStars();
            return acc + total;
        });
    }

    public virtual int TotalCoins()
    {
        return levels.Aggregate(0, (acc, level) => acc + level.coins);
    }

    public static GameData Create()
    {
        return new GameData()
        {
            retries = Game.instance.initialRetriesSet,
            createdAt = DateTime.UtcNow.ToString(),
            updatedAt = DateTime.UtcNow.ToString(),
            levels = Game.instance.levels.Select((level) =>
            {
                return new LevelData()
                {
                    locked = level.locked
                };
            }).ToArray()
        };
    }
}
