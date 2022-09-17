using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    #if !UNITY_IOS || !UNITY_ANDROID
		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
    #endif

    #if !UNITY_IOS || !UNITY_ANDROID

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

    #endif

    
    [SerializeField]
    private PlayerUIController playerUIController;

    private PlayerControls playerControls;
    private bool moving = false;
    private Vector2 move;

    public bool cursorLock = true;

    private void Awake(){
        playerControls = new PlayerControls();
    }

    private void OnEnable(){
        playerControls.Enable();
    }

    private void OnDisable(){
        playerControls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {   
        SetCursorState(true);
        playerControls.UI.Disable();
        playerControls.Player.Move.performed += MoveS;
        playerControls.Player.Move.canceled += MoveC;
        playerControls.Player.Look.performed += Look;

        playerControls.Player.CreateSphere.performed += CreateSphere;
        playerControls.Player.DestroySphere.performed += DestroySphere;

        playerControls.Player.GetLocation.performed += GetLocation;

        playerControls.Player.ToggleMenu.performed += ToggleEscMenu;
        playerControls.UI.ToggleMenu.performed += ToggleEscMenuUi;
    }

    // Update is called once per frame
    void Update()
    {
        // if (cursorLock){
        //     Cursor.lockState = CursorLockMode.Locked
        // }
        if(moving){
            transform.position += transform.forward * move.y;
            transform.position += transform.right * move.x;
        }

        // Vector2 move = playerControls.Player.Move.ReadValue<Vector2>();
        // transform.position += transform.forward * move.y;
        // transform.position += transform.right * move.x;

        // Vector2 look = playerControls.Player.Look.ReadValue<Vector2>();
        
        // transform.localRotation = Quaternion.AngleAxis(look.x, Vector3.up);
        // transform.localRotation *= Quaternion.AngleAxis(look.y, Vector3.left);

        // float createSphere = playerControls.Player.CreateSphere.ReadValue<float>();

        // if (createSphere > 0){
        //     RaycastHit hit;
        //     if(Physics.Raycast(transform.position, transform.forward, out hit, 100)){
        //         EditTerrain.AddVoxelSphere(hit, 3f,new Voxel(),true);
        //     }
        // }

        // float destroySphere = playerControls.Player.DestroySphere.ReadValue<float>();

        // if (destroySphere > 0){
        //     RaycastHit hit;
        //     if(Physics.Raycast(transform.position, transform.forward, out hit, 100)){
        //         EditTerrain.AddVoxelSphere(hit, 3f,new Voxel(),false);
        //     }
        // }


    }

    private void MoveS(InputAction.CallbackContext context){
        move = context.ReadValue<Vector2>();
        moving = true;
    }

    private void MoveC(InputAction.CallbackContext context){
        moving = false;        
    }

    private void Look(InputAction.CallbackContext context){
        Vector2 look = context.ReadValue<Vector2>();
        transform.localRotation *= Quaternion.AngleAxis(look.x,  Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(look.y,  Vector3.left);
        
        
    }

    private void CreateSphere(InputAction.CallbackContext context){
        bool createSphere = context.ReadValueAsButton();
        if(createSphere){
            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.forward, out hit, 100)){
                EditTerrain.AddVoxelSphere(hit, 3f,new Voxel(),true);
            }
        }
    }

    private void DestroySphere(InputAction.CallbackContext context){
        bool destroySphere = context.ReadValueAsButton();
        if(destroySphere){
            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.forward, out hit, 100)){
                EditTerrain.AddVoxelSphere(hit, 3f,new Voxel(),false);
            }
        }        
    }

    private void GetLocation(InputAction.CallbackContext context){
        bool getLocation = context.ReadValueAsButton();
        if(getLocation){
            RaycastHit hit;
            if(Physics.Raycast(transform.position, transform.forward, out hit, 100)){
                EditTerrain.GetLocation(hit);
            }
        }
    }


    public void TogglePlayerUiControls(bool player){
        //Bool player is true to enable player controls and disable ui controls
        if(player){
            playerControls.Player.Enable();
            playerControls.UI.Disable();
            SetCursorState(true);
        }
        else{
            playerControls.Player.Disable();
            playerControls.UI.Enable();
            SetCursorState(false);
        }
    }

    private void ToggleEscMenu(InputAction.CallbackContext context){
        TogglePlayerUiControls(false);
        playerUIController.ToggleEscMenu();
    }

    private void ToggleEscMenuUi(InputAction.CallbackContext context){
        TogglePlayerUiControls(true);
        playerUIController.ToggleEscMenu();
    }
}
