using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class UILevelCard : MonoBehaviour
{
    [Header("UI")]
    public Text title;           
    public Text description;     
    public Text coins;           
    public Text time;            
    public Image image;          
    public Button play;          
    public Image[] starsImages;
    public string scene { get; set; }
    protected bool m_locked;
    public bool locked
    {
        get { return m_locked; }
        set
        {
            m_locked = value;
            play.interactable = !m_locked; 
        }
    }


    protected virtual void Start()
    {
        play.onClick.AddListener(Play);
    }

    public virtual void Fill(GameLevel level)
    {
        if (level != null)
        {
            locked = level.locked;
            scene = level.scene;

            title.text = level.name;
            description.text = level.description;
            time.text = GameLevel.FormattedTime(level.time);
            coins.text = level.coins.ToString("000");
            image.sprite = level.image;
            for (int i = 0; i < starsImages.Length; i++)
            {
                starsImages[i].enabled = level.stars[i];
            }
        }
    }

    public virtual void Play()
    {
        GameLoader.instance.Load(scene); 
    }
}
