using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(FocusWindow))]
public class ScreenController : Singleton<ScreenController>
{
    public Vector2Int resolution;
    public bool mouseStatus = true;
    public bool enableFocusWindow = true;
    public bool showFPS = false;
    private FocusWindow focusWindow = null;
    [SerializeField] private float _hudRefreshRate = 0.1f;
    // Start is called before the first frame update

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        Screen.SetResolution(this.resolution.x, this.resolution.y, true);
        Cursor.visible = this.mouseStatus;
        focusWindow = GetComponent<FocusWindow>();
        focusWindow.isOn = this.enableFocusWindow;

        StartCoroutine(countFPS());
      
    }

    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.F1))
        {
            mouseStatus = !this.mouseStatus;
            Cursor.visible = mouseStatus;
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            this.enableFocusWindow = !this.enableFocusWindow;
            focusWindow.isOn = this.enableFocusWindow;
        }
        else if(Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Reset");
            SceneManager.LoadScene(1);
        }       
        else if (Input.GetKeyDown(KeyCode.F7) || Input.GetKeyDown(KeyCode.PageUp))
        {
            Debug.Log("DryrunMode");
            if (ConfigPage.Instance != null)
                ConfigPage.Instance.dryrunMode = !ConfigPage.Instance.dryrunMode;
            }
        else if (Input.GetKeyDown(KeyCode.F8))
        {
            Debug.Log("Auto Debug");
            if (ConfigPage.Instance != null)
                ConfigPage.Instance.autoDebug = !ConfigPage.Instance.autoDebug;
        }
    }


    private IEnumerator countFPS()
    {
        while (true)
        {
            this.FPS = 1f / Time.unscaledDeltaTime;
            yield return new WaitForSeconds(_hudRefreshRate);
        }
    }

    private float _fps;
    public float FPS
    {
        get
        {
            return _fps;
        }
        set
        {
            this._fps = value;
        }
    }

}