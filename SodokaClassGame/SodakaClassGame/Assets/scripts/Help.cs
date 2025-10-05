using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help : MonoBehaviour
{
    public GameObject uiPanel; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (uiPanel != null)
                uiPanel.SetActive(!uiPanel.activeSelf); 
        }
    }
}
