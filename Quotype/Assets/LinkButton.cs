using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnLinkClicked()
    {
        Application.OpenURL("https://gg-undroid-games.itch.io/");
    }
}
