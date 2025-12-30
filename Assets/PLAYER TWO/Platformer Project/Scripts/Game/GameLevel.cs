using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameLevel 
{
    public bool locked;
    public string scene;
    public string name;
    public string description;
    public Sprite image;
    public int coins;
    public float time;
    public static readonly int StarsPerLevel = 3;

    public bool[] stars = new bool[StarsPerLevel];

    public virtual void LoadState(LevelData data)
    {
        locked = data.locked;  
        coins = data.coins;    
        time = data.time;      
        stars = data.stars;    
    }

    public virtual LevelData ToData()
    {
        return new LevelData()
        {
            locked = this.locked,
            coins = this.coins,
            time = this.time,
            stars = this.stars
        };
    }

    public static string FormattedTime(float time)
    {
        var minutes = Mathf.FloorToInt(time / 60f);
        var seconds = Mathf.FloorToInt(time % 60f);
        var milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        return minutes.ToString("0") + "'" + seconds.ToString("00") + "\"" + milliseconds.ToString("00");
    }
}
