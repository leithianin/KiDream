﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    Animator    animator  ;
    Rigidbody2D rigidBody ;
    GameMaster  gameMaster;

    //Collider2D[] hitResult = new Collider2D[10]; //Sert à l'attaque

    //Liste des objets qu'il y'a en Cauchemar et Reve
    GameObject[] cauchemarObjects;
    GameObject[] reveObjects     ;

    [Range(0,2)]
    public float worldTransitionDelay;

    public WaveManager waveManager;

    [Header("Shooting System")]
    public ParticleSystem myShooter         ;
    public Transform      myShooterTransform;
    public Transform      target            ;
    public Transform      targetRotator     ;

    [Range(0, 0.62f), SerializeField]
    private float joystickTolerance = 0f;

    [Header("Tilemaps")]
    public GameObject tilemapD;
    public GameObject tilemapN;
    public GameObject tileWallD;
    public GameObject tileWallN;

    [Header("Dialogue System")]
    public GameObject dialogueManagerObject;
    public GameObject dialogueTriggerObject;

    DialogueManager dialogueManager;
    DialogueTrigger dialogueTrigger;

    public bool dialogueHasStarted = false;

    [Header("Player Characteristics")]
    [SerializeField] float maxSpeed = 2f;
             private float moveX    = 0f;
             private float moveY    = 0f;

    public int hp = 3;

    private bool isKilled  = false;
    //private bool nightmare = false;  //Useless donc je l'ai viré
    public  bool isDream     =  true;

    private bool leftAxisInUse;

    private void Awake()
    {
        StartCoroutine(GoToDream());
    }

    void Start()
    {
        animator  = GetComponent<Animator   >();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (myShooter != waveManager.WaveShooters[waveManager.selectionIndex])
        {
            myShooter          = waveManager.WaveShooters         [waveManager.selectionIndex];
            myShooterTransform = waveManager.WaveShootersTransform[waveManager.selectionIndex];
        }

        moveX = Input.GetAxis("Horizontal");
        moveY = Input.GetAxis("Vertical"  );

        if(Input.GetAxisRaw("ChangeWorld") != 0 && isDream && leftAxisInUse == false)
        {
            StartCoroutine(GoToNightmare());
            isDream = false;
            leftAxisInUse = true;
        }
        else if(Input.GetAxisRaw("ChangeWorld") != 0 && !isDream && leftAxisInUse == false)
        {
            StartCoroutine(GoToDream());
            isDream = true;
            leftAxisInUse = true;
        }

        if(Input.GetAxisRaw("ChangeWorld") == 0)
        {
            leftAxisInUse = false;
        }

        if(Input.GetKeyDown(KeyCode.Joystick1Button0) && !dialogueHasStarted)
        {
            dialogueTrigger = dialogueTriggerObject.GetComponent<DialogueTrigger>();
            dialogueTrigger.TriggerDialogue();
            dialogueHasStarted = true;
        }
        else if (Input.GetKeyDown(KeyCode.Joystick1Button0) && dialogueHasStarted)
        {
            dialogueManager = dialogueManagerObject.GetComponent<DialogueManager>();
            dialogueManager.DisplayNextSentence();
        }

        Twist();
        RotationUpdater();
    }

    private void FixedUpdate()
    {
        rigidBody.velocity = new Vector2(maxSpeed * moveX, maxSpeed * moveY);

        IsMoving();
        //IsAttacking();
        Dead(hp);       
    }

    void IsMoving()
    {
        if ((moveX > 0 || moveX < 0) || 
            (moveY > 0 || moveY < 0))
        {
            animator.SetFloat("moveX", moveX);
            animator.SetFloat("moveY", moveY);

            animator.SetBool("isMoving", true);
        }
        else{animator.SetBool("isMoving", false);}
    }

    /**************************************
     * Permet d'aller en mode Cauchemar   *
     **************************************/
    IEnumerator GoToNightmare() //On pourrait enlver la première partie si on met tous les objets en enfant d'un "CeQuiApparaitEnReve" ou "CeQuiApparaitEnCauchemar"
    {
        yield return new WaitForSeconds(worldTransitionDelay);

        //GameObject.Find("Main Camera").GetComponent<Rippleeffect>().RippleEff(transform, 10f, 1f);
        tilemapD.GetComponent<TilemapRenderer>().enabled = false;
        tileWallD.GetComponent<TilemapRenderer>().enabled = false;
        tilemapN.GetComponent<TilemapRenderer>().enabled =  true;
        tileWallN.GetComponent<TilemapRenderer>().enabled = true;
        isDream = false;

        reveObjects = GameObject.FindGameObjectsWithTag("CeQuiApparaitEnReve");
        cauchemarObjects = GameObject.FindGameObjectsWithTag("CeQuiApparaitEnCauchemar");

        foreach (GameObject reveObject in reveObjects)  // Pour chaque object avec le tag "CeQuiApparaitEnReve", je désactive le spriteRenderer et active le isTrigger
        {
            if (reveObject.GetComponent<SpriteRenderer>() != null)
            {
                reveObject.GetComponent<SpriteRenderer>().enabled = false;
            }
            if (reveObject.GetComponent<BoxCollider2D>()  != null) // Faire une liste pour les exceptions (optimisation)
            {
                reveObject.GetComponent<BoxCollider2D>().enabled = false;
            }

            if(reveObject.transform.childCount > 0)
            {
                foreach(Transform child in reveObject.transform)
                {
                    if (child.GetComponent<SpriteRenderer>() != null)
                    {
                        child.GetComponent<SpriteRenderer>().enabled = false;
                    }
                    if (child.GetComponent<BoxCollider2D>() != null) // Faire une liste pour les exceptions (optimisation)
                    {
                        child.GetComponent<BoxCollider2D>().enabled = false;
                    }

                    if (child.transform.childCount > 0)
                    {
                        foreach (Transform childChild in child.transform)
                        {
                            if (childChild.GetComponent<SpriteRenderer>() != null)
                            {
                                childChild.GetComponent<SpriteRenderer>().enabled = false;
                            }
                            if (childChild.GetComponent<BoxCollider2D>() != null) // Faire une liste pour les exceptions (optimisation)
                            {
                                childChild.GetComponent<BoxCollider2D>().enabled = false;
                            }
                        }
                    }
                }
            }
        }

        foreach (GameObject cauchemarObject in cauchemarObjects) // Pour chaque object avec le tag "CeQuiApparaitEnCauchemar", j'active le spriteRenderer et desactive le isTrigger
        {
            if (cauchemarObject.GetComponent<SpriteRenderer>() != null)
            {
                cauchemarObject.GetComponent<SpriteRenderer>().enabled = true;
            }
            if (cauchemarObject.GetComponent<BoxCollider2D>() != null)
            {
                cauchemarObject.GetComponent<BoxCollider2D>().enabled = true;
            }

            if (cauchemarObject.transform.childCount > 0)
            {
                foreach (Transform child in cauchemarObject.transform)
                {
                    if (child.GetComponent<SpriteRenderer>() != null)
                    {
                        child.GetComponent<SpriteRenderer>().enabled = true;
                    }
                    if (child.GetComponent<BoxCollider2D>() != null)
                    {
                        child.GetComponent<BoxCollider2D>().enabled = true;
                    }

                    if (child.transform.childCount > 0)
                    {
                        foreach (Transform childChild in child.transform)
                        {
                            if (childChild.GetComponent<SpriteRenderer>() != null)
                            {
                                childChild.GetComponent<SpriteRenderer>().enabled = true;
                            }
                            if (childChild.GetComponent<BoxCollider2D>() != null) // Faire une liste pour les exceptions (optimisation)
                            {
                                childChild.GetComponent<BoxCollider2D>().enabled = true;
                            }
                        }
                    }
                }
            }
        }
    }

    /*********************************
     * Permet d'aller en mode Reve   *
     *********************************/
    IEnumerator GoToDream()
    {
        yield return new WaitForSeconds(worldTransitionDelay);

        //GameObject.Find("Main Camera").GetComponent<Rippleeffect>().RippleEff(transform, 10f, 1f);
        tilemapD.GetComponent<TilemapRenderer>().enabled = true;
        tileWallD.GetComponent<TilemapRenderer>().enabled = true;
        tilemapN.GetComponent<TilemapRenderer>().enabled = false;
        tileWallN.GetComponent<TilemapRenderer>().enabled = false;
        isDream = true;

        reveObjects = GameObject.FindGameObjectsWithTag("CeQuiApparaitEnReve");
        cauchemarObjects = GameObject.FindGameObjectsWithTag("CeQuiApparaitEnCauchemar");

        foreach (GameObject cauchemarObject in cauchemarObjects)
        {
            
            if (cauchemarObject.GetComponent<SpriteRenderer>() != null) // Pour chaque object avec le tag "CeQuiApparaitEnCauchemar", je desactive le spriteRenderer et active le isTrigger
            {
                cauchemarObject.GetComponent<SpriteRenderer>().enabled = false;
            }
            if (cauchemarObject.GetComponent<BoxCollider2D>() != null)
            {
                cauchemarObject.GetComponent<BoxCollider2D>().enabled = false;
            }

            if (cauchemarObject.transform.childCount > 0)
            {
                foreach (Transform child in cauchemarObject.transform)
                {
                    if (child.GetComponent<SpriteRenderer>() != null) // Pour chaque object avec le tag "CeQuiApparaitEnCauchemar", je desactive le spriteRenderer et active le isTrigger
                    {
                        child.GetComponent<SpriteRenderer>().enabled = false;
                    }
                    if (child.GetComponent<BoxCollider2D>() != null)
                    {
                        child.GetComponent<BoxCollider2D>().enabled = false;
                    }

                    if (child.transform.childCount > 0)
                    {
                        foreach (Transform childChild in child.transform)
                        {
                            if (childChild.GetComponent<SpriteRenderer>() != null)
                            {
                                childChild.GetComponent<SpriteRenderer>().enabled = false;
                            }
                            if (childChild.GetComponent<BoxCollider2D>() != null) // Faire une liste pour les exceptions (optimisation)
                            {
                                childChild.GetComponent<BoxCollider2D>().enabled = false;
                            }
                        }
                    }
                }
            }

        }

        foreach (GameObject reveObject in reveObjects) // Pour chaque object avec le tag "CeQuiApparaitEnReve", j'active le spriteRenderer et desactive le isTrigger
        {   
            if (reveObject.GetComponent<SpriteRenderer>() != null)
            {
                reveObject.GetComponent<SpriteRenderer>().enabled = true;
            }
            if (reveObject.GetComponent<BoxCollider2D>() != null )
            {
                reveObject.GetComponent<BoxCollider2D>().enabled = true;
            }

            if (reveObject.transform.childCount > 0)
            {
                foreach (Transform child in reveObject.transform)
                {
                    if (child.GetComponent<SpriteRenderer>() != null)
                    {
                        child.GetComponent<SpriteRenderer>().enabled = true;
                    }
                    if (child.GetComponent<BoxCollider2D>() != null)
                    {
                        child.GetComponent<BoxCollider2D>().enabled = true;
                    }

                    if (child.transform.childCount > 0)
                    {
                        foreach (Transform childChild in child.transform)
                        {
                            if (childChild.GetComponent<SpriteRenderer>() != null)
                            {
                                childChild.GetComponent<SpriteRenderer>().enabled = true;
                            }
                            if (childChild.GetComponent<BoxCollider2D>() != null) // Faire une liste pour les exceptions (optimisation)
                            {
                                childChild.GetComponent<BoxCollider2D>().enabled = true;
                            }
                        }
                    }
                }
            }
        }
    }

    /*********************************************
     * Condition de Mort (Animation a rajouter)  *
     *********************************************/
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //gameMaster = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
        if (collision.tag == "DeathZone")
        {
            transform.position = gameMaster.lastCheckpointPos;
            isKilled = true;
        }
    }

    public void damage()
    {
        hp--;
        Debug.Log(hp);
    }

    void Dead(int hp)
    {
        if(hp <= 0)
        {
            //transform.position = gameMaster.lastCheckpointPos;
            Destroy(gameObject);
            isKilled = true;
        }
    }

      void Twist()
    {
        float h1 = Input.GetAxis("HorizontalRight");
        float v1 = Input.GetAxis("VerticalRight");

        //Debug.Log("y = " + v1 + " et x = " + h1);

        if (h1 > joystickTolerance || h1 < -joystickTolerance || v1 > joystickTolerance || v1 < -joystickTolerance)
        {
            targetRotator.transform.localEulerAngles = new Vector3(0f, 0f, -(Mathf.Atan2(h1, v1) * 180 / Mathf.PI)); // this does the actual rotaion according to inputs
        }

        ////////////// NE PAS SUPPRIMER - Permet de rotate back à la position initiale;
        /*else
        {
            Vector3 curRot = targetRotator.transform.localEulerAngles;
            Vector3 homeRot;
            if (curRot.z > 180f)
            {
                homeRot = new Vector3(0f, 0f, 359.999f);
            }
            else
            {
                homeRot = Vector3.zero;
            }
            targetRotator.transform.localEulerAngles = Vector3.Slerp(curRot, homeRot, Time.deltaTime * 2);
        }*/
        //////////////
    }

    public void UseWave()
    {
        Vector2 rotationVector = new Vector2(target.position.x - myShooterTransform.position.x, target.position.y - myShooterTransform.position.y);
        float angleValue = Mathf.Atan2(rotationVector.normalized.y, rotationVector.normalized.x) * Mathf.Rad2Deg;

        ParticleSystem.ShapeModule wpshape = myShooter.shape;
        wpshape.rotation = new Vector3(0, 0, angleValue);

        myShooter.Emit(1);
    }


    private void RotationUpdater()
    {
        waveManager.WaveShooters[waveManager.selectionIndex].startRotation = -targetRotator.localRotation.ToEulerAngles().z;
    }

    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "DeathZone")
        {
            Destroy(gameObject,0.2f); //Enlever le delai lorsque que y'aura l'animation
        }
    }*/
}
