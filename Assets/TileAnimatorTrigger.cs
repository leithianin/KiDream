﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAnimatorTrigger : MonoBehaviour
{
    private Animator anim;
    BoxCollider2D Collider;
    public bool isChecked = false;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        Collider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && isChecked == false)
        {
            anim.SetBool("isTouched", true);
            isChecked = true;
            Destroy(Collider);
            Destroy(this);
        }
    }
}
