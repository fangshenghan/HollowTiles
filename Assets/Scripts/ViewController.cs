using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewController : MonoBehaviour
{

    public static ViewController instance;

    private static float nextMoveTime = 0, lastMoveTime = 0;
    private static bool moveDelayFlag = true, checkPatternFlag = false;

    public void Awake()
    {
        instance = this;
    }

    public void Update()
    {
        if (GameManagerScript.isPaused)
        {
            return;
        }

        Vector3 moveVector = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveVector += new Vector3(0f, 0.2f, 0f);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveVector += new Vector3(-0.2f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveVector += new Vector3(0f, -0.2f, 0f);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveVector += new Vector3(0.2f, 0f, 0f);
        }

        if (moveVector != Vector3.zero)
        {
            if (Time.time >= nextMoveTime)
            {
                transform.position += moveVector;

                lastMoveTime = Time.time;
                nextMoveTime = Time.time + 0.03f;

                if (moveDelayFlag)
                {
                    nextMoveTime += 0.22f;
                    moveDelayFlag = false;
                }

                checkPatternFlag = true;
            }
        }
        else
        {
            moveDelayFlag = true;
            nextMoveTime = 0;

            if (checkPatternFlag && Time.time > lastMoveTime + 0.75f)
            {
                checkPatternFlag = false;
                GameManagerScript.instance.CheckPatternAtViewPosition();
            }
        }
    }

}
