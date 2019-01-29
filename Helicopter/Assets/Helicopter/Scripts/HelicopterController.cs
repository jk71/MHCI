using UnityEngine;
using UnityEngine.UI;

public class HelicopterController : MonoBehaviour
{
    public AudioSource HelicopterSound;
    public AudioSource BulletSound;
    public ControlPanel ControlPanel;
    public Rigidbody HelicopterModel;
    public HeliRotorController MainRotorController;
    public HeliRotorController SubRotorController;

    public Quaternion controllerRotation;

    public GameObject projectile;
    public GameObject explosion;

    private Vector2 touchPos;

    public float TurnForce = 3f;
    public float ForwardForce = 10f;
    public float ForwardTiltForce = 20f;
    public float TurnTiltForce = 30f;
    public float EffectiveHeight = 100f;

    public float turnTiltForcePercent = 1.5f;
    public float turnForcePercent = 1.3f;

    private float _engineForce;
    public float EngineForce
    {
        get { return _engineForce; }
        set
        {
            MainRotorController.RotarSpeed = value * 80;
            SubRotorController.RotarSpeed = value * 40;
            HelicopterSound.pitch = Mathf.Clamp(value / 40, 0, 1.2f);
            if (UIGameController.runtime.EngineForceView != null)
                UIGameController.runtime.EngineForceView.text = string.Format("Engine value [ {0} ] ", (int)value);

            _engineForce = value;
        }
    }

    private Vector2 hMove = Vector2.zero;
    private Vector2 hTilt = Vector2.zero;
    private float hTurn = 0f;
    public bool IsOnGround = true;

    // Use this for initialization
	void Start ()
	{
        ControlPanel.KeyPressed += OnKeyPressed;

        Debug.Log("TESTCONTROLLERSTATUS: " + UnityEngine.XR.XRDevice.isPresent);
	}

  
    
	/*void Update () {


        //TESTING SECTION
        //#########################################################
        OVRInput.Update();

        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            EngineForce += 0.1f;
        }

        if (OVRInput.Get(OVRInput.Button.PrimaryTouchpad))
        {
            EngineForce -= 0.12f;
            if (EngineForce < 0) EngineForce = 0;
        }



        

        //############################################################

    }*/

    
    //public Transform controller;

    public static bool leftHanded { get; private set; }
    private Quaternion rotation = Quaternion.identity;

    void Awake()
    {
        #if UNITY_EDITOR
        leftHanded = false;        // (whichever you want to test here)
#else
        leftHanded = OVRInput.GetControllerPositionTracked(OVRInput.Controller.LTouch);
#endif
      
    }

    void Update()
    {
        
        OVRInput.Controller c = leftHanded ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        if (OVRInput.GetControllerPositionTracked(c))
        {
            //controller.localRotation = OVRInput.GetLocalControllerRotation(c);
            //controller.localPosition = OVRInput.GetLocalControllerPosition(c);

            //controllerRotation = controller.localRotation;

            
            rotation = OVRInput.GetLocalControllerRotation(c);
            //Debug.Log("ControllerRotation: " + rotation.ToString());
        }
    }
    
    void FixedUpdate()
    {

        if (UnityEngine.XR.XRDevice.isPresent)
        {
            JoyStickController();
        }
      
        LiftProcess();
        MoveProcess();
        TiltProcess();
        
    }

    private void MoveProcess()
    {
        var turn = TurnForce * Mathf.Lerp(hMove.x, hMove.x * (turnTiltForcePercent - Mathf.Abs(hMove.y)), Mathf.Max(0f, hMove.y));
        hTurn = Mathf.Lerp(hTurn, turn, Time.fixedDeltaTime * TurnForce);
        HelicopterModel.AddRelativeTorque(0f, hTurn * HelicopterModel.mass, 0f);
        HelicopterModel.AddRelativeForce(Vector3.forward * Mathf.Max(0f, hMove.y * ForwardForce * HelicopterModel.mass));
    }

    private void LiftProcess()
    {
        var upForce = 1 - Mathf.Clamp(HelicopterModel.transform.position.y / EffectiveHeight, 0, 1);
        upForce = Mathf.Lerp(0f, EngineForce, upForce) * HelicopterModel.mass;
        HelicopterModel.AddRelativeForce(Vector3.up * upForce);
    }

    private void TiltProcess()
    {
        hTilt.x = Mathf.Lerp(hTilt.x, hMove.x * TurnTiltForce, Time.deltaTime);
        hTilt.y = Mathf.Lerp(hTilt.y, hMove.y * ForwardTiltForce, Time.deltaTime);
        HelicopterModel.transform.localRotation = Quaternion.Euler(hTilt.y, HelicopterModel.transform.localEulerAngles.y, -hTilt.x);
    }

    private void OnKeyPressed(PressedKeyCode[] obj)
    {
        float tempY = 0;
        float tempX = 0;

        // stable forward
        if (hMove.y > 0)
            tempY = - Time.fixedDeltaTime;
        else
            if (hMove.y < 0)
                tempY = Time.fixedDeltaTime;

        // stable lurn
        if (hMove.x > 0)
            tempX = -Time.fixedDeltaTime;
        else
            if (hMove.x < 0)
                tempX = Time.fixedDeltaTime;
       

      
        
        foreach (var pressedKeyCode in obj)
        {
            switch (pressedKeyCode)
            {
                case PressedKeyCode.SpeedUpPressed:

                    EngineForce += 0.1f;
                    break;
                case PressedKeyCode.SpeedDownPressed:

                    EngineForce -= 0.12f;
                    if (EngineForce < 0) EngineForce = 0;
                    break;

                    case PressedKeyCode.ForwardPressed:

                    if (IsOnGround) break;
                    tempY = Time.fixedDeltaTime;
                    break;
                    case PressedKeyCode.BackPressed:

                    if (IsOnGround) break;
                    tempY = -Time.fixedDeltaTime;
                    break;
                    case PressedKeyCode.LeftPressed:

                    if (IsOnGround) break;
                    tempX = -Time.fixedDeltaTime;
                    break;
                    case PressedKeyCode.RightPressed:

                    if (IsOnGround) break;
                    tempX = Time.fixedDeltaTime;
                    break;
                    case PressedKeyCode.TurnRightPressed:
                    {
                        if (IsOnGround) break;
                        var force = (turnForcePercent - Mathf.Abs(hMove.y))*HelicopterModel.mass;
                        HelicopterModel.AddRelativeTorque(0f, force, 0);
                    }
                    break;
                    case PressedKeyCode.TurnLeftPressed:
                    {
                        if (IsOnGround) break;
                        
                        var force = -(turnForcePercent - Mathf.Abs(hMove.y))*HelicopterModel.mass;
                        HelicopterModel.AddRelativeTorque(0f, force, 0);
                    }
                    break;
                    
                    case PressedKeyCode.ShootPressed:
                    {
                        Shoot();
                    }
                    break;
                    
            }
        }

        hMove.x += tempX;
        hMove.x = Mathf.Clamp(hMove.x, -1, 1);

        hMove.y += tempY;
        hMove.y = Mathf.Clamp(hMove.y, -1, 1);

    }

    private void JoyStickController()
    {


        OVRInput.Update();

        
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            Shoot();
        }

    
        touchPos = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
       

       

        EngineForce = (touchPos.y + 1) * 25;

        Debug.Log("TouchPos: " + touchPos);
       // Debug.Log("Engine " + EngineForce);
       
        if(touchPos.x != 0f || touchPos.y != 0f)
        {
        //    Debug.Log("TouchPos: " + touchPos.ToString());
        }



        // Movement
        if (!IsOnGround)
        {
            hMove.x = rotation.y * 2;
            //hMove.x = Mathf.Clamp(hMove.x, -10, 10);

            hMove.y = rotation.x * 2 - 0.5f * (-Mathf.Sign(rotation.w));
            //hMove.y = Mathf.Clamp(hMove.y, -10, 10);
        }

    }

    private void Shoot()
    {

        GameObject bullet = Instantiate(projectile, GameObject.Find("BulletSpawn").transform.position, Quaternion.identity) as GameObject;
        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * 10000);

        GameObject rocket = Instantiate(projectile, GameObject.Find("RocketSpawn").transform.position, Quaternion.identity) as GameObject;
        rocket.GetComponent<Rigidbody>().AddForce(transform.forward * 10000);

        Instantiate(explosion, GameObject.Find("BulletSpawn").transform.position, Quaternion.identity);
        Instantiate(explosion, GameObject.Find("RocketSpawn").transform.position, Quaternion.identity);
        BulletSound.Play();

    }



    private void OnCollisionEnter()
    {
        IsOnGround = true;
    }

    private void OnCollisionExit()
    {
        IsOnGround = false;
    }
}