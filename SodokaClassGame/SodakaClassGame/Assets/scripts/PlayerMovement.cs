using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerMovement : MonoBehaviour
{
    //players coordinates on the grid
    private int x = 0;
    private int y = 0;


    //10 move b4 scrable

    public int scrambleAfterMoves = 10;
    private int movesLeft;

    private bool isReloading = false;

    //anims
    public Animator mouthAnimator;
    public string introAnim = "MouthOpen"; 
    public string outroAnim = "MouthClosed";


    private bool inputLocked = false;
    private bool levelEnding = false;


    private static bool introPlayed = false;


    public string nextSceneName;


    //playersprite
    public Sprite up;
    public Sprite down;
    public Sprite right;
    public Sprite left;

    SpriteRenderer sr;

    //audio

    public AudioSource audioSource;

    public AudioClip moveClip;        
    public AudioClip pushClip;
    

    public AudioClip reloadClip;

    public AudioClip introClip;       
    public AudioClip outroClip;



    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

        void Start()
    {

        var cell = GridController.instance.WorldToCell(transform.position);
        x = cell.x;
        y = cell.y;


        //put the obj at world pos tht matches grid x y
        

        transform.position = GridController.instance.GetWorldPos(x, y);

        movesLeft = scrambleAfterMoves;

        //  ResetScrambleTimer();


        if (!introPlayed)
        {
            inputLocked = true;
            mouthAnimator.Play(introAnim, 0, 0f);
        }
        else
        {
            inputLocked = false;  // skip intro if already played
        }


    }

        


        private void Update()
    {

        if (inputLocked) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (audioSource && moveClip)
                audioSource.PlayOneShot(moveClip, 0.4f);

            Debug.Log("beingPressed");
            Move(-1, 0);
            sr.sprite = left;

        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (audioSource && moveClip)
                audioSource.PlayOneShot(moveClip, 0.4f);

            Move(1, 0);
            sr.sprite = right;

        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (audioSource && moveClip)
                audioSource.PlayOneShot(moveClip, 0.4f);

            Move(0, 1);
            sr.sprite = up;

        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (audioSource && moveClip)
                audioSource.PlayOneShot(moveClip, 0.4f);

            Move(0, -1);
            sr.sprite = down;

        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            isReloading = true;

            if (audioSource && reloadClip)
                StartCoroutine(ReloadAfterSound());
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }



        if (!levelEnding && !isReloading && GridController.instance.AllGoalsCovered())
        {
            levelEnding = true;
            inputLocked = true;
            mouthAnimator.Play(outroAnim, 0, 0f);
        }

    }

    public IEnumerator ReloadAfterSound()
    {
        audioSource.PlayOneShot(reloadClip);
        yield return new WaitForSeconds(reloadClip.length);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }



    public void OnIntroDone()
    {
        Debug.Log("Intro finished → unlocking input");
        introPlayed = true;
        inputLocked = false;
    }

    public void OnOutroDone()
    {
 
            if (isReloading) return;


        SceneManager.LoadScene(nextSceneName);
    }




    //xmov, y mov r steps u wanna take **

    private void Move(int xmove, int ymove)
    {

        bool moved = false;

        //first, update grid cord to new block cell
        x += xmove;
        y += ymove;


        //ask grid what is at the next block, is it a tile? block? wall is it empty?
        var target = GridController.instance.GetTile(x, y);


        //depending on what the block is, the behavior is chosen
        switch (target)
        {
            case TileType.Empty:
                // move!

                //move obj to world space cuz its empty
                transform.position = GridController.instance.GetWorldPos(x, y);


                break;

            case TileType.Block:
                // push the block!

                var start = new Vector3Int(x, y, 0); //where the block is
                var destination = new Vector3Int(x + xmove, y + ymove, 0); //the cell after block in same direction **

                //if we can push....
                if (GridController.instance.CanPushBlock(start, destination))
                {
                    //push block forward
                    GridController.instance.PushBlock(start, destination);

                    // the player goes into old block place
                    transform.position = GridController.instance.GetWorldPos(x, y);

                    moved = true;

                    if (audioSource && pushClip)
                        audioSource.PlayOneShot(pushClip);
                }
                else
                {
                
                    x -= xmove;
                    y -= ymove;
                }
                //otherwise, dont move

                break;

            case TileType.Wall:
                //undo our move

                x -= xmove;
                y -= ymove;
                break;

            default:
                Debug.LogError($"unknown tile type {target.ToString()}. did you forget to update Move()?");
                break;


        }

        if (moved && !GridController.instance.AllGoalsCovered())
        {
            movesLeft-=1;

            if (movesLeft <= 0)
            {
                movesLeft = scrambleAfterMoves;
                GridController.instance.MoveAnyBlockToRandomRespawn();

            }
        }
    }







    //public void ResetScrambleTimer()
    //{
    //    CancelInvoke(nameof(Scramble));
    //    Invoke(nameof(Scramble), scrambleAfterMoves);
    //}

    //void Scramble()
    //{
    //    // optional: don’t run if already solved
    //    //  if (GridController.instance.AllGoalsCovered()) { ResetScrambleTimer(); return; }

    //    GridController.instance.AllGoalsCovered = true;
    //    if (mouthAnimator) mouthAnimator.Play("mouthOpen", 0, 0f);
    //    // No coroutine here; the Animation Events will do the work.
    //}


}



