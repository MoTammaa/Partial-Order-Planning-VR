using UnityEngine;
using System.Collections;

public class DemoController : MonoBehaviour
{

	private Animator animator;

	public float walkspeed = 5;
	private float horizontal;
	private float vertical;
	private float rotationDegreePerSecond = 1000;
	private bool isAttacking = false;

	public GameObject gamecam;
	public Vector2 camPosition;
	private bool dead;
    private bool isFlying;


	public GameObject[] characters;
	public int currentChar = 0;

    public GameObject[] targets;
    public float minAttackDistance;

    public UnityEngine.UI.Text nameText;


	void Start()
	{
		setCharacter(0);
        // FlyToPosition(new Vector3(30, 20, 45));
	}

    void FixedUpdate()
    {
        if (animator && !dead && !isFlying)
        {
            //walk
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            Vector3 stickDirection = new Vector3(horizontal, 0, vertical);
            float speedOut;

            if (stickDirection.sqrMagnitude > 1) stickDirection.Normalize();

            if (!isAttacking)
                speedOut = stickDirection.sqrMagnitude;
            else
                speedOut = 0;

            if (stickDirection != Vector3.zero && !isAttacking)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(stickDirection, Vector3.up), rotationDegreePerSecond * Time.deltaTime);
                GetComponent<Rigidbody>().velocity = transform.forward * speedOut * walkspeed + new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);
            }
            else
            {
                GetComponent<Rigidbody>().velocity = new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);
            }

            animator.SetFloat("Speed", speedOut);
        }
    }   

	void Update()
	{
		if (!dead)
		{
			// move camera
			if (gamecam)
				gamecam.transform.position = transform.position + new Vector3(0, camPosition.x, -camPosition.y);

			// attack
			if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump") && !isAttacking)
			{
				isAttacking = true;
				animator.SetTrigger("Attack");
				StartCoroutine(stopAttack(1));
                tryDamageTarget();


            }
            // get Hit
            if (Input.GetKeyDown(KeyCode.N) && !isAttacking)
            {
                isAttacking = true;
                animator.SetTrigger("Hit");
                StartCoroutine(stopAttack(1));
            }

            animator.SetBool("isAttacking", isAttacking);

			//switch character

			if (Input.GetKeyDown("left"))
			{
				setCharacter(-1);
				isAttacking = true;
				StartCoroutine(stopAttack(1f));
			}

			if (Input.GetKeyDown("right"))
			{
				setCharacter(1);
				isAttacking = true;
				StartCoroutine(stopAttack(1f));
			}

			// death
			if (Input.GetKeyDown("m"))
				StartCoroutine(selfdestruct());

            //Leave
            if (Input.GetKeyDown("l"))
            {
                if (this.ContainsParam(animator,"Leave"))
                {
                    animator.SetTrigger("Leave");
                    StartCoroutine(stopAttack(1f));
                }
            }
        }

	}
    GameObject target = null;
    public void tryDamageTarget()
    {
        target = null;
        float targetDistance = minAttackDistance + 1;
        foreach (var item in targets)
        {
            float itemDistance = (item.transform.position - transform.position).magnitude;
            if (itemDistance < minAttackDistance)
            {
                if (target == null) {
                    target = item;
                    targetDistance = itemDistance;
                }
                else if (itemDistance < targetDistance)
                {
                    target = item;
                    targetDistance = itemDistance;
                }
            }
        }
        if(target != null)
        {
            transform.LookAt(target.transform);
            
        }
    }
    public void DealDamage(DealDamageComponent comp)
    {
        if (target != null)
        {
            target.GetComponent<Animator>().SetTrigger("Hit");
            var hitFX = Instantiate<GameObject>(comp.hitFX);
            hitFX.transform.position = target.transform.position + new Vector3(0, target.GetComponentInChildren<SkinnedMeshRenderer>().bounds.center.y,0);
        }
    }

    public IEnumerator stopAttack(float length)
	{
		yield return new WaitForSeconds(length); 
		isAttacking = false;
	}

    public IEnumerator selfdestruct()
    {
        animator.SetTrigger("isDead");
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        dead = true;

        yield return new WaitForSeconds(3f);
        while (true)
        {
            if (Input.anyKeyDown)
            {
                Application.LoadLevel(Application.loadedLevelName);
                yield break;
            }
            else
                yield return 0;

        }
    }
    public void setCharacter(int i)
	{
		currentChar += i;

		if (currentChar > characters.Length - 1)
			currentChar = 0;
		if (currentChar < 0)
			currentChar = characters.Length - 1;

		foreach (GameObject child in characters)
		{
            if (child == characters[currentChar])
            {
                child.SetActive(true);
                if (nameText != null)
                    nameText.text = child.name;
            }
            else
            {
                child.SetActive(false);
            }
		}
		animator = GetComponentInChildren<Animator>();
    }

    public bool ContainsParam(Animator _Anim, string _ParamName)
    {
        foreach (AnimatorControllerParameter param in _Anim.parameters)
        {
            if (param.name == _ParamName) return true;
        }
        return false;
    }

    public void FlyToPosition(Vector3 position)
    {
        // gradually move to position
        StartCoroutine(flyToPosition(position));
    }

    public IEnumerator flyToPosition(Vector3 position)
    {
        isFlying = true;
        Rigidbody rb = GetComponent<Rigidbody>();
        RigidbodyConstraints originalConstraints = rb.constraints;

        rb.constraints = RigidbodyConstraints.FreezeAll;

        Quaternion originalRotation = transform.rotation; // Save the original rotation
        Quaternion targetRotation = Quaternion.LookRotation(position - transform.position); // Calculate the target rotation

        float time = 0;
        Vector3 startPosition = transform.position;
        while (time < 1)
        {
            time += Time.deltaTime / walkspeed / 4.0f;
            transform.position = Vector3.Lerp(startPosition, position, time);
            transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, time); // Interpolate the rotation
            yield return null;
        }
        // Set the rotation to the original value
        transform.rotation = targetRotation;
        rb.constraints = originalConstraints;
        isFlying = false;
    }

}
