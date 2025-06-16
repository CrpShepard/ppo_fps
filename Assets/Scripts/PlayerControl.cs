using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float sensitivity;
    public float xRotation, yRotation = 0f;
    Vector2 moveDirection;
    Vector2 mouseDelta;

    Player player;
    Rigidbody rb;
    Transform cameraTransform;

    [Header("InputAction")]
    public InputActionReference move;
    public InputActionReference moveCamera;
    public InputActionReference fire;
    public InputActionReference switchWeapon;
    public InputActionReference reload;

    private void OnEnable()
    {
        fire.action.started += Fire;
        switchWeapon.action.started += SwitchWeapon;
        reload.action.started += Reload;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        fire.action.started -= Fire;
        switchWeapon.action.started -= SwitchWeapon;
        reload.action.started -= Reload;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Fire(InputAction.CallbackContext context)
    {
        player.Attack();
    }

    void SwitchWeapon(InputAction.CallbackContext context)
    {
        var control = context.control.name;
        int index = int.Parse(control);
        player.ChangeWeapon(index);
    }

    void Reload(InputAction.CallbackContext context)
    {
        player.Reload();
    }

    public void OnSpawn()
    {
        cameraTransform.rotation = player.transform.rotation;
        xRotation = cameraTransform.rotation.eulerAngles.x;
        yRotation = cameraTransform.rotation.eulerAngles.y;
    }

    private void Awake()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        cameraTransform = GameObject.Find("PlayerCamera").transform;
    }
    
    private void LateUpdate()
    {
        cameraTransform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

        moveDirection = move.action.ReadValue<Vector2>();
        mouseDelta = moveCamera.action.ReadValue<Vector2>();

        xRotation = cameraTransform.rotation.eulerAngles.x - mouseDelta.y * sensitivity * Time.fixedDeltaTime;
        if (xRotation > 180f) xRotation -= 360f;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        yRotation = cameraTransform.rotation.eulerAngles.y + mouseDelta.x * sensitivity * Time.fixedDeltaTime;
        
        cameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    private void FixedUpdate()
    {
        rb.MoveRotation(Quaternion.Euler(0, yRotation, 0));

        if (player.canMove)
        {
            Vector3 moveForce = (transform.forward * moveDirection.y + transform.right * moveDirection.x).normalized * moveSpeed * 10f;
            rb.AddForce(moveForce, ForceMode.Force);
            if (rb.linearVelocity.magnitude > 10f) { rb.linearVelocity = rb.linearVelocity.normalized * 10f; }
        }
        else { rb.linearVelocity = Vector3.zero; }
    }
}
