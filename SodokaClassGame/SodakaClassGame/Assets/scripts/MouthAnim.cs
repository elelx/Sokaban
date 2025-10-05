using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouthAnim : MonoBehaviour
{
    public PlayerMovement player;

    public void IntroDone()
    {
        if (player) player.OnIntroDone();
    }

    public void OutroDone()
    {
        if (player) player.OnOutroDone();
    }

    public void PlayIntroAudio()
    {
        if (player && player.audioSource && player.introClip)
            player.audioSource.PlayOneShot(player.introClip);
    }

    
    public void PlayOutroAudio()
    {
        if (player && player.audioSource && player.outroClip)
            player.audioSource.PlayOneShot(player.outroClip);
    }
}
