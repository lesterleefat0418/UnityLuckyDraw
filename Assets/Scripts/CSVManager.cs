using System.IO;
using UnityEngine;
using System.Text;
using System.Linq;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;

public class CSVManager : Singleton<CSVManager>
{
    [HideInInspector]
    public string luckyDrawRecord;
    public string luckyDrawFile;
    public string luckyDrawPhraseFile;
    public string recordCSVFolderName;
    public HeaderName[] giftCSVHeader;
    public HeaderName[] giftRecordHeader;
    public PrizeData[] filterData;
    public GiftResult giftResult;
    public GiftRecord giftRecord;
    public bool disableRandomGift;


    public void LoadCSVPathsData()
    {
        this.luckyDrawRecord = ReadFileAtLocation(getLuckyDrawRecordPath, true, this.giftRecordHeader);
        this.luckyDrawFile = ReadFileAtLocation(getLuckyDrawCSVPath, false);

        if (LuckyDrawLogic.Instance != null)
        {
            LuckyDrawLogic.Instance.LoadLuckyDrawCSVClass(this.luckyDrawFile);
            List<GiftRecord> totalRecords = LuckyDrawLogic.Instance.TotalOfLuckyDrawRecords(this.luckyDrawRecord);
            if (totalRecords.Count > 0)
                LuckyDrawLogic.Instance.TotalOfDrawTime = totalRecords.Count;
            else
                LuckyDrawLogic.Instance.TotalOfDrawTime = 0;
        }
        
        Debug.Log("Loaded CSV Data!!");
    }

    public void ResetData()
    {
        this.giftResult.reset();
        this.giftRecord.reset();
        if(LuckyDrawLogic.Instance != null) LuckyDrawLogic.Instance.ResetData();
        Debug.Log("Reset data");
    }

    public void RandomGift()
    {
        if(disableRandomGift)
            return;

        if (LuckyDrawLogic.Instance != null) { 
            DateTime currentDateTime = DateTime.Now;
            string dateFormat = LuckyDrawLogic.Instance.dateStringFormat;
            LuckyDrawLogic.Instance.CurrentTime = currentDateTime;
            LuckyDrawLogic.Instance.CurrentDateTime = currentDateTime.ToString(dateFormat);  
            this.filterData = LuckyDrawLogic.Instance.GiftsMatchCurrentDateTime(currentDateTime);

            if (this.filterData != null)
            {
                if (this.filterData.Length > 0)
                    getRandomGiftData();
            }
            else
            {
                LuckyDrawLogic.Instance.GiftResultString = "No any gift lists included, Show Default Gift Image && Title!";
                if (GameController.Instance.celebrationText != null) 
                    GameController.Instance.celebrationText.enabled = false;
                Debug.Log("No any gift lists included!!");
            }
        }

        
    }

    private void getRandomGiftData()
    {
        try
        {
            if (LuckyDrawLogic.Instance != null)
            {
                PrizeData line =null;
                if (LuckyDrawLogic.Instance.HighPrioityDrawData)
                {
                    if (this.filterData[0] != null)
                        line = this.filterData[0];
                }
                else
                {
                    LuckyDrawLogic.Instance.RandId = LuckyDrawLogic.Instance.randomRemainTotalNumberOfGift(this.filterData);
                    var randomIdFromList = LuckyDrawLogic.Instance.FindGiftByRandomId(LuckyDrawLogic.Instance.RandId, LuckyDrawLogic.Instance.giftRemainList);

                    if (this.filterData[randomIdFromList] != null)
                        line = this.filterData[randomIdFromList];
                }

                if (line != null)
                {
                    this.giftResult.giftId = line.PrizeId;
                    this.giftResult.prize_name = line.PrizeName;
                    this.giftResult.gift_image_filename = line.GiftImageFileName;
                    this.giftResult.gift_title_filename = line.GiftTitleFileName;
                    this.giftResult.quota = line.Quota;

                    if(!ConfigPage.Instance.dryrunMode) { 
                        if (line.RedeemedQuota == 0 && line.QuotaLeft == 0)
                        {
                            if (this.giftResult.quota > 0)
                            {
                                this.giftResult.redeemed_quota += 1;
                                this.giftResult.quota_left = this.giftResult.quota - this.giftResult.redeemed_quota;
                            }
                        }
                        else
                        {
                            this.giftResult.redeemed_quota = line.RedeemedQuota;
                            this.giftResult.quota_left = line.QuotaLeft;

                            if (this.giftResult.quota > this.giftResult.redeemed_quota)
                            {
                                this.giftResult.redeemed_quota += 1;
                                this.giftResult.quota_left = this.giftResult.quota - this.giftResult.redeemed_quota;
                            }
                        }
                    }
                    else
                    {
                        this.giftResult.redeemed_quota = line.RedeemedQuota;
                        this.giftResult.quota_left = this.giftResult.quota - this.giftResult.redeemed_quota;
                    }

                    LuckyDrawLogic.Instance.PoolType = line.Pool;
                    this.giftResult.prioritize = line.Prioritize;
                    this.giftResult.pool = line.Pool;
                    this.giftResult.number_of_play_to_win = line.NumberOfPlayToWin;
                    this.giftResult.startDate = line.StartDate;
                    this.giftResult.endDate = line.EndDate;

                    this.giftRecord.drawRecordId = LuckyDrawLogic.Instance.TotalOfDrawTime;
                    this.giftRecord.giftId = this.giftResult.giftId;
                    this.giftRecord.prizeName = this.giftResult.prize_name;
                    this.giftRecord.createTime = LuckyDrawLogic.Instance.CurrentDateTime;
                    this.giftRecord.pool = this.giftResult.pool;
                    this.giftRecord.prioritize = this.giftResult.prioritize;
                    this.giftRecord.quota = this.giftResult.quota;
                    this.giftRecord.redeemed_quota = this.giftResult.redeemed_quota;
                    this.giftRecord.quota_left = this.giftResult.quota_left;

                    line.RedeemedQuota = this.giftResult.redeemed_quota;
                    line.QuotaLeft = this.giftResult.quota_left;

                    if (!string.IsNullOrEmpty(line.GiftImageFilePath) &&
                        !string.IsNullOrEmpty(line.GiftTitleFilePath))
                    {
                        Debug.Log("randomed image & title path");
                        this.giftResult.gift_image_file_path = line.GiftImageFilePath;
                        this.giftResult.gift_title_file_path = line.GiftTitleFilePath;
                        StartCoroutine(loadTexture(this.giftResult.gift_image_file_path, true));
                        StartCoroutine(loadTexture(this.giftResult.gift_title_file_path, false));

                    }
                    else
                    {
                        Debug.Log("missing Randomed gift image && title image path ");
                    }

                    PartId ptId = LuckyDrawLogic.Instance.partId;
                    UpdateLuckyDrawCSVRedeem(this.giftRecord.giftId, ptId);
                    CreateLuckyDrawRecord();
                                                       
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if(LuckyDrawLogic.Instance != null) { 
                LuckyDrawLogic.Instance.debugEnabled = !LuckyDrawLogic.Instance.debugEnabled;
                LuckyDrawLogic.Instance.errorLog = !LuckyDrawLogic.Instance.errorLog;
            }
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            LuckyDrawLogic.Instance.errorLog = !LuckyDrawLogic.Instance.errorLog;
        }
    }

    private string[] TrimEmptyLines(string[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Trim();
        }

        lines = RemoveEmptyLines(lines);

        return lines;
    }

    private string[] RemoveEmptyLines(string[] lines)
    {
        lines = lines.Where(line => !string.IsNullOrEmpty(line)).ToArray();
        return lines;
    }

    public string ReadFileAtLocation(string filePath, bool autoCreate=false, HeaderName[] headerName=null)
    {
        //This resets the file string just in case read.ReadToEnd() does not overwrite it. 
        string _file = "";
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            lines = TrimEmptyLines(lines);
            _file = string.Join("\n", lines);
        }
        else
        {
            if (autoCreate)
            {
                // Create the file if it doesn't exist
                FileStream fileStream = File.Create(filePath);
                fileStream.Close();
                Debug.Log("Created new lucky draw record file at " + filePath);

                // Add the header to the file
                StringBuilder header = new StringBuilder();
                foreach(var _header in headerName)
                {
                    header.Append(_header.Title+",");
                }

                File.WriteAllText(filePath, header.ToString());
                Debug.Log("Created new file at " + filePath + " with header");
            }
            else
            {
                Debug.LogError("File at " + filePath + " does not exist");
            }
        }

        return _file;
    }

    public string getLuckyDrawCSVPath
    {
        get {  

            if(!string.IsNullOrEmpty(this.luckyDrawPhraseFile)) { 
                if(this.luckyDrawPhraseFile.Contains(".csv"))
                    return Path.Combine(Application.streamingAssetsPath, this.luckyDrawPhraseFile);
                else
                    return Path.Combine(Application.streamingAssetsPath, this.luckyDrawPhraseFile + ".csv");
            }
            else 
                return "";
        }
    }

    private string luckyDrawRecordPath;
    public string getLuckyDrawRecordPath
    {
        get
        {
            string recordFileName = luckyDrawPhraseFile.Replace("CSV/", "");
            this.luckyDrawRecordPath = Path.Combine(Application.streamingAssetsPath, recordCSVFolderName + "/" + recordFileName + "_Record.csv");
            return this.luckyDrawRecordPath;
        }
        set
        {
            this.luckyDrawRecordPath = value;
        }
    }

    public string getImagePath(string imageName="")
    {
        return Path.Combine(Application.streamingAssetsPath, imageName);
    }

    public void CreateLuckyDrawRecord()
    {
        if (File.Exists(this.getLuckyDrawRecordPath))
        {
            GiftRecord gr = this.giftRecord;
            StringBuilder newLine = new StringBuilder();
            newLine.Append(gr.drawRecordId);
            newLine.Append(",");
            newLine.Append(gr.giftId);
            newLine.Append(",");
            newLine.Append(gr.prizeName);
            newLine.Append(",");
            newLine.Append(gr.createTime);
            newLine.Append(",");
            newLine.Append(gr.pool);
            newLine.Append(",");
            newLine.Append(gr.prioritize);
            newLine.Append(",");
            newLine.Append(gr.quota);
            newLine.Append(",");
            newLine.Append(gr.redeemed_quota);
            newLine.Append(",");
            newLine.Append(gr.quota_left);

            LuckyDrawLogic.Instance.GiftResultString = newLine.ToString();
            LuckyDrawLogic.Instance.GiftPriority = gr.prioritize;

            if (ConfigPage.Instance != null && ConfigPage.Instance.dryrunMode)
            {
                Debug.Log("It is DryrunMode, data will not be update");
                return;
            }

            LuckyDrawLogic.Instance.AddNewDrawRecord(gr);
            File.AppendAllText(this.getLuckyDrawRecordPath, "\n" + newLine.ToString());
            Debug.Log("Added a new record to the lucky draw record CSV");

        }

        Debug.Log("Reload Again CSV Path");
        this.LoadCSVPathsData();
    }

    public void UpdateLuckyDrawCSVRedeem(int giftRowId, PartId ptId)
    {
        if (ConfigPage.Instance != null && ConfigPage.Instance.dryrunMode)
        {
            Debug.Log("It is DryrunMode, data will not be update");
            return;
        }

        if (File.Exists(getLuckyDrawCSVPath))
        {
            string[] lines = File.ReadAllLines(getLuckyDrawCSVPath);
            // Check if the specified indices are within the grid bounds
            if (giftRowId >= 0 && giftRowId < lines.Length)
            {
                string[] columns = lines[giftRowId].Split(',');

                if (ptId.RedeemedQuotaId >= 0 && ptId.RedeemedQuotaId < columns.Length)
                {
                    // Update the value in the specified grid cell
                    columns[ptId.RedeemedQuotaId] = this.giftResult.redeemed_quota.ToString();
                    columns[ptId.QuotaLeftId] = this.giftResult.quota_left.ToString();
                    columns[ptId.PoolId] = this.giftResult.pool;
                    columns[ptId.PriorityId] = this.giftResult.prioritize.ToString();

                    // Join the modified columns back into a single string
                    string updatedRow = string.Join(",", columns);

                    // Replace the old row with the updated row
                    lines[giftRowId] = updatedRow;

                    // Write the modified lines back to the CSV file
                    File.WriteAllLines(getLuckyDrawCSVPath, lines);

                    Debug.Log("CSV grid value updated.");
                }
            }           
        }
        else
        {
            Debug.LogError("Missing Lucky Draw CSV File");
        }
        
    }


   

    private IEnumerator loadTexture(string filePath, bool isImage=true)
    {
        if(File.Exists(filePath)) { 
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(filePath);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);

                if(isImage)
                    this.giftResult.GiftTexture = texture;
                else
                    this.giftResult.GiftTitleTexture = texture;
                Debug.Log("Texture loaded successfully.");
            }
            else
            {
                this.giftResult.GiftTexture = null;
                this.giftResult.GiftTitleTexture = null;
                Debug.Log("Failed to load texture. Error: " + www.error);
            }
        }
        else
        {
            this.giftResult.GiftTexture = null;
            this.giftResult.GiftTitleTexture = null;
            Debug.Log("File is not existed");
        }
    }
}


[Serializable]
public class GiftRecord
{
    public int drawRecordId;
    public int giftId;
    public string prizeName;
    public string createTime;
    public string pool;
    public int prioritize;
    public int quota;
    public int redeemed_quota;
    public int quota_left;

    public void reset()
    {
        this.drawRecordId = 0;
        this.giftId = 0;
        this.prizeName = "";
        this.createTime = "";
        this.pool = "";
        this.prioritize = 0;
        this.quota = 0;
        this.redeemed_quota = 0;
        this.quota_left = 0;
    }
}

[Serializable]
public class GiftResult
{
    public int giftId;
    public string prize_name = "";
    public string gift_image_filename = "";
    public string gift_title_filename = "";
    public string gift_image_file_path = "";
    public string gift_title_file_path = "";
    public Texture2D _giftTexture = null;
    public Texture2D _giftTitleTexture = null;
    public int quota;
    public int redeemed_quota;
    public int quota_left;
    public int prioritize;
    public string pool = "";
    public int number_of_play_to_win;
    public string startDate;
    public string endDate;

    public Texture2D GiftTexture
    {
        get
        {
            return _giftTexture;
        }
        set
        {
            this._giftTexture = value;
        }
    }

    public Texture2D GiftTitleTexture
    {
        get
        {
            return _giftTitleTexture;
        }
        set
        {
            this._giftTitleTexture = value;
        }
    }

    public void reset()
    {
        this.giftId = 0;
        this.prize_name = "";
        this.gift_image_filename = "";
        this.gift_title_filename = "";
        this.gift_image_file_path = "";
        this.gift_title_file_path = "";
        this.quota = 0;
        this.redeemed_quota = 0;
        this.quota_left = 0;
        this.prioritize = 0;
        this.pool = "";
        this.number_of_play_to_win = 0;
        this.startDate = "";
        this.endDate = "";
        this._giftTexture = null;
        this._giftTitleTexture = null;
    }
}


[Serializable]
public class HeaderName
{
    public string Title;
    public float width;

    public HeaderName(string title="", float width=0)
    {
        this.Title = title;
        this.width = width;
    }
}
