using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Interaction Settings")]
    [SerializeField] private float raycastDistance = 2f;

    private Vector2 inputVec;
    private Vector2 RaycastVector;
    private GameObject scanObject;
    private Rigidbody2D rigid;
    private Animator anim;

    public DialogueManager dialogueManager;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if(!dialogueManager.isInteraction)
            HandleInput();
        Interaction();
    }

    private void FixedUpdate()
    {
        if (!dialogueManager.isInteraction)
        {
            MovePlayer();
            ScanForObjects();
        }
    }

    // 🕹️ 입력 처리
    private void HandleInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 수직 입력 우선
        if (v != 0)
        {
            inputVec = new Vector2(0, v);
            RaycastVector = inputVec;
        }
        // 수평 입력 처리
        else if (h != 0)
        {
            inputVec = new Vector2(h, 0);
            RaycastVector = inputVec;
        }
        else
        {
            inputVec = Vector2.zero;
        }

        if (anim.GetInteger("h") != h)
        {
            anim.SetBool("isChange", true);
            anim.SetInteger("h", (int)h);
        }
        else if (anim.GetInteger("v") != v)
        {
            anim.SetBool("isChange", true);
            anim.SetInteger("v", (int)v);
        }
        else
        {
            anim.SetBool("isChange", false);
        }
    }

    // 🚶 이동 처리
    private void MovePlayer()
    {
        rigid.velocity = inputVec * moveSpeed;
    }

    // 🛠️ 상호작용 처리
    private void Interaction()
    {
        if (Input.GetKeyDown(KeyCode.Space) && scanObject != null)
        {
            dialogueManager.Interaction(scanObject);
        }
    }

    // 🔦 레이캐스트 탐지
    private void ScanForObjects()
    {
        Debug.DrawRay(rigid.position, RaycastVector * raycastDistance, Color.green);
        RaycastHit2D hit = Physics2D.Raycast(rigid.position, RaycastVector, raycastDistance, LayerMask.GetMask("Interactable"));

        if (hit.collider != null) scanObject = hit.collider.gameObject;
        else
        {
            scanObject = null;
        }
    }
}