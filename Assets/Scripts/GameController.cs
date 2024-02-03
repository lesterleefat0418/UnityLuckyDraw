using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameController : MonoBehaviour
{
    public static GameController Instance = null;
    public AnimationDelay animationDelay;
    public bool allowDraw = false;
    public float allowDrawCountDown = 5f;
    public Zooming cameraZoom;
    public Transform ColliderParent;
    public Animator drawButton;
    public Button spinButton;
    public Animator Capsule_Top, Capsule_Bottom, Gift;
    public ColliderAutoForce[] balls;
    public GameObject celebration;

    public GameObject Blur;
    public GameObject Capsule;
    public CanvasGroup StartBtn, drawBtnHint, resultPage;
    public Image giftImage, celebrationText;
    public Image giftTitleImage;
    public ParticleSystem[] particles;
    public AudioManager audioManager;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (ConfigPage.Instance == null)
        {
            SceneManager.LoadScene(0);
            return;
        }
        else
        {
            this.allowDrawCountDown = ConfigPage.Instance.configData.startBtnIdling;
        }

        if (CSVManager.Instance != null) { 
            CSVManager.Instance.ResetData();
        }
        changeAnimation(0);

        if (ColliderParent != null)
            this.balls = ColliderParent.GetComponentsInChildren<ColliderAutoForce>();

    }

    void changeCapsuleLayer(int layerId)
    {
        if (Blur != null) Blur.SetActive(layerId==5 ? true:false);
        if (Capsule_Top != null) Capsule_Top.gameObject.layer = layerId;
        if (Capsule_Bottom != null) Capsule_Bottom.gameObject.layer = layerId;
        if (Gift != null) Gift.gameObject.layer = layerId;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(this.animationDelay.animationStage != AnimationDelay.AnimationStage.FrontPageIdling &&
           this.animationDelay.animationStage != AnimationDelay.AnimationStage.ShowResult) { 

            if(this.animationDelay.count > 0f)
            {
                this.animationDelay.count -= Time.deltaTime;
            }
            else
            {
                var nextPageId = (int)this.animationDelay.animationStage + 1;
                changeAnimation(nextPageId);
            }
        }


        if (allowDraw && this.animationDelay.animationStage == AnimationDelay.AnimationStage.FrontPageIdling && allowDrawCountDown != -1)
        {
            if(allowDrawCountDown > 0f)
            {
                allowDrawCountDown -= Time.deltaTime;
            }
            else
            {
                Debug.Log("Reset Idling");
                TriggerDrawBtnHint(false);
                AddForceToBall(false);
                if (ConfigPage.Instance != null)
                    this.allowDrawCountDown = ConfigPage.Instance.configData.startBtnIdling;
                else
                    this.allowDrawCountDown = 5f;

                allowDraw = false;
            }
        }

        /*if(this.animationDelay.animationStage == AnimationDelay.AnimationStage.ShowResult)
        {
            if (isHolding)
            {
                float holdDuration = Time.time - holdStartTime;
                if (holdDuration >= holdTime)
                {
                    Debug.Log("Button held for 5 seconds.");
                    this.Replay();
                }
            }
            else
            {

            }
        }*/


        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            if(this.animationDelay.animationStage == AnimationDelay.AnimationStage.FrontPageIdling)
                StartGame();
            else if(this.animationDelay.animationStage == AnimationDelay.AnimationStage.ShowResult)
                Replay();
        }
    }

    void TriggerDrawBtnHint(bool status)
    {
        if (StartBtn != null)
        {
            StartBtn.DOFade(status ? 0f:1f, 0.5f);
            StartBtn.interactable = status? false : true;
            StartBtn.blocksRaycasts = status ? false : true;
            StartBtn.GetComponent<Button>().enabled = status ? false : true;
        }

        if (drawBtnHint != null) { 
            drawBtnHint.DOFade(status? 1f: 0f, status? 0.5f: 0f);
        }

        if(spinButton != null)
        {
            spinButton.enabled = status? true:false;
        }
    }

    public void StartGame()
    {
        if(!allowDraw) {
            Debug.Log("Start Game");
            allowDraw = true;
            AddForceToBall(true);
            if(audioManager != null) audioManager.PlayStartButton();
            TriggerDrawBtnHint(true);
        }
    }

    public void Replay()
    {
        Debug.Log("Replay");
        SceneManager.LoadScene(1);
    }


    /*public bool isHolding = false;
    public float holdTime = 5f;
    public float holdStartTime = 0f;

    public void HoldToReplay()
    {
        isHolding = true;
        holdStartTime = Time.time;
        Debug.Log("isHolding: " + isHolding);
    }

    public void ReleaseReplay()
    {
        isHolding = false;
        Debug.Log("isHolding: " + isHolding);
    }*/


    public void showResultPage(bool status = false)
    {
        if (resultPage != null) { 
            resultPage.DOFade(status ? 1f: 0f, status ? 1f: 0f);
            resultPage.interactable = status;
            resultPage.blocksRaycasts = status;
        }
        if (status) StartCoroutine(capture(1f));
    }


    IEnumerator capture(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (CaptureController.Instance != null)
            CaptureController.Instance.captureImage(() => Debug.Log("Finished"));
    }

    void playParticle()
    {
        foreach (var part in particles)
        {
            if (part != null) { 
                part.DORewind(false);
                part.Play();
            }
        }
    }


    public void LuckyDraw()
    {
        if(allowDraw) { 
            Debug.Log("Start Draw");
            if (CSVManager.Instance != null) CSVManager.Instance.RandomGift();
            if (drawBtnHint != null) drawBtnHint.DOFade(0f, 0.5f);
            changeAnimation(1);
            if (audioManager != null) audioManager.PlaySpinning();
        }
    }

    void AddForceToBall(bool trigger=true)
    {
        for (int i = 0; i < balls.Length; i++)
        {
            if (balls[i] != null) {
                if (!trigger)
                {
                    balls[i].addForceTime = 0;
                }
                else
                {
                    balls[i].AddForce(Vector2.up, balls[i].min_force, balls[i].max_force);
                }
            }
        }
    }


    void changeAnimation(int toPageId)
    {
        this.animationDelay.setStage(toPageId);

        switch (this.animationDelay.animationStage)
        {
            case AnimationDelay.AnimationStage.FrontPageIdling:
                if (celebration != null) celebration.SetActive(false);
                if (celebrationText != null) celebrationText.enabled = true;
                if (Blur != null) Blur.SetActive(false);
                changeCapsuleLayer(3);
                showResultPage(false);               
                break;
            case AnimationDelay.AnimationStage.ButtonSpinning:
                if (drawButton != null) drawButton.SetTrigger("Draw");
                break;
            case AnimationDelay.AnimationStage.BallAddForced:
                allowDraw = false;
                break;
            case AnimationDelay.AnimationStage.CapsuleDropping:
                if (Capsule_Top != null) Capsule_Top.SetTrigger("Drop");
                if (Capsule_Bottom != null) Capsule_Bottom.SetTrigger("Drop");
                break;
            case AnimationDelay.AnimationStage.MachineZooming:
                if (CaptureController.Instance != null && CSVManager.Instance != null)
                {
                    var giftSprite = CaptureController.Instance.captureFormat.Texture2DToSprite(CSVManager.Instance.giftResult.GiftTexture);
                    var giftTitleSprite = CaptureController.Instance.captureFormat.Texture2DToSprite(CSVManager.Instance.giftResult.GiftTitleTexture);
                    if (Gift != null && giftSprite != null) Gift.transform.GetComponent<SpriteRenderer>().sprite = giftSprite;
                    if (giftImage != null && giftTitleSprite != null) giftImage.sprite = giftSprite;
                    if (giftTitleImage != null && giftTitleSprite != null) giftTitleImage.sprite = giftTitleSprite;
                }
                if (cameraZoom != null) cameraZoom.allowToZoom = true;
                if (Gift != null) Gift.SetTrigger("Zoom");
                break;
            case AnimationDelay.AnimationStage.CapsuleAnimating:
                changeCapsuleLayer(5);
                break;
            case AnimationDelay.AnimationStage.Celebration:
                if (audioManager != null) audioManager.PlayCongratulation();
                if (celebration != null) celebration.SetActive(true);
                playParticle();
                break;
            case AnimationDelay.AnimationStage.ShowResult:
                changeCapsuleLayer(3);
                showResultPage(true);
                playParticle();
                break;

        }
    }

}

[System.Serializable]
public class AnimationDelay {

    public AnimationStage animationStage = AnimationStage.FrontPageIdling;
    public StageDelay[] stageDelays;
    public float count = 0f;

    public enum AnimationStage
    {
        FrontPageIdling=0,
        ButtonSpinning=1,
        BallAddForced=2,
        CapsuleDropping=3,
        MachineZooming=4,
        CapsuleAnimating=5,
        Celebration=6,
        ShowResult=7
    }

    public void setStage(int stageId)
    {
        this.animationStage = (AnimationStage)stageId;
        this.count = stageDelay((AnimationStage)stageId);
    }


    public float stageDelay (AnimationStage stage)
    { 
        return this.stageDelays [(int)stage].delay;
    }

}

[System.Serializable]
public class StageDelay { 
    public string stage;
    public float delay;
}

