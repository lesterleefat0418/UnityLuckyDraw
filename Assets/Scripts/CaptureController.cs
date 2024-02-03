using UnityEngine;
using System;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class CaptureController : CaptureManager
{
    public static CaptureController Instance = null;
    public int TimeOfDeleteLastDayFolder = 12;
    public bool isDeletedLastDayFolder = false;
    public bool triggeredDelete = false;
    public bool startCapturing =false;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        base.init();
    }

    // Update is called once per frame
    void Update()
    {
   
        if (DateTime.Now.Hour >= (int)TimeOfDeleteLastDayFolder && isDeletedLastDayFolder && !triggeredDelete)
        {
            ControlDeleteFolder(this.captureFormat.TempFolder);
            Debug.Log("Delete Last Day Images");
            triggeredDelete = true;
        }
    }

}

[Serializable]
public class CaptureManager:MonoBehaviour
{
    public CaptureFormat captureFormat;
    public Camera captureCamera;
    public CanvasScaler targetScaler;
    public OutputFormat outputFormat = OutputFormat.jpg;
    public RenderTexture renderTexture;
    private string filepath;
    private long photoFormat;
    private Texture2D finalCapture;

    public void init()
    {
        this.finalCapture = new Texture2D((int)targetScaler.referenceResolution.x, (int)targetScaler.referenceResolution.y, TextureFormat.RGB24, false, false);

        this.filepath = this.captureFormat.CaptureFolderPath("Images");
        if (!Directory.Exists(filepath))
            Directory.CreateDirectory(filepath);
    }

    public void resetTargetTexture()
    {
        if (captureCamera != null && renderTexture != null)
            captureCamera.targetTexture = renderTexture;
    }

    public enum OutputFormat
    {
        jpg,
        png
    }


    public Sprite capturedSprite
    {
        get
        {
            return this.captureFormat.Texture2DToSprite(this.finalCapture);
        }
    }

    public void captureImage(Action finished)
    {
        if (captureCamera != null && targetScaler != null)
        {
            this.captureCamera.targetTexture = null;
            Debug.Log("Capture!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            photoFormat = (long)(DateTime.UtcNow - epochStart).TotalSeconds;

            Debug.Log("Screen.height: " + Screen.height);
            Vector2 targetResolution = targetScaler.referenceResolution;
            Rect rect = new Rect(Vector2.zero, new Vector2(targetResolution.x, targetResolution.y));
            RenderTexture rt = new RenderTexture((int)targetResolution.x, (int)targetResolution.y, 24, RenderTextureFormat.ARGB32);
            captureCamera.targetTexture = rt;
            captureCamera.Render();
            RenderTexture.active = captureCamera.targetTexture;
            this.finalCapture.filterMode = FilterMode.Point;
            this.finalCapture.ReadPixels(rect, 0, 0);
            this.finalCapture.Apply();
            captureCamera.targetTexture = null;
            RenderTexture.active = null;

            SavePhotoToLocal(this.finalCapture);
            // Send image to web server
            if (finished != null) finished();
        }
    }


    private void SavePhotoToLocal(Texture2D screenShot)
    {
        string imagePath = "";
        // Loacl storage image
        if (Directory.Exists(filepath))
        {
            imagePath = filepath + "Cp_" + this.GenerateFileName + this.captureFormat.CaptureOutPutFormat;
            File.WriteAllBytes(imagePath, this.captureFormat.CaptureOutPutBytes(screenShot));
        }

    }

    public void ControlDeleteFolder(string Path)
    {
        DirectoryInfo dir = new DirectoryInfo(Path);
        DirectoryInfo[] folders = dir.GetDirectories();
        foreach (var folder in folders)
        {
            if (!this.captureFormat.DateFolderFormat.Equals(folder.Name))
            {
                Directory.Delete(dir + folder.Name, true);
            }
        }
    }

    public string GenerateFileName
    {
        get
        {
            // Get the current date and time
            DateTime now = DateTime.Now;
            string dateTimeString = "";
            if (LuckyDrawLogic.Instance != null)
                dateTimeString = LuckyDrawLogic.Instance.CurrentTime.ToString("yyyyMMddHHmmss");
            else
                dateTimeString = now.ToString("yyyyMMddHHmmss");

            // Append the photo ID to the date and time string
            string fileName = dateTimeString + "_" + photoFormat;

            return fileName;
        }
    }

}


[Serializable]
public class CaptureFormat
{
    public enum OutputFormat
    {
        jpg,
        png
    }

    public OutputFormat outputFormat = OutputFormat.jpg;
    public string TempFolder
    {
        get
        {
            return Directory.GetCurrentDirectory() + "/Capture/";
        }
    }

    public string CaptureFolderPath(string folderName)
    {
        return this.TempFolder + this.DateFolderFormat + "/" + folderName + "/";
    }

    public string DateFolderFormat
    {
        get
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }
    }

    public string LastDayFolderFormat
    {
        get
        {
            return DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
        }
    }

    public byte[] CaptureOutPutBytes(Texture2D texture)
    {
        byte[] bytes = null;
        switch (this.outputFormat)
        {
            case OutputFormat.jpg:
                bytes = texture.EncodeToJPG();
                break;
            case OutputFormat.png:
                bytes = texture.EncodeToPNG();
                break;
        }
        return bytes;
    }

    public string CaptureOutPutFormat
    {
        get
        {
            string format = "";
            switch (outputFormat)
            {
                case OutputFormat.jpg:
                    format = ".jpg";
                    break;
                case OutputFormat.png:
                    format = ".png";
                    break;
            }
            return format;
        }
    }

    private Rect ImageFormat(Texture2D tex)
    {
        return new Rect(0.0f, 0.0f, tex.width, tex.height);
    }

    public Sprite Texture2DToSprite(Texture2D image)
    {
        if (image != null)
            return Sprite.Create(image, ImageFormat(image), new Vector2(0.5f, 0.5f));
        else
            return null;
    }

    public void init()
    {
        if (!Directory.Exists(this.TempFolder))
            Directory.CreateDirectory(this.TempFolder);
    }
}

