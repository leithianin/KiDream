﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullEffect : MonoBehaviour
{
    CircleCollider2D colRadius;
    ReactionToWave parentBehaviour;
    ParticleSystem corruptedPullPS;

    public Rigidbody2D rb;

    ReactionToWave behaviour;

    public bool affectPlayer;

    [Header("Forces horizontales et verticales")]
    [Range(0, 1000), SerializeField]
    public float forceY;
    [Range(0, 1000), SerializeField]
    public float forceX;


    public bool xEqualY;

    public bool adaptRadius;

    [Header("Attraction complémentaire")]
    [Range(0, 1000), SerializeField]
    private float strongWaveBonus;
    [SerializeField]
    private float timeBeforeNewStrongWave;
    [SerializeField]
    private float strongWaveDuration;


    void Start()
    {
        InvokeRepeating("AttractionBonus", 0, timeBeforeNewStrongWave + strongWaveDuration);

        colRadius = this.GetComponent<CircleCollider2D>();
        parentBehaviour = this.GetComponentInParent<ReactionToWave>();
        corruptedPullPS = this.GetComponent<ParticleSystem>();
        if (adaptRadius == true)
        {
            colRadius.radius = parentBehaviour.corruptedPullRadius;
        }
    }

    void Update()
    {
        if (xEqualY == true)
        {
            forceX = forceY;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (collision.tag == "Player" && affectPlayer == true)
        {
            rb = collision.gameObject.GetComponent<Rigidbody2D>();
            rb.AddForce(new Vector2((this.transform.position.x - collision.transform.position.x) * forceX, (this.transform.position.y - collision.transform.position.y) * forceY));
        }

        else if (collision.tag == "ActionObject") //Tous les objets ayant ce tag doivent avoir un BoxCollider2D (Normal), un RigidBody2D (Dynamic + GravityScale à 0) et le script ReactionToWave.
        {
            if (collision.GetComponent<ReactionToWave>() != null)
            {
                behaviour = collision.GetComponent<ReactionToWave>();
            }
            else
            {
                return;
            }

            if (behaviour.canBePulled == true)
            {
                this.GetComponentInParent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

                rb = collision.gameObject.GetComponent<Rigidbody2D>();
                rb.AddForce(new Vector2((this.transform.position.x - collision.transform.position.x) * forceX, (this.transform.position.y - collision.transform.position.y) * forceY));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "ActionObject")
        {
            this.GetComponentInParent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private void AttractionBonus()
    {
        StartCoroutine("StrongWaves");
    }

    IEnumerator StrongWaves()
    {
        forceX = forceX + strongWaveBonus;
        forceY = forceY + strongWaveBonus;
        yield return new WaitForSeconds(strongWaveDuration);
        forceX = forceX - strongWaveBonus;
        forceY = forceY - strongWaveBonus;
    }

}
