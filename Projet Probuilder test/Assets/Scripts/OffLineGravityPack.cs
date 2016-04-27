using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class OffLineGravityPack : MonoBehaviour {
    public float intensiteGravite;
    public float saut;
    public bool grounded;
    public bool canDoubleJump = true;
    public float doubleJumpTimer;
    public bool isChangingGravity;
    float angleToTurn;
    public float degreeToTurn = 15;
    private char keyChangeGravity;

    private bool controlManette = false;

    // Ref Transform exterieur
    private Transform tCamera;

    //Timer pour controle touche
    private float keyTimer;
    public float freqKeyTimer = 0.5f;
    private float rotationTimer;
    public float freqRotationTimer = 0.02f;
    private Rigidbody joueur;

    // PARTIE AUDIO
    AudioSource aS;
    public AudioClip sonSautImpulsion;
    public AudioClip sonSautReception;
    public AudioClip sonDoubleSaut;
    // directional vector gravity the player want to change
    private Vector3 vDirectionChangeGravity;
    //directional vector for the rotation of the player body
    private Vector3 vRotationPlayer;
    private Vector3 vRotationCamera;
    // lookAt vector
    private Vector3 playerLookAt;

    /** GRAVITY **/
    // World Gravity References Vectors
    struct GravityVector{
        public string name;
        public Vector3 vector;

        public GravityVector(string s, Vector3 v)
        {
            name = s;
            vector = v;
        }
    }
    GravityVector[] tabGravityVector = new GravityVector[6];
    // Gravity vector tu use
    private Vector3 gravity;


    void Start()
    {
        keyTimer = Time.time;
        rotationTimer = keyTimer;
        doubleJumpTimer = keyTimer;
        grounded = false;
        joueur = GetComponent<Rigidbody>();
        isChangingGravity = false;
        angleToTurn = 0;
        keyChangeGravity = 'X';
        tCamera = transform.Find("PlayerCamera").transform;
        // init du tableau des vecteurs gravité
        tabGravityVector[0] = new GravityVector("RIGHT_WORLD", Vector3.right);
        tabGravityVector[1] = new GravityVector("LEFT_WORLD", Vector3.left);
        tabGravityVector[2] = new GravityVector("UP_WORLD", Vector3.up);
        tabGravityVector[3] = new GravityVector("DOWN_WORLD", Vector3.down);
        tabGravityVector[4] = new GravityVector("FORWARD_WORLD", Vector3.forward);
        tabGravityVector[5] = new GravityVector("BACK_WORLD", Vector3.back);

        // catch the player's directionnals points
        gravity = tabGravityVector[3].vector;

        aS = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
        clampAngleCam();
    }

    void FixedUpdate()
    {

        //changeGravity();
        if (isChangingGravity)
        {
            rotatePlayer();
        }
        else
        {
            // gravity (of Fame)
            joueur.AddForce(gravity * intensiteGravite, ForceMode.Force);
            changeGravity();

        }
        if (grounded && canDoubleJump)
        {
            if ((Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift)) || Input.GetButton("Jump"))
            {
                aS.PlayOneShot(sonSautImpulsion);
                joueur.AddForce(-gravity * saut, ForceMode.VelocityChange);
                grounded = false;
                doubleJumpTimer = Time.time;
            }
        }
        else if (!grounded && canDoubleJump && (Time.time - doubleJumpTimer) > 0.2f)
        {
            
            if ( (Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift)) || Input.GetButton("Jump"))
            {
                aS.PlayOneShot(sonDoubleSaut);
                joueur.AddForce(-gravity * saut, ForceMode.VelocityChange);
                if(joueur.velocity.magnitude > 4.0f)
                {
                    joueur.velocity = joueur.velocity.normalized * 2;
                } 
                Debug.Log(joueur.velocity);
                canDoubleJump = false;

            }
        }
       
    }

    // entrée en collision
    void OnCollisionEnter(Collision collision)
    {
        grounded = true;
        canDoubleJump = true;
    }
    // sortie de collision
    void OnCollisionExit(Collision collisionInfo)
    {

    }


    // find the vector tu use as gravity
    // depend on the player position
    void changeGravity()
    {
        // key Shift needed to be bind with Z,Q,S,D
        if ( (controlManette || Input.GetKey(KeyCode.LeftShift)) && !isChangingGravity && (Time.time - keyTimer) > freqKeyTimer)
        {
           
            if (Input.GetKey(KeyCode.D) || Input.GetButton("RightWall"))
            {
                vDirectionChangeGravity = transform.right;
                vDirectionChangeGravity.Normalize();
                vRotationPlayer = transform.forward;
                vRotationPlayer.Normalize();
                keyChangeGravity = 'D';
                vRotationCamera = transform.Find("PlayerCamera").transform.forward;
                //reset timer if gravity changed
                if (changeGravityVector())
                {
                    keyTimer = Time.time;
                    grounded = false;
                    rotatePlayer();
                }
            }else if (Input.GetKey(KeyCode.Q) || Input.GetButton("LeftWall"))
            {
                vDirectionChangeGravity = -transform.right;
                vDirectionChangeGravity.Normalize();
                vRotationPlayer = -transform.forward;
                vRotationPlayer.Normalize();
                keyChangeGravity = 'Q';
                vRotationCamera = -transform.Find("PlayerCamera").transform.forward;
                //reset timer if gravity changed
                if (changeGravityVector())
                {
                    keyTimer = Time.time;
                    grounded = false;
                    rotatePlayer();
                }
            }else if (Input.GetKey(KeyCode.Space) || Input.GetButton("FrontWall"))
            {
                vDirectionChangeGravity = transform.forward;
                vDirectionChangeGravity.Normalize();
                vRotationPlayer = -transform.right;
                vRotationPlayer.Normalize();
                keyChangeGravity = 'Z';
                //reset timer if gravity changed
                if (changeGravityVector())
                {
                    keyTimer = Time.time;
                    grounded = false;
                }
            }else if (Input.GetKey(KeyCode.S) || Input.GetButton("BackWall"))
            {
                vDirectionChangeGravity = -transform.forward;
                vDirectionChangeGravity.Normalize();
                vRotationPlayer = transform.right;
                vRotationPlayer.Normalize();
                keyChangeGravity = 'S';
                //reset timer if gravity changed
                if (changeGravityVector())
                {
                    keyTimer = Time.time;
                    grounded = false;
                }
            }

            if (Input.GetKey(KeyCode.Y))
            {
                Debug.Log(Vector3.Angle(transform.right, transform.Find("PlayerCamera").transform.right));

                keyTimer = Time.time;

            }
        }
    }

    // return if the gravity vector changed
    // the order of the line matter
    private bool changeGravityVector()
    {
        int iToGravity = -1;
        int iToRotation = -1;
        float tmp = 180;
        float tmp2 = tmp;
        bool changed = false;


            // trouve le plus petit angle pour la gravité
            for (int i = 0; i < 6; i++)
            {
                if (Vector3.Angle(vDirectionChangeGravity, tabGravityVector[i].vector) < tmp)
                {
                    iToGravity = i;
                    tmp = Vector3.Angle(vDirectionChangeGravity, tabGravityVector[i].vector);
                }
                if (Vector3.Angle(vRotationPlayer, tabGravityVector[i].vector) < tmp2)
                {
                    iToRotation = i;
                    tmp2 = Vector3.Angle(vRotationPlayer, tabGravityVector[i].vector);
                }
            }
            vRotationPlayer = tabGravityVector[iToRotation].vector;
            changed = tabGravityVector[iToGravity].vector != gravity;
            isChangingGravity = changed;
            gravity = tabGravityVector[iToGravity].vector;
  
       
           

        //Debug.Log("Vecteur gravité resultat : "+ tabGravityVector[iToGravity].name);
        //Debug.Log("Axe de rotation resultat : " + tabGravityVector[iToRotation].name);
        return changed;
    }

    void rotatePlayer()
    {
        if ((Time.time - rotationTimer) > freqRotationTimer)
        {
            rotationTimer = Time.time;
            angleToTurn += degreeToTurn;
            if (keyChangeGravity == 'Q' )
            {
                // rotation camera
                transform.RotateAround(transform.Find("PlayerCamera").transform.position, vRotationCamera, degreeToTurn);
                if (angleToTurn>=90)
                {
                    float angleT = Vector3.Angle(-transform.up, gravity);
                    if(angleT != 0)
                    {
                        Vector3 vT = Vector3.Cross(-transform.up, gravity);
                        Vector3 lK = transform.Find("PlayerCamera").transform.forward;
                        Vector3 pT = transform.Find("PlayerCamera").transform.position + 10000 * lK;
                        Quaternion rotation = Quaternion.LookRotation(lK, -gravity);
                        

                        //place le body pour etre a plat sur le nouveau sol
                        transform.RotateAround(transform.Find("PlayerCamera").transform.position, vT, angleT);

                        //deplace la camera pour le tracking cible
                        transform.Find("PlayerCamera").transform.LookAt(pT, transform.up);

                        //calcul la différence entre les axes body et cam selon l'axe UP (-gravity) 
                        angleT = Vector3.Angle(transform.right, transform.Find("PlayerCamera").transform.right);

                        if (Vector3.Angle(transform.forward, transform.Find("PlayerCamera").transform.right) > 90)
                        {
                            transform.RotateAround(transform.Find("PlayerCamera").transform.position, -gravity, angleT);
                            transform.Find("PlayerCamera").transform.RotateAround(transform.Find("PlayerCamera").transform.position, -gravity, -angleT);
                        }
                        else
                        {
                            transform.RotateAround(transform.Find("PlayerCamera").transform.position, -gravity, -angleT);
                            transform.Find("PlayerCamera").transform.RotateAround(transform.Find("PlayerCamera").transform.position, -gravity, angleT);
                        }



                        Debug.Log("angle plat body/cam  " + Vector3.Angle(transform.right, transform.Find("PlayerCamera").transform.right));
                        //transform.Find("PlayerCamera").transform.RotateAround(transform.Find("PlayerCamera").transform.position, transform.right, -angleT);
                    }

                }
            
            }
            else if (keyChangeGravity == 'D')
            {
                // rotation camera

                transform.RotateAround(transform.Find("PlayerCamera").transform.position, vRotationCamera, degreeToTurn);
                if (angleToTurn >= 90)
                {
                    float angleT = Vector3.Angle(-transform.up, gravity);
                    if(angleT != 0)
                    {
                        Vector3 vT = Vector3.Cross(-transform.up, gravity);
                        Vector3 lK = transform.Find("PlayerCamera").transform.forward;
                        Vector3 pT = transform.Find("PlayerCamera").transform.position + 10000 * lK;


                        //place le body pour etre a plat sur le nouveau sol
                        transform.RotateAround(transform.Find("PlayerCamera").transform.position, vT, angleT);
                       
                        //deplace la camera pour le tracking cible
                        transform.Find("PlayerCamera").transform.LookAt(pT, transform.up);

                        //calcul la différence entre les axes body et cam selon l'axe UP (-gravity) 
                        angleT = Vector3.Angle(transform.right, transform.Find("PlayerCamera").transform.right);

                        if(Vector3.Angle(transform.forward, transform.Find("PlayerCamera").transform.right) > 90)
                        {
                            transform.RotateAround(transform.Find("PlayerCamera").transform.position, -gravity, angleT);
                            transform.Find("PlayerCamera").transform.RotateAround(transform.Find("PlayerCamera").transform.position, -gravity, -angleT);
                        }
                        else
                        {
                            transform.RotateAround(transform.Find("PlayerCamera").transform.position, -gravity, -angleT);
                            transform.Find("PlayerCamera").transform.RotateAround(transform.Find("PlayerCamera").transform.position, -gravity, angleT);
                        }

                        

                        Debug.Log("angle plat body/cam  " + Vector3.Angle(transform.right, transform.Find("PlayerCamera").transform.right));
                        //transform.Find("PlayerCamera").transform.RotateAround(transform.Find("PlayerCamera").transform.position, transform.right, -angleT);
                    }
                }
            }
            else if (keyChangeGravity == 'Z')
            {
                // rotation camera
                transform.RotateAround(transform.Find("PlayerCamera").transform.position, vRotationPlayer, degreeToTurn);
                transform.Find("PlayerCamera").transform.RotateAround(transform.Find("PlayerCamera").transform.position, transform.right, degreeToTurn);

            }
            else
            {
                // rotation camera
                transform.RotateAround(transform.Find("PlayerCamera").transform.position, vRotationPlayer, degreeToTurn);
                transform.Find("PlayerCamera").transform.RotateAround(transform.Find("PlayerCamera").transform.position, -transform.right, degreeToTurn);

            }

            if (angleToTurn >= 90.0f)
            {
                isChangingGravity = false;
                angleToTurn = 0;
                //transform.Find("PlayerCamera").transform.Rotate(Vector3.right, angleT);
                //isGravityActive = true;

                //correctionBody();
            }


        }

    }

    // clamp camera angle
    private void clampAngleCam()
    {
        

        float angleCam = Vector3.Angle(transform.forward, tCamera.forward);
        float angleTopCam = Vector3.Angle(transform.up, tCamera.forward);

        if ( angleCam > 85.0f)
        {
            if (angleTopCam>85)
            {
                tCamera.RotateAround(tCamera.position, tCamera.right, 85.0f - angleCam);
            }
            else
            {
                tCamera.RotateAround(tCamera.position, tCamera.right, angleCam- 85.0f);
            }
        }
    }

    // calculation of the geometric angle in radian
    public float angleGeometrique(Vector3 vPlayer, Vector3 vDirection)
    {
        return (Mathf.Acos((Vector3.Dot(vPlayer, vDirection))));
    }

    // access to the gravity value
    /* public void activateGravityPack()
     {
         float x, y, z;

         if (gravity == tabGravityVector[0].vector)
         {
             //x = transform.rotation.x;
             //y = transform.rotation.y;
             transform.Rotate(Vector3.forward, 90);
         }
         if (gravity == tabGravityVector[3].vector)
         {
             //x = transform.rotation.x;
             //y = transform.rotation.y;
             transform.Rotate(Vector3.forward, 90);
         }
     }*/

    //convertion degree radian
    public float DegreeToRadian(float angle)
    {
        return Mathf.PI * angle / 180.0f;
    }

    public float RadianToDegree(float angle)
    {
        return 180 * angle / Mathf.PI;
    }

}


/*
Debug.Log("DIRECTION PLAYER" + vDirectionChangeGravity);
Debug.Log("RIGHT" + Vector3.Cross(vDirectionChangeGravity, RIGHT_WORLD));
Debug.Log("RIGHT ANGLE" + angleGeometrique(vDirectionChangeGravity, RIGHT_WORLD));
Debug.Log("LEFT" + Vector3.Cross(vDirectionChangeGravity, LEFT_WORLD));
Debug.Log("LEFT ANGLE" + angleGeometrique(vDirectionChangeGravity, LEFT_WORLD));
Debug.Log("UP" + Vector3.Cross(vDirectionChangeGravity, UP_WORLD));
Debug.Log("UP ANGLE" + angleGeometrique(vDirectionChangeGravity, UP_WORLD));
Debug.Log("DOWN" + Vector3.Cross(vDirectionChangeGravity, DOWN_WORLD));
Debug.Log("DOWN ANGLE" + angleGeometrique(vDirectionChangeGravity, DOWN_WORLD));
Debug.Log("FOWARD" + Vector3.Cross(vDirectionChangeGravity, FORWARD_WORLD));
Debug.Log("FOWARD ANGLE" + angleGeometrique(vDirectionChangeGravity, FORWARD_WORLD));
Debug.Log("BACK" + Vector3.Cross(vDirectionChangeGravity, BACK_WORLD));
Debug.Log("BACK ANGLE" + angleGeometrique(vDirectionChangeGravity, BACK_WORLD));
*/
