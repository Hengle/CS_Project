using UnityEngine;
using System.Collections;

public class BasicButton : MonoBehaviour {

    /// <summary>
    /// The camera this button is meant to be viewed from.
    /// Set this explicitly for best performance.\n
    /// The system will automatically traverse up the hierarchy to find a camera if this is not set.\n
    /// If nothing is found, it will fall back to the active <see cref="tk2dCamera"/>.\n
    /// Failing that, it will use Camera.main.
    /// </summary>
    public Camera ViewCamera;

    // Button Up = normal state
    // Button Down = held down
    // Button Pressed = after it is pressed and activated

    /// <summary>
    /// Audio clip to play when the button transitions from up to down state. Requires an AudioSource component to be attached to work.
    /// </summary>
    public AudioClip ButtonDownSound = null;
    /// <summary>
    /// Audio clip to play when the button transitions from down to up state. Requires an AudioSource component to be attached to work.
    /// </summary>
    public AudioClip ButtonUpSound = null;
    /// <summary>
    /// Audio clip to play when the button is pressed. Requires an AudioSource component to be attached to work.
    /// </summary>
    public AudioClip ButtonPressedSound = null;

    /// <summary>
    /// Button event handler delegate.
    /// </summary>
    public delegate void ButtonHandlerDelegate(BasicButton source);


    // Messaging

    /// <summary>
    /// Target object to send the message to. The event methods below are significantly more efficient.
    /// </summary>
    public GameObject TargetObject = null;
    /// <summary>
    /// The message to send to the object. This should be the name of the method which needs to be called.
    /// </summary>
    public string MessageName = "";

    /// <summary>
    /// Occurs when button is pressed (tapped, and finger lifted while inside the button)
    /// </summary>
    public event ButtonHandlerDelegate ButtonPressedEvent;

    /// <summary>
    /// Occurs every frame for as long as the button is held down.
    /// </summary>
    public event ButtonHandlerDelegate ButtonAutoFireEvent;
    /// <summary>
    /// Occurs when button transition from up to down state
    /// </summary>
    public event ButtonHandlerDelegate ButtonDownEvent;
    /// <summary>
    /// Occurs when button transitions from down to up state
    /// </summary>
    public event ButtonHandlerDelegate ButtonUpEvent;

//    tk2dBaseSprite sprite;
    bool _buttonDown = false;

    /// <summary>
    /// How much to scale the sprite when the button is in the down state
    /// </summary>
    public float TargetScale = 1.1f;
    /// <summary>
    /// The length of time the scale operation takes
    /// </summary>
    public float ScaleTime = 0.05f;
    /// <summary>
    /// How long to wait before allowing the button to be pressed again, in seconds.
    /// </summary>
    public float pressedWaitTime = 0.3f;

    void OnEnable()
    {
        _buttonDown = false;
    }

    private void Start()
    {
        if (ViewCamera == null)
        {
            // Find a camera parent 
            Transform node = transform;
            while (node && node.camera == null)
            {
                node = node.parent;
            }
            if (node && node.camera != null)
            {
                ViewCamera = node.camera;
            }

            // ...otherwise, use the main camera
            if (ViewCamera == null)
            {
                ViewCamera = Camera.main;
            }
        }

        if (collider == null)
        {
            BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
            Vector3 colliderSize = newCollider.size;
            colliderSize.z = 0.2f;
            newCollider.size = colliderSize;
        }

        if ((ButtonDownSound != null || ButtonPressedSound != null || ButtonUpSound != null) &&
            audio == null)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    // Modify this to suit your audio solution
    // In our case, we have a global audio manager to play one shot sounds and pool them
    void PlaySound(AudioClip source)
    {
        if (audio && source)
        {
            audio.PlayOneShot(source);
        }
    }

    IEnumerator coScale(Vector3 defaultScale, float startScale, float endScale)
    {
        float t0 = Time.realtimeSinceStartup;

        Vector3 scale = defaultScale;
        float s = 0.0f;
        while (s < ScaleTime)
        {
            float t = Mathf.Clamp01(s / ScaleTime);
            float scl = Mathf.Lerp(startScale, endScale, t);
            scale = defaultScale * scl;
            transform.localScale = scale;

            yield return 0;
            s = (Time.realtimeSinceStartup - t0);
        }

        transform.localScale = defaultScale * endScale;
    }


    IEnumerator LocalWaitForSeconds(float seconds)
    {
        float t0 = Time.realtimeSinceStartup;
        float s = 0.0f;
        while (s < seconds)
        {
            yield return 0;
            s = (Time.realtimeSinceStartup - t0);
        }
    }

    IEnumerator coHandleButtonPress(int fingerId)
    {
        _buttonDown = true; // inhibit processing in Update()
        bool buttonPressed = true; // the button is currently being pressed

        Vector3 defaultScale = transform.localScale;

        // Button has been pressed for the first time, cursor/finger is still on it
        if (TargetScale != 1.0f)
        {
            // Only do this when the scale is actually enabled, to save one frame of latency when not needed
            yield return StartCoroutine(coScale(defaultScale, 1.0f, TargetScale));
        }
        PlaySound(ButtonDownSound);

        if (ButtonDownEvent != null)
            ButtonDownEvent(this);

        while (true)
        {
            Vector3 cursorPosition = Vector3.zero;
            bool cursorActive = true;


            if (fingerId != -1)
            {
                bool found = false;
                for (int i = 0; i < Input.touchCount; ++i)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.fingerId == fingerId)
                    {
                        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                            break; // treat as not found
                        cursorPosition = touch.position;
                        found = true;
                    }
                }

                if (!found) cursorActive = false;
            }
            else
            {
                if (!Input.GetMouseButton(0))
                    cursorActive = false;
                cursorPosition = Input.mousePosition;
            }

            // user is no longer pressing mouse or no longer touching button
            if (!cursorActive)
                break;

            Ray ray = ViewCamera.ScreenPointToRay(cursorPosition);

            RaycastHit hitInfo;
            bool colliderHit = collider.Raycast(ray, out hitInfo, Mathf.Infinity);
            if (buttonPressed && !colliderHit)
            {
                if (TargetScale != 1.0f)
                {
                    // Finger is still on screen / button is still down, but the cursor has left the bounds of the button
                    yield return StartCoroutine(coScale(defaultScale, TargetScale, 1.0f));
                }
                PlaySound(ButtonUpSound);

                if (ButtonUpEvent != null)
                    ButtonUpEvent(this);

                buttonPressed = false;
            }
            else if (!buttonPressed & colliderHit)
            {
                if (TargetScale != 1.0f)
                {
                    // Cursor had left the bounds before, but now has come back in
                    yield return StartCoroutine(coScale(defaultScale, 1.0f, TargetScale));
                }
                PlaySound(ButtonDownSound);
           
                if (ButtonDownEvent != null)
                    ButtonDownEvent(this);

                buttonPressed = true;
            }

            if (buttonPressed && ButtonAutoFireEvent != null)
            {
                ButtonAutoFireEvent(this);
            }

            yield return 0;
        }

        if (buttonPressed)
        {
            if (TargetScale != 1.0f)
            {
                // Handle case when cursor was in bounds when the button was released / finger lifted
                yield return StartCoroutine(coScale(defaultScale, TargetScale, 1.0f));
            }
            PlaySound(ButtonPressedSound);

            if (TargetObject)
            {
                TargetObject.SendMessage(MessageName);
            }

            if (ButtonUpEvent != null)
                ButtonUpEvent(this);

            if (ButtonPressedEvent != null)
                ButtonPressedEvent(this);

            // Button may have been deactivated in ButtonPressed / Up event
            // Don't wait in that case

            if (gameObject.activeInHierarchy)
            {
                yield return StartCoroutine(LocalWaitForSeconds(pressedWaitTime));
            }

        }

        _buttonDown = false;
    }

    // Update is called once per frame
	void Update ()
	{
		if (_buttonDown) // only need to process if button isn't down
			return;

		bool detected = false;
		if (Input.multiTouchEnabled)
		{
			for (int i = 0; i < Input.touchCount; ++i)
			{
				Touch touch = Input.GetTouch(i);
				if (touch.phase != TouchPhase.Began) continue;
	            Ray ray = ViewCamera.ScreenPointToRay(touch.position);
	            RaycastHit hitInfo;
	            if (collider.Raycast(ray, out hitInfo, 1.0e8f))
	            {
					if (!Physics.Raycast(ray, hitInfo.distance - 0.01f))
					{
						StartCoroutine(coHandleButtonPress(touch.fingerId));
						detected = true;
						break; // only one finger on a buton, please.
					}
	            }	            
			}
		}
		if (!detected)
		{
			if (Input.GetMouseButtonDown(0))
	        {
	            Ray ray = ViewCamera.ScreenPointToRay(Input.mousePosition);
	            RaycastHit hitInfo;
	            if (collider.Raycast(ray, out hitInfo, 1.0e8f))
	            {
					if (!Physics.Raycast(ray, hitInfo.distance - 0.01f))
						StartCoroutine(coHandleButtonPress(-1));
	            }
	        }
		}
	}
}
