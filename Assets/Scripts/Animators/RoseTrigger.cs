﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoseTrigger : MonoBehaviour
{
    Animator roseAnim;

    void Start()
    {
        roseAnim = GetComponentInParent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(roseAnim);
        if (collision.tag == "Player")
        {
            roseAnim.SetBool("isTouched", true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            roseAnim.SetBool("isTouched", false);
        }
    }
}
