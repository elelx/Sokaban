using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMusic : MonoBehaviour
{
    public AudioSource audioSource;  

    void Awake()
    {

        DontDestroyOnLoad(gameObject);


        if (FindObjectsOfType<BGMusic>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }


        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();


        audioSource.loop = true;
        audioSource.playOnAwake = true;


        if (!audioSource.isPlaying && audioSource.clip != null)
            audioSource.Play();
    }
}
