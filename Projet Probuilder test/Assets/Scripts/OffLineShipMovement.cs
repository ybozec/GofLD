using UnityEngine;

using System.Collections.Generic;

public class OffLineShipMovement :MonoBehaviour {

    public GameObject SphereTemoin;
    public bool manetteControl = false;
    //reference to the changeGravity script
    private OffLineGravityPack changeGravityRef;
    private Transform cameraTransform;
    private AudioSource aS;
    public AudioClip footStep;
    OffLineGravityPack gP;

    public float m_SpeedHautBas = 2.0f;         // vitesse de deplacement avant/arriere
    public float m_SpeedGaucheDroite = 2.0f;
    public float m_X_Axis = 2.0f;              // vitesse d'inclinaison haut/bas
    public float m_Y_Axis = 2.0f;              // vitesse de rotation droite/gauche

    // rigidbody de reference 
    private Rigidbody m_Rigidbody;

    // reaction quand touche mur
    bool isTouchingWall=false;

    List<Vector3> lVecteurNormal = new List<Vector3>();

    Vector3 normaleMur;
    public float collisionCheckDistance;
    Vector3 directionToMove;
    //vector rotation de reference souris
    Vector3 sourisRotation;

    /* --- VARIABLE DES INPUTS --- */
    private float m_HorizontalInputValue;
    private float m_VerticalInputValue;
    private float m_MousseXInputValue;
    private float m_MousseYInputValue;

    float timer;

    public float speed = 10.0f;
    public float maxVelocityChange = 10.0f;


    void Start()
    {
        Cursor.visible = false;
        changeGravityRef = GetComponent<OffLineGravityPack>();
        cameraTransform = transform.Find("PlayerCamera");
        aS = GetComponent<AudioSource>();
        timer = Time.time;
        gP = GetComponent<OffLineGravityPack>();
        m_Rigidbody = GetComponent<Rigidbody>();
        if (manetteControl)
        {
            m_X_Axis = 130.0f;              // vitesse d'inclinaison haut/bas
            m_Y_Axis = 130.0f;
        }
    }

	
	// Update is called once per frame
	void Update () {
       
    }

    void FixedUpdate()
    {

        m_HorizontalInputValue = Input.GetAxis("Horizontal");
        m_VerticalInputValue = Input.GetAxis("Vertical");
        if (manetteControl)
        {
            m_MousseXInputValue = Input.GetAxis("RJoy_X");
           // Debug.Log("Joy X : "+ m_MousseXInputValue);
            m_MousseYInputValue = Input.GetAxis("RJoy_Y");
            //Debug.Log("Joy Y : " + m_MousseYInputValue);
        }
        else
        {
            m_MousseXInputValue = Input.GetAxis("Mouse X");
            m_MousseYInputValue = Input.GetAxis("Mouse Y");
        }

        if (!Input.GetKey(KeyCode.LeftShift))
        {
            MouvementsDroits();
        }
        if (!gP.isChangingGravity)
        {
            MouvementsRotations();

          
        }


    }

    //correspond aux deplacements du personnage
    void MouvementsDroits()
    {
        // premier vecteur vers l'avant
        /* Vector3 mvtHaut = transform.forward * m_VerticalInputValue * m_SpeedHautBas * Time.deltaTime;


          // second vecteur gauche droite
          Vector3 mvtDroite = transform.right * m_HorizontalInputValue * m_SpeedGaucheDroite * Time.deltaTime;

          if ( (mvtHaut.magnitude > 0.1 || mvtDroite.magnitude > 0.1) && gP.grounded) 
          {
              if  ((Time.time - timer) > 0.3)
              {
                  timer = Time.time;
                  float tmpPitch = Random.Range(0.0f, 0.2f);
                  aS.pitch = 0.8f + tmpPitch;
                  aS.volume = 0.5f;
                  aS.clip = footStep;
                  aS.Play();
              }
          }

            m_Rigidbody.MovePosition(m_Rigidbody.position + mvtHaut + mvtDroite);*/

        //Vector3 vertical = transform.forward * m_VerticalInputValue * m_SpeedHautBas * Time.deltaTime;
        // Vector3 horizontal = transform.right * m_HorizontalInputValue * m_SpeedGaucheDroite * Time.deltaTime;

        // Vector3 vertical = transform.forward * m_VerticalInputValue * m_SpeedHautBas * Time.deltaTime;
        // Vector3 horizontal = transform.right * m_HorizontalInputValue * m_SpeedGaucheDroite * Time.deltaTime;

        directionToMove = new Vector3(m_HorizontalInputValue, 0, m_VerticalInputValue);
        directionToMove = transform.TransformVector(directionToMove);
        directionToMove *= speed;
        if ((m_Rigidbody.velocity.magnitude > 0.1) && gP.grounded)
        {
            if ((Time.time - timer) > 0.3)
            {
                timer = Time.time;
                float tmpPitch = Random.Range(0.0f, 0.2f);
                aS.pitch = 0.8f + tmpPitch;
                aS.volume = 0.5f;
                aS.clip = footStep;
                aS.Play();
            }
        }

        

        //test si on peut aller par là
        /*RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToMove,  out hit, collisionCheckDistance))
        {
            normaleMur = hit.normal;
            float angle = Vector3.Angle(normaleMur, directionToMove);
            //Debug.Log("vecteur normal : "+normaleMur);
            
            
            if (angle > 90 && angle < 180) // si la direction
            {
                Vector3 nTmp = Vector3.Cross(normaleMur, directionToMove);
                angle = Vector3.Angle(transform.up, nTmp);
               // Debug.Log("angle avec la normale"+angle);
                if (angle < 90) //alors direction avant du mur
                {
                    directionToMove = Vector3.Cross(transform.up, normaleMur);
                    directionToMove *= speed;
                    //Debug.Log("vecteur direction to move :" + directionToMove);
                }
                else //direction arrière
                {
                    directionToMove = Vector3.Cross(normaleMur, transform.up);
                    directionToMove *= speed;
                }
            }
        }*/
        


        //Debug.Log(directionToMove);

        Vector3 rbVelocity = m_Rigidbody.velocity;
        // on retire la velocity dans le sens y du joueur
        rbVelocity = transform.InverseTransformVector(rbVelocity);
        rbVelocity.y = 0;
        rbVelocity = transform.TransformVector(rbVelocity);

        Vector3 changeVelocity = directionToMove - rbVelocity;
        changeVelocity = transform.InverseTransformVector(changeVelocity);
        changeVelocity.x = Mathf.Clamp(changeVelocity.x, -maxVelocityChange, maxVelocityChange);
        changeVelocity.z = Mathf.Clamp(changeVelocity.z, -maxVelocityChange, maxVelocityChange);
        changeVelocity.y = 0;
        changeVelocity = transform.TransformVector(changeVelocity);

        m_Rigidbody.AddForce(changeVelocity, ForceMode.Impulse);

    }


    // correspond aux depalcements de la camera
    void MouvementsRotations()
    {
        float sourisY = m_Y_Axis * m_MousseYInputValue * Time.deltaTime;
        float sourisX = m_X_Axis * m_MousseXInputValue * Time.deltaTime;
        transform.Rotate(Vector3.up, sourisX);
        cameraTransform.transform.Rotate(Vector3.right, -sourisY);
    }

    void initPlayer()
    {
       
    }

    void OnCollisionEnter(Collision collision)
    {

            
    }
    void OnCollisionStay(Collision collisionInfo)
    {
        // recherche des normales des murs qui touchent
        // le joueur excepté le sol
      /*  foreach (ContactPoint contact in collisionInfo.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal * 10, Color.white);
            if (contact.thisCollider.tag == "Player" && contact.otherCollider.tag == "WallMap")
            {
                if (!lVecteurNormal.Contains(contact.normal))
                {
                    lVecteurNormal.Add(contact.normal);
                }
            }
        }*/
    }
    void OnCollisionExit(Collision collision)
    {

    }

}
