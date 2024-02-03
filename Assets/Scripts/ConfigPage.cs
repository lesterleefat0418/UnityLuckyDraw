using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfigPage : Singleton<ConfigPage>
{
    public ConfigData configData;
    public bool autoDebug = false;
    public bool dryrunMode = false;

    private void Awake()
    {
        this.LoadRecords();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (LuckyDrawLogic.Instance != null) { 
            LuckyDrawLogic.Instance.Init();
        }
        if (CSVManager.Instance != null) CSVManager.Instance.LoadCSVPathsData();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("Switch Scene");

        }*/
    }

    public void LoadRecords()
    {
        if (DataManager.Load() != null)
        {
            this.configData = DataManager.Load();
            //Debug.Log("Load config file: " + configData);
            if(CSVManager.Instance != null) { 
                CSVManager.Instance.luckyDrawPhraseFile = this.configData.luckyDrawPhraseFileName;
                CSVManager.Instance.recordCSVFolderName = this.configData.recordCSVFolderName;
            }


            this.changeScene(1);
        }
        else
        {
            Debug.Log("config file is empty and get data from inspector!");
            this.configData.luckyDrawPhraseFileName = "CSV/LuckyDraw-Phase1";
            this.configData.recordCSVFolderName = "Record";
            this.configData.startBtnIdling = 5;
            this.SaveRecords();
        }
    }

    public void SaveRecords()
    {
        DataManager.Save(this.configData);
        this.LoadRecords();
    }

    public void changeScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }


    private void OnDisable()
    {
        this.SaveRecords();
    }


    private void OnApplicationQuit()
    {
        this.SaveRecords();
    }
}


[System.Serializable]
public class ConfigData
{
    public string luckyDrawPhraseFileName;
    public string recordCSVFolderName;
    public float startBtnIdling = 5f;
}

public static class DataManager
{
    public static string directory = Directory.GetCurrentDirectory();
    public static string fileName = "/config.txt";
    public static void Save(ConfigData sData, bool dataMultipleLines = true)
    {
        string json = JsonUtility.ToJson(sData, dataMultipleLines);
        File.WriteAllText(directory + fileName, json);

        Debug.Log("Saved config file");
    }

    public static ConfigData Load()
    {
        string fullPath = directory + fileName;
        ConfigData loadData = new ConfigData();

        if (File.Exists(fullPath))
        {
            if (new FileInfo(fileName.Replace("/", "")).Length != 0)
            {
                string json = File.ReadAllText(fullPath);
                loadData = JsonUtility.FromJson<ConfigData>(json);
                return loadData;
            }
            else
            {
                Debug.Log("Empty File");
                return null;
            }
        }
        else
        {
            Debug.Log("Save File does not exist & create new One");
            var newFile = File.Create(fullPath);
            newFile.Close();
            return null;
        }
    }

}
