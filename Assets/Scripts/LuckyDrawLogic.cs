using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class LuckyDrawLogic : Singleton<LuckyDrawLogic>
{
    public PartId partId;
    public DateTime currentDateTime;
    public string currentDateTimeString;
    public int totalOfDrawTime = 0;
    public int numberOfPlay = 0;
    public string _pool = "";
    public string _giftResult = "";
    public int _giftPriority = -1;
    public string dateStringFormat = "yyyy-MM-dd HH:mm:ss";
    public List<PrizeData> prizeDataList = null;
    public List<PrizeData> lowPrioityDataSortList = null;
    public List<PrizeData> highPrioityDataSortList = null;
    public List<GiftRecord> highPriorityRecordFulFillTime = null;
    public List<GiftRecord> giftRecordList = null;
    public List<GiftRecord> allLowPriority = null;

    public void Init()
    {
       this.prizeDataList = new List<PrizeData>();
       this.lowPrioityDataSortList = new List<PrizeData>();
       this.highPrioityDataSortList = new List<PrizeData>();
       this.highPriorityRecordFulFillTime = new List<GiftRecord>();
       this.allLowPriority = new List<GiftRecord>();
       this.giftRecordList = new List<GiftRecord>();
       this.ResetData();
    }

    Vector2 resolution;
    Font customFont;
    private void Start()
    {
        if (ScreenController.Instance != null)
            resolution = ScreenController.Instance.resolution;

        string fontPath = "";
#if UNITY_EDITOR
        fontPath = "mingliu.ttc";
#elif UNITY_STANDALONE
            fontPath = "mingliu";
#endif
        customFont = Resources.Load<Font>(fontPath);
    }

    public void ResetData()
    {
        this.GiftResultString = "";
        this.GiftPriority = -1;
        this.NumberOfPlayFromDrawRecord = -1;
        this.PoolType = "";
        this.TotalRandNumber = 0;
        this.RandId = 0;
        this.lowPrioityDataSortList.Clear();
        this.highPrioityDataSortList.Clear();
        Debug.Log("Reset data");
    }


    public int NumberOfPlayFromDrawRecord
    {
        get
        {
            return this.numberOfPlay;
        }
        set
        {
            this.numberOfPlay = value;
        }
    }

    public int TotalOfDrawTime
    {
        get
        {
            return this.totalOfDrawTime;
        }
        set
        {
            this.totalOfDrawTime = value;
        }
    }

    public void LoadLuckyDrawCSVClass(string getLuckyDrawCSVPath)
    {
        StringBuilder error = new StringBuilder("Error:\n");
        string[] lines = getLuckyDrawCSVPath.Split("\n"[0]).Where(line => !string.IsNullOrEmpty(line.Trim())).ToArray();
        if(this.prizeDataList.Count != lines.Length -1)
        {

            for (int i = 1; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    string[] parts = lines[i].Split(","[0]);
                    PrizeData prizeData = CreatePrizeDataFromParts(parts, error);
                    prizeDataList.Add(prizeData);
                }
            }
            Debug.Log("Updated prizeDataList due to csvFile has different lines");

            if (this.errorLog)
                this.ErrorMessage = error.ToString();
        }
    }

    private PrizeData CreatePrizeDataFromParts(string[] parts, StringBuilder error)
    {
        PrizeData prizeData = new PrizeData();
        
        if (parts.Length > partId.PrizeId && int.TryParse(parts[partId.PrizeId].Trim(), out int prizeId))
            prizeData.PrizeId = prizeId;

        if (parts.Length > partId.PrizeNameId)
            prizeData.PrizeName = parts[partId.PrizeNameId].Trim();

        if (parts.Length > partId.GiftImageFilepathId) { 
            string giftImageName = parts[partId.GiftImageFilepathId].Trim();
            prizeData.GiftImageFileName = giftImageName;
            string imageFilePath = CSVManager.Instance.getImagePath(giftImageName);
            if (!File.Exists(imageFilePath))
            {
                this.errorId +=1;
                error.Append(this.errorId + ".image " + prizeData.PrizeId + "missed: " + giftImageName + ";\n");
                this.errorLog = true;
            }
            else
            {
                prizeData.GiftImageFilePath = imageFilePath;
            }
        }

        if (parts.Length > partId.GiftTitleFilepathId) { 
            string giftTitleName = parts[partId.GiftTitleFilepathId].Trim();
            prizeData.GiftTitleFileName = giftTitleName;
            string titleFilePath = CSVManager.Instance.getImagePath(giftTitleName);
            if (!File.Exists(titleFilePath))
            {
                this.errorId += 1;
                error.Append(this.errorId + ".title-image " + prizeData.PrizeId + " missed: " + giftTitleName + ";\n");
                this.errorLog = true;
            }
            else
            {
                prizeData.GiftTitleFilePath = titleFilePath;
            }
        }

        if (parts.Length > partId.QuotaId && int.TryParse(parts[partId.QuotaId].Trim(), out int quota))
            prizeData.Quota = quota;

        if (parts.Length > partId.RedeemedQuotaId && int.TryParse(parts[partId.RedeemedQuotaId].Trim(), out int redeemedQuota))
            prizeData.RedeemedQuota = redeemedQuota;

        if (parts.Length > partId.QuotaLeftId) {
            if(int.TryParse(parts[partId.QuotaLeftId].Trim(), out int quotaLeft))
                prizeData.QuotaLeft = quotaLeft;
            else
                prizeData.QuotaLeft = prizeData.Quota - prizeData.RedeemedQuota;
        }

        if (parts.Length > partId.PriorityId && int.TryParse(parts[partId.PriorityId].Trim(), out int priority))
            prizeData.Prioritize = priority;

        if (parts.Length > partId.PoolId)
            prizeData.Pool = parts[partId.PoolId].Trim();

        if (parts.Length > partId.NumberOfPlayId && int.TryParse(parts[partId.NumberOfPlayId].Trim(), out int numberOfPlay))
            prizeData.NumberOfPlayToWin = numberOfPlay;

        if (parts.Length > partId.StartDateId)
            prizeData.StartDate = parts[partId.StartDateId].Trim();

        if (parts.Length > partId.EndDateId)
            prizeData.EndDate = parts[partId.EndDateId].Trim();

        return prizeData;
    }

    public List<GiftRecord> TotalOfLuckyDrawRecords (string recordFile)
    {
        string[] lines = recordFile.Split("\n"[0]).Where(line => !string.IsNullOrEmpty(line.Trim())).ToArray();
        this.giftRecordList.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(lines[i]))
            {
                string[] parts = lines[i].Split(","[0]);
                GiftRecord gr = new GiftRecord();

                if (!string.IsNullOrEmpty(parts[0]) && int.TryParse(parts[0].Trim(), out int drawRecordId))
                    gr.drawRecordId = drawRecordId;
                if (!string.IsNullOrEmpty(parts[1]) && int.TryParse(parts[1].Trim(), out int giftId))
                    gr.giftId = giftId;

                gr.prizeName = parts[2]?.Trim();
                gr.createTime = parts[3]?.Trim();
                gr.pool = parts[4]?.Trim();

                if (!string.IsNullOrEmpty(parts[5]) && int.TryParse(parts[5].Trim(), out int prioritize))
                    gr.prioritize = prioritize;
                if (!string.IsNullOrEmpty(parts[6]) && int.TryParse(parts[6].Trim(), out int quota))
                    gr.quota = quota;
                if (!string.IsNullOrEmpty(parts[7]) && int.TryParse(parts[7].Trim(), out int redeemedQuota))
                    gr.redeemed_quota = redeemedQuota;
                if (!string.IsNullOrEmpty(parts[8]) && int.TryParse(parts[8].Trim(), out int quotaLeft))
                    gr.quota_left = quotaLeft;

                this.giftRecordList.Add(gr);
            }
        }

        Debug.Log("Updated giftRecordList due to record has different lines");
        FilterLowPriorityDraw();
        return this.giftRecordList;
    }

    public void AddNewDrawRecord(GiftRecord gr)
    {
        if (this.giftRecordList != null && this.giftRecordList.Count > 0)
        {
            this.giftRecordList.Add(gr);
        }
    }

    public void FilterLowPriorityDraw()
    {
        List<GiftRecord> totalRecords = giftRecordList;
        if (totalRecords.Count > 0)
            this.TotalOfDrawTime = totalRecords.Count;
        else
            this.TotalOfDrawTime = 0;

        this.allLowPriority.Clear();
        if (this.giftRecordList != null && this.giftRecordList.Count > 0)
        {
            foreach (var record in this.giftRecordList)
            {
                int priority = record.prioritize;

                if (priority == 0)
                {
                    this.allLowPriority.Add(record);
                }

            }
        }
    }
    public int TotalLowPriorityTimes
    {
        get
        {
            return this.allLowPriority.Count;
        }
    }

    public bool HighPrioityDrawData
    {
        get
        {
            return this.highPrioityDataSortList.Count > 0;
        }
    }

    public DateTime CurrentTime
    {
        get
        {
            return this.currentDateTime;
        }
        set
        {
            this.currentDateTime = value;
        }
    }

    public string CurrentDateTime
    {
        get
        {
            return this.currentDateTimeString;
        }
        set
        {
            this.currentDateTimeString = value;
        }
    }

    public int DataRecordsMatchCurrentDateTime(List<GiftRecord> lines, DateTime startTime, DateTime endTime)
    {
        highPriorityRecordFulFillTime.Clear();
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != null)
            {
                string creatTime = lines[i].createTime;
                if (IsWithinRange(creatTime, startTime, endTime))
                {
                    highPriorityRecordFulFillTime.Add(lines[i]);
                }
            }
        }

        return highPriorityRecordFulFillTime.Count;
    }

    public bool IsWithinRange(string createTime, DateTime startTime, DateTime endTime)
    {
        DateTime createTimeDateTime;

        if (DateTime.TryParseExact(createTime, this.dateStringFormat, null, System.Globalization.DateTimeStyles.None, out createTimeDateTime))
        {
            if (createTimeDateTime >= startTime && createTimeDateTime <= endTime)
            {
                return true;
            }
        }

        return false;
    }

    public PrizeData[] GiftsMatchCurrentDateTime(DateTime currentDateTime)
    {

        lowPrioityDataSortList.Clear();
        highPrioityDataSortList.Clear();

        for(int i=0; i< this.prizeDataList.Count; i++)
        {
            PrizeData prizeData = this.prizeDataList[i];
            if (prizeData != null) { 
                string startDateString = prizeData.StartDate.Trim();
                string endDateString = prizeData.EndDate.Trim();
                DateTime startDate;
                DateTime endDate;

                if (DateTime.TryParseExact(startDateString, "yyyy-MM-dd H:mm:ss", null, System.Globalization.DateTimeStyles.None, out startDate) &&
                    DateTime.TryParseExact(endDateString, "yyyy-MM-dd H:mm:ss", null, System.Globalization.DateTimeStyles.None, out endDate))
                {
                    startDateString = startDate.ToString(this.dateStringFormat);
                    endDateString = endDate.ToString(this.dateStringFormat);
                }

                if (DateTime.TryParseExact(startDateString, this.dateStringFormat, null, System.Globalization.DateTimeStyles.None, out startDate) &&
                    DateTime.TryParseExact(endDateString, this.dateStringFormat, null, System.Globalization.DateTimeStyles.None, out endDate))
                {

                    if (startDate <= currentDateTime && currentDateTime <= endDate)
                    {
                        int quota = prizeData.Quota;
                        int redeemedQuota = prizeData.RedeemedQuota;//default
                        int quota_left = quota - redeemedQuota; //default

                        Debug.Log(prizeData.PrizeId + " quota: " + quota + " redeemedQuota: " + redeemedQuota + " quota_left: " + quota_left);

                        if (quota_left > 0)
                        {
                            int priority = prizeData.Prioritize;

                            if (priority == 1)
                            {
                                int numberOfPlay = prizeData.NumberOfPlayToWin;
                                NumberOfPlayFromDrawRecord = DataRecordsMatchCurrentDateTime(giftRecordList, startDate, endDate);
                                Debug.Log("NumberOfPlayFromDrawRecord: " + NumberOfPlayFromDrawRecord);

                                if (NumberOfPlayFromDrawRecord >= numberOfPlay - 1) {
                                    highPrioityDataSortList.Add(prizeData);
                                }
                            }
                            else
                            {
                                lowPrioityDataSortList.Add(prizeData);
                            }
                        }
                    }
                }

            }
        }

        if (this.HighPrioityDrawData)
        {
            if (highPrioityDataSortList.Count > 1)
                return FindEarliestDateTime(highPrioityDataSortList).ToArray();
            else
                return highPrioityDataSortList.ToArray();
        }
        else
        {
            if(lowPrioityDataSortList.Count > 0)
            {
                if (lowPrioityDataSortList.Count > 1)
                {
                    this.PoolType = GetPoolType;
                    return FindABPoolGifts(lowPrioityDataSortList).ToArray();
                }
                else {
                    this.PoolType = lowPrioityDataSortList[0].Pool;
                    return lowPrioityDataSortList.ToArray();
                }
            } 
            else 
                return null;
        }
    }

    private List<PrizeData> FindEarliestDateTime(List<PrizeData> dataList = null)
    {
        List<PrizeData> earliestGift = new List<PrizeData>();
        int dataListCount = dataList.Count;
        if (dataListCount > 1)
        {
            DateTime earliestDateTime = DateTime.MaxValue;
            for (int i=0; i< dataListCount; i++)
            {
                PrizeData gift = dataList[i];

                string startDateString = gift.StartDate;
                DateTime startDate;

                if (DateTime.TryParseExact(startDateString, this.dateStringFormat, null, System.Globalization.DateTimeStyles.None, out startDate))
                {
                    if (startDate < earliestDateTime)
                    {
                        earliestDateTime = startDate;

                        if (earliestGift.Count == 0)
                            earliestGift.Add(gift);
                        else
                            earliestGift[0] = gift;
                    }
                }

            }
        }

        return earliestGift;
    }

    string GetPoolType
    {
        get
        {
            string pool = "";
            int modValue = CalculateModulo(this.TotalLowPriorityTimes);
            switch (modValue)
            {
                case 0:
                    pool = "A";
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                    pool = "B";
                    break;                   
            }
            return pool;
        }
        
    }

    private List<PrizeData> FindABPoolGifts(List<PrizeData> dataList = null)
    {
        List<PrizeData> poolGifts = new List<PrizeData>();
        int dataListCount = dataList.Count;
        if (dataListCount > 1)
        {          
            for (int i = 0; i < dataListCount; i++)
            {
                PrizeData gift = dataList[i];
                string giftPool = gift.Pool;

                if (giftPool == PoolType)
                {
                    poolGifts.Add(gift);
                }
            }
        }

        if(poolGifts.Count == 0) { 
            poolGifts = dataList;
        }
        
        return poolGifts;
    }

    private int CalculateModulo(int totalDraw)
    {
        var mod = totalDraw % 5;
        return mod;
    }
    public string PoolType
    {
        get { return _pool; }
        set { this._pool = value; }
    }

    public string GiftResultString
    {
        get { return _giftResult; }
        set { this._giftResult = value; }
    }

    public int GiftPriority
    {
        get { return _giftPriority; }
        set { this._giftPriority = value; }
    }

    public int _totalRandNumber = 0;
    public int TotalRandNumber
    {
        get { return _totalRandNumber; }
        set { this._totalRandNumber = value; }
    }

    public int _randId = 0;
    public int RandId
    {
        get { return _randId; }
        set { this._randId = value; }
    }

    public int[] giftRemainList;
    public string Quota_left_string = "";

    public int randomRemainTotalNumberOfGift(PrizeData[] filterData)
    {
        int totalRemain = 0;
        StringBuilder sb = new StringBuilder();
        if (filterData.Length > 0 && filterData != null)
        {
            this.giftRemainList = new int[filterData.Length];

            for (var i = 0; i < filterData.Length; i++)
            {
                var prizeData = filterData[i];
                if (prizeData != null)
                {
                    int quota_left;

                    if(prizeData.QuotaLeft == 0)
                        quota_left = prizeData.Quota;
                    else
                        quota_left = prizeData.QuotaLeft;

                    this.giftRemainList[i] = quota_left;
                    sb.Append(quota_left.ToString() + ",");
                    totalRemain += quota_left;
                }
            }
        }

        this.Quota_left_string = sb.ToString();
        this.TotalRandNumber = totalRemain;
        int randId = UnityEngine.Random.Range(1, totalRemain + 1);
        return randId;
    }

    public int randGiftIdFromArray;
    public int FindGiftByRandomId(int randomId, int[] gifts)
    {
        int sum = 0;
        for (int i = 0; i < gifts.Length; i++)
        {
            sum += gifts[i];
            if (randomId <= sum)
            {
                this.randGiftIdFromArray = i;
                return this.randGiftIdFromArray;
            }
        }
        return -1;
    }

    public bool debugEnabled = true;
    public int debugDepth = 0;
    public float debuglineSpace = 1.5f;
    public int debugTextSize = 50;
    public int areaTextSize = 22;
    public float luckyDrawRecordTextAreaHeight = 0f;
    public float luckyDrawFileTextAreaHeight = 300f;
    public Vector2 scrollPosition_luckyDrawTable = Vector2.zero;
    public Vector2 scrollPosition_luckyDrawFile = Vector2.zero;
    public Vector2 scrollPosition_errorBox = Vector2.zero;
    public float panelWidth = 400f;
    public float panelHeight = 300f;
    public bool errorLog = false;
    public int errorId = 0;
    public float errorBoxWidth = 400f;
    public float errorBoxHeight = 300f;
    public float cellHeight = 60f;
    public int recordsPerPage = 4;
    public int currentPage = 0;

    private void OnGUI()
    {
        if (debugEnabled)
        {
            // Calculate the center position based on the screen dimensions
            float centerX = resolution.x / 2;
            float centerY = resolution.y / 2;

            Rect panelRect = new Rect(centerX - panelWidth / 2f, centerY - panelHeight / 2f, panelWidth, panelHeight);
            GUI.depth = debugDepth;
            GUI.backgroundColor = Color.black;
            GUI.contentColor = Color.white;
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.fontSize = debugTextSize;

            GUI.Box(new Rect(0f, 0f, resolution.x, resolution.y), GUIContent.none, GUI.skin.box);

            // Add another layer for GUI.Label
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = debugTextSize;
            labelStyle.fontStyle = FontStyle.Bold;

            // Display the label
            GUI.Label(new Rect(0f, (debugTextSize * 0 * debuglineSpace), resolution.x, resolution.y), "DateTime: " + DateTime.Now.ToString(this.dateStringFormat), labelStyle);
            GUI.Label(new Rect(0f, (debugTextSize * 1 * debuglineSpace), resolution.x, resolution.y), "Total Gifts Data: " + this.prizeDataList.Count, labelStyle);
            GUI.Label(new Rect(0f, (debugTextSize * 2 * debuglineSpace), resolution.x, resolution.y), "H_Priority Gifts(num): " + this.highPrioityDataSortList.Count, labelStyle);
            GUI.Label(new Rect(0f, (debugTextSize * 3 * debuglineSpace), resolution.x, resolution.y), "L_Priority Gifts(num): " + this.lowPrioityDataSortList.Count, labelStyle);
            GUI.Label(new Rect(0f, (debugTextSize * 4 * debuglineSpace), resolution.x, resolution.y), "TotalOfRecords: " + this.totalOfDrawTime, labelStyle);
            GUI.Label(new Rect(0f, (debugTextSize * 5 * debuglineSpace), resolution.x, resolution.y), "TotalOfLowPriority: " + this.TotalLowPriorityTimes, labelStyle);
            GUI.Label(new Rect(0f, (debugTextSize * 6 * debuglineSpace), resolution.x, resolution.y), "NumberOfPlayToWin(HP): " + NAText(this.NumberOfPlayFromDrawRecord), labelStyle);
            GUI.Label(new Rect(0f, (debugTextSize * 7 * debuglineSpace), resolution.x, resolution.y), "Pool Type: " + this.PoolType, labelStyle);
            GUI.Label(new Rect(0f, (debugTextSize * 8 * debuglineSpace), resolution.x, resolution.y), "Priority: " + NAText(this.GiftPriority), labelStyle);
            GUI.Label(new Rect(0f, (debugTextSize * 9 * debuglineSpace), resolution.x, resolution.y), "Random ID/Total Random Amount: ", labelStyle);
            GUI.Label(new Rect(0f, (debugTextSize * 10 * debuglineSpace), resolution.x, resolution.y), this.RandId + "/" + this.TotalRandNumber, labelStyle);

            GUIStyle giftResultStyle = new GUIStyle(GUI.skin.label);

            giftResultStyle.fontSize = 35;
            giftResultStyle.fontStyle = FontStyle.Bold;
            giftResultStyle.font = customFont;

            GUI.Label(new Rect(0f, (debugTextSize * 11 * debuglineSpace), resolution.x, resolution.y), "Gift Result:", labelStyle);
            GUI.Label(new Rect(0f, (debugTextSize * 12 * debuglineSpace), resolution.x, resolution.y), this.GiftResultString, giftResultStyle);

            GUIStyle textStyle = new GUIStyle(GUI.skin.textArea);
            textStyle.fontSize = areaTextSize;
            textStyle.font = customFont;

            // FPS Monitor
            GUIStyle fpsStyle = new GUIStyle(GUI.skin.label);
            fpsStyle.fontSize = 20;
            fpsStyle.alignment = TextAnchor.UpperRight;
            fpsStyle.font = customFont;
            GUI.Label(new Rect(resolution.x - 110f, 0f, 100f, 30f), "FPS: " + +Mathf.Round(ScreenController.Instance.FPS), fpsStyle);

            GUIStyle tableTextStyle = new GUIStyle(GUI.skin.textArea);
            tableTextStyle.fontSize = areaTextSize;
            tableTextStyle.font = customFont;
            tableTextStyle.alignment = TextAnchor.MiddleCenter;

            /*Rect scrollRect = new Rect(panelRect.x + 10, panelRect.y + luckyDrawRecordTextAreaHeight, panelRect.width - 20, panelRect.height - 20);
            Vector2 contentSize = textStyle.CalcSize(new GUIContent(CSVManager.Instance.luckyDrawRecord));
            scrollPosition_luckyDrawTable = GUI.BeginScrollView(scrollRect, scrollPosition_luckyDrawFile, new Rect(0, 0, contentSize.x, contentSize.y));
            GUI.TextArea(new Rect(0, 0, contentSize.x * 1.5f, contentSize.y), CSVManager.Instance.luckyDrawRecord, textStyle);
            GUI.EndScrollView();*/

            /*Rect scrollRect2 = new Rect(panelRect.x + 10, panelRect.y + luckyDrawFileTextAreaHeight, panelRect.width - 20, panelRect.height - 20);
            Vector2 contentSize2 = textStyle.CalcSize(new GUIContent(CSVManager.Instance.luckyDrawFile));
            scrollPosition_luckyDrawFile = GUI.BeginScrollView(scrollRect2, scrollPosition_luckyDrawFile, new Rect(0, 0, contentSize2.x, contentSize2.y));
            GUI.TextArea(new Rect(0, 0, contentSize2.x * 1.5f, contentSize2.y), CSVManager.Instance.luckyDrawFile, textStyle);
            GUI.EndScrollView();*/

            //GUI Table
            GUILayout.BeginVertical();
            int totalPages = Mathf.CeilToInt((float)giftRecordList.Count / recordsPerPage);
            int startIndex = currentPage * recordsPerPage;
            int endIndex = Mathf.Min(startIndex + recordsPerPage, giftRecordList.Count);

            GUILayout.Window(0, new Rect(panelRect.x, panelRect.y + luckyDrawRecordTextAreaHeight, panelWidth, panelHeight), (id) =>
            {
                scrollPosition_luckyDrawTable = GUILayout.BeginScrollView(scrollPosition_luckyDrawTable, GUILayout.Width(panelWidth - 20), GUILayout.Height(panelHeight - 20));

                GUILayout.BeginHorizontal();
                for (int i = 0; i < CSVManager.Instance.giftRecordHeader.Length; i++)
                {
                    string header = CSVManager.Instance.giftRecordHeader[i].Title;
                    float grid_w = CSVManager.Instance.giftRecordHeader[i].width;
                    GUILayout.Label(header, tableTextStyle, GUILayout.Width(grid_w), GUILayout.Height(cellHeight));
                }
                GUILayout.EndHorizontal();

                for (int i = startIndex; i < endIndex; i++)
                {
                    GUILayout.BeginHorizontal();
                    var record = this.giftRecordList[i];
                    GUILayout.Label(record.drawRecordId.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftRecordHeader[0].width), GUILayout.Height(cellHeight));
                    GUILayout.Label(record.giftId.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftRecordHeader[1].width), GUILayout.Height(cellHeight));
                    GUILayout.Label(record.prizeName.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftRecordHeader[2].width), GUILayout.Height(cellHeight));
                    GUILayout.Label(record.createTime.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftRecordHeader[3].width), GUILayout.Height(cellHeight));
                    GUILayout.Label(record.pool.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftRecordHeader[4].width), GUILayout.Height(cellHeight));
                    GUILayout.Label(record.prioritize.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftRecordHeader[5].width), GUILayout.Height(cellHeight));
                    GUILayout.Label(record.quota.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftRecordHeader[6].width), GUILayout.Height(cellHeight));
                    GUILayout.Label(record.redeemed_quota.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftRecordHeader[7].width), GUILayout.Height(cellHeight));
                    GUILayout.Label(record.quota_left.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftRecordHeader[8].width), GUILayout.Height(cellHeight));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUIStyle pageIdTextStyle = new GUIStyle(GUI.skin.label);
                pageIdTextStyle.fontSize = areaTextSize;
                pageIdTextStyle.font = customFont;
                pageIdTextStyle.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label(("Page " + (currentPage + 1) + " / " + totalPages), pageIdTextStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();


                // Display the "First Page" button
                if (GUILayout.Button("First Page"))
                {
                    currentPage = 0;
                    scrollPosition_luckyDrawTable.y = 0;
                }

                // Display the "Previous Page" button if there is a previous page
                if (currentPage > 0 && GUILayout.Button("Previous Page"))
                {
                    currentPage--;
                    scrollPosition_luckyDrawTable.y = 0;
                }

                // Display the "Next Page" button if there is a next page
                if (currentPage < totalPages - 1 && GUILayout.Button("Next Page"))
                {
                    currentPage++;
                    scrollPosition_luckyDrawTable.y = 0;
                }

                // Display the "Last Page" button
                if (GUILayout.Button("Last Page"))
                {
                    currentPage = totalPages - 1;
                    scrollPosition_luckyDrawTable.y = 0;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }, "Draw Records");
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Window(1, new Rect(panelRect.x, panelRect.y + luckyDrawFileTextAreaHeight, panelWidth, panelHeight), (id) =>
            {
                scrollPosition_luckyDrawFile = GUILayout.BeginScrollView(scrollPosition_luckyDrawFile, GUILayout.Width(panelWidth - 20), GUILayout.Height(panelHeight - 20));

                GUILayout.BeginHorizontal();
                for (int i = 0; i < CSVManager.Instance.giftCSVHeader.Length; i++)
                {
                    string header = CSVManager.Instance.giftCSVHeader[i].Title;
                    float grid_w = CSVManager.Instance.giftCSVHeader[i].width;
                    GUILayout.Label(header, tableTextStyle, GUILayout.Width(grid_w), GUILayout.Height(cellHeight));
                }
                GUILayout.EndHorizontal();

                foreach (var prize in this.prizeDataList)
                {
                    GUILayout.BeginHorizontal();
                     GUILayout.Label(prize.PrizeId.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftCSVHeader[0].width), GUILayout.Height(cellHeight));
                     GUILayout.Label(prize.PrizeName, tableTextStyle, GUILayout.Width(CSVManager.Instance.giftCSVHeader[1].width), GUILayout.Height(cellHeight));
                     GUILayout.Label(prize.GiftImageFileName, tableTextStyle, GUILayout.Width(CSVManager.Instance.giftCSVHeader[2].width), GUILayout.Height(cellHeight));
                     GUILayout.Label(prize.GiftTitleFileName, tableTextStyle, GUILayout.Width(CSVManager.Instance.giftCSVHeader[3].width), GUILayout.Height(cellHeight));
                     GUILayout.Label(prize.Quota.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftCSVHeader[4].width), GUILayout.Height(cellHeight));
                     GUILayout.Label(prize.RedeemedQuota.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftCSVHeader[5].width), GUILayout.Height(cellHeight));
                     GUILayout.Label(prize.QuotaLeft.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftCSVHeader[6].width), GUILayout.Height(cellHeight));
                     GUILayout.Label(prize.Prioritize.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftCSVHeader[7].width), GUILayout.Height(cellHeight));
                     GUILayout.Label(prize.Pool, tableTextStyle, GUILayout.Width(CSVManager.Instance.giftCSVHeader[8].width), GUILayout.Height(cellHeight));
                     GUILayout.Label(prize.NumberOfPlayToWin.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftCSVHeader[9].width), GUILayout.Height(cellHeight));
                     GUILayout.Label(prize.StartDate.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftCSVHeader[10].width), GUILayout.Height(cellHeight));
                     GUILayout.Label(prize.EndDate.ToString(), tableTextStyle, GUILayout.Width(CSVManager.Instance.giftCSVHeader[11].width), GUILayout.Height(cellHeight));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }, "Gift File Details");
            GUILayout.EndVertical();
        }

        if (this.errorLog)
        {
            float centerX = resolution.x / 2;
            float centerY = resolution.y / 2;
            Rect boxRect = new Rect((resolution.x - errorBoxWidth) / 2f, (resolution.y - errorBoxHeight) / 2f, errorBoxWidth, errorBoxHeight);
            GUIStyle textStyle = new GUIStyle(GUI.skin.textArea);
            textStyle.alignment = TextAnchor.MiddleLeft;
            textStyle.fontSize = 30;
            textStyle.normal.textColor = Color.red;

            Rect scrollRect3 = new Rect(boxRect.x + 10, boxRect.y + luckyDrawRecordTextAreaHeight, boxRect.width - 20, boxRect.height - 20);
            Vector2 contentSize = textStyle.CalcSize(new GUIContent(this.ErrorMessage));
            scrollPosition_errorBox = GUI.BeginScrollView(scrollRect3, scrollPosition_errorBox, new Rect(0, 0, contentSize.x, contentSize.y));
            GUI.TextArea(new Rect(0, 0, contentSize.x * 1.5f, contentSize.y), this.ErrorMessage, textStyle);
            GUI.EndScrollView();
        }
    }



    string NAText(int num)
    {
        return num == -1 ? "NA" : num.ToString();
    }

    private string _errorMessage;
    string ErrorMessage 
    {
        get
        {
            return _errorMessage;
        }
        set
        {
            this._errorMessage = value;
        }
    }

    private void OnApplicationQuit()
    {
        this.prizeDataList.Clear();
        this.giftRecordList.Clear();
        this.allLowPriority.Clear();
    }
}

[Serializable]
public class PrizeData
{
    public int PrizeId;
    public string PrizeName;
    public string GiftImageFileName;
    public string GiftTitleFileName;
    public int Quota;
    public int RedeemedQuota;
    public int QuotaLeft;
    public int Prioritize;
    public string Pool;
    public int NumberOfPlayToWin;
    public string StartDate;
    public string EndDate;
    public string GiftImageFilePath;
    public string GiftTitleFilePath;

}




[Serializable]
public class PartId
{
    public int PrizeId = 0;
    public int PrizeNameId = 1;
    public int GiftImageFilepathId = 2;
    public int GiftTitleFilepathId = 3;
    public int QuotaId=4;
    public int RedeemedQuotaId = 5;
    public int QuotaLeftId = 6;
    public int PriorityId= 7;
    public int PoolId = 8;
    public int NumberOfPlayId = 9;
    public int StartDateId = 10;
    public int EndDateId = 11;
}
