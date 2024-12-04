using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler 
{
    public Action RotateMino;
    public Action MoveLeft;
    public Action MoveRight;
    public Action MoveDown;
    public Action Fall;
    public Action ChangeColor;
    public Action ResetMinoDataTable;
    public Action HoldMino;

    private bool fastFlag = false;
    private float timer;
    private float secondTimer = 0;
    private float maxTimer = 0.05f;
    private float waitTime = 0.15f;

    public void HandleInput()
    {
        timer += Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
        {
            secondTimer += Time.deltaTime;
            if(!fastFlag && timer > maxTimer)
            {
                MoveLeft?.Invoke();
                timer=0;
                fastFlag = true;
                secondTimer = 0;
            }
            else if(secondTimer > waitTime)
            {
                if (timer > maxTimer)
                {
                    timer = 0;
                    MoveLeft?.Invoke();
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            secondTimer = 0;
            fastFlag = false;
        }

        if (Input.GetKey(KeyCode.D))
        {
            secondTimer += Time.deltaTime;
            if(!fastFlag && timer > maxTimer)
            {
                MoveRight?.Invoke();
                timer=0;
                fastFlag = true;
                secondTimer = 0;
            }
            else if(secondTimer > waitTime)
            {
                if (timer > maxTimer)
                {
                    timer = 0;
                    MoveRight?.Invoke();
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            secondTimer = 0;
            fastFlag = false;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Fall?.Invoke();
        }
        if (Input.GetKey(KeyCode.S))
        {
            if (timer > maxTimer)
            {
                MoveDown?.Invoke();
                timer=0;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            HoldMino?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            //ToggleMinoVisibility();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ResetMinoDataTable?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RotateMino?.Invoke();
        }

        
    }
}
