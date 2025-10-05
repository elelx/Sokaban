using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TitleChange : MonoBehaviour
{ 

    public Animator animator;              
    public string triggerToPlay = "Play";

    public string nextSceneName = "Level2"; 
    public float spaceLockSeconds = 10f;   

    bool canPress = false;
    bool started = false;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        Invoke(nameof(UnlockSpace), Mathf.Max(0f, spaceLockSeconds));
    }

    void UnlockSpace() => canPress = true;

    void Update()
    {
        if (!started && canPress && Input.GetKeyDown(KeyCode.Space))
        {
            started = true;                 // block extra presses
            if (!string.IsNullOrEmpty(triggerToPlay))
                animator.SetTrigger(triggerToPlay);
        }
    }


    public void AnimationFinished()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }


}
