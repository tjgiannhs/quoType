using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Sockets;
using System.IO;
using System;
using System.Globalization;
using Unity.Services.Core;
using Unity.Services.Analytics;

public class Typing: MonoBehaviour
{
    [SerializeField] TMP_InputField  textInput;
    [SerializeField] TextMeshProUGUI textOutput;
    [SerializeField] TextMeshProUGUI floatingText;
    [SerializeField] GameObject linkButton;
    [SerializeField] GameObject timerText;
    [SerializeField] GameObject creditsText;
    [SerializeField] AudioClip myAudioClip;
    AudioSource myAudioSource;
    string targetText;
    [SerializeField] Image backgroundImage;
    string bgColorHmlValue;
    int nextLetterIndex = 0;
    char nextLetter;
    bool gameEnded = false;
    int totalSecondsToday = -1;
    int currentDayNumber;
    int releaseDayNumber = 334;//the day it was released to the public, 30/11/2022
    int quotesFileIndex;
    string siteURL = "https://en.wikipedia.org/wiki/";
    [SerializeField] List<TextAsset> quoteFilesList;

    void Start()
    {
        Application.runInBackground = true;
        //GetCurrentTimeOnline();
        AddAnalytics();
        GetCurrentTime();
        myAudioSource = GetComponent<AudioSource>();
        bgColorHmlValue = ColorUtility.ToHtmlStringRGBA(backgroundImage.color);
        string[] fileInfo = GetQuoteInfoFromFile().Split('/');
        linkButton.GetComponent<TextMeshProUGUI>().text = fileInfo[0];
        targetText = fileInfo[1];
        siteURL += fileInfo[2];
        string s = "";
        string[] sa = textOutput.text.Split(" ");
        for(int i=2; i<sa.Length-1; i++)
        {
            s+=sa[i]+" ";
        }
        s+=sa[sa.Length-1];//to not have a space at the end
        textOutput.text = "<color=#c4b9aa>" + targetText.Split(" ")[0]+" "+targetText.Split(" ")[1] + " </color>";
        MakeNextTextFloat();
    }

    async void AddAnalytics()
    {
        try
        {
            await UnityServices.InitializeAsync();
            List<string> consentIdentifiers = await AnalyticsService.Instance.CheckForRequiredConsents();
        }
        catch (ConsentCheckException e)
        {
          // Something went wrong when checking the GeoIP, check the e.Reason and handle appropriately.
        }
    }

    void GameSetUp(int daysToAdd)
    {
        timerText.gameObject.SetActive(false);
        creditsText.gameObject.SetActive(false);
        linkButton.gameObject.SetActive(false);
        gameEnded = false;
        nextLetterIndex = 0;
        siteURL = "https://en.wikipedia.org/wiki/";
        currentDayNumber+=daysToAdd;
        floatingText.gameObject.SetActive(true);
        string[] fileInfo = GetQuoteInfoFromFile().Split('/');
        linkButton.GetComponent<TextMeshProUGUI>().text = fileInfo[0];
        targetText = fileInfo[1];
        siteURL += fileInfo[2];
        string s = "";
        string[] sa = textOutput.text.Split(" ");
        for(int i=2; i<sa.Length-1; i++)
        {
            s+=sa[i]+" ";
        }
        s+=sa[sa.Length-1];//to not have a space at the end
        textOutput.text = "<color=#c4b9aa>" + targetText.Split(" ")[0]+" "+targetText.Split(" ")[1] + " </color>";
        MakeNextTextFloat();
    }
    string GetQuoteInfoFromFile()
    {
        TextAsset quotesFile = quoteFilesList[quotesFileIndex];
        var content = quotesFile.text;
        var allLines = content.Split("\n");
        List<string> lines = new List<string>(allLines);

        //string data = System.IO.File.ReadAllText(Application.dataPath+"/"+quotesFileName+".txt");
        //string[] lines = data.Split(System.Environment.NewLine);
        if(lines.Count<=currentDayNumber-releaseDayNumber) return lines[currentDayNumber-releaseDayNumber-lines.Count   ];//to prevent the case of crashing if not having a quote
        else return lines[currentDayNumber - releaseDayNumber];
    }
    void GetCurrentTime()
    {
        print((int)DateTime.Now.TimeOfDay.TotalSeconds/3600+":"+(int)DateTime.Now.TimeOfDay.TotalSeconds%3600/60+":"+(int)DateTime.Now.TimeOfDay.TotalSeconds%60);
        quotesFileIndex = (DateTime.Now.Year-2022)%2;//when adding files for more years than 2 years (2022 and 2023) just increase the mod number by 1
        print(quotesFileIndex);
        if(DateTime.Now.Year!=2022)
        {
            releaseDayNumber=1;//since this releases in late 2022 the quotes will be less at first for file 0.txt
        }
        print(DateTime.Now.DayOfYear+"-"+DateTime.Now.Year);
        currentDayNumber = DateTime.Now.DayOfYear;
        totalSecondsToday = (int)DateTime.Now.TimeOfDay.TotalSeconds;
        //StartCoroutine(CountSecondUp());
    }
    void GetCurrentTimeOnline()
    {
        var client = new TcpClient("www.google.com", 13);
        using (var streamReader = new StreamReader(client.GetStream()))
        {
            var response = streamReader.ReadToEnd();
            var utcDateTimeString = response.Substring(7, 17);
            var localDateTime = DateTime.ParseExact(utcDateTimeString, "yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            print((int)localDateTime.TimeOfDay.TotalSeconds/3600+":"+(int)localDateTime.TimeOfDay.TotalSeconds%3600/60+":"+(int)localDateTime.TimeOfDay.TotalSeconds%60);
            quotesFileIndex = (localDateTime.Year-2022)%2;//when adding files for more years than 2 years (2022 and 2023) just increase the mod number by 1
            print(quotesFileIndex);
            if(localDateTime.Year!=2022)
            {
                releaseDayNumber=1;//since this releases in late 2022 the quotes will be less at first for file 0.txt
            }
            print(localDateTime.DayOfYear+"-"+localDateTime.Year);
            currentDayNumber = localDateTime.DayOfYear;
            totalSecondsToday = (int)localDateTime.TimeOfDay.TotalSeconds;
            //StartCoroutine(CountSecondUp());
        }
    }

    // IEnumerator CountSecondUp()
    // {
    //     yield return new WaitForSeconds(1);
    //     totalSecondsToday++;
    //     if(totalSecondsToday>86400) Application.LoadLevel(Application.loadedLevel);
    //     StartCoroutine(CountSecondUp());
    // }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.DownArrow))  Application.LoadLevel(Application.loadedLevel);
        //for testing next and previous day's quotes
        //if(Input.GetKeyDown(KeyCode.RightArrow))  GameSetUp(1);
        //if(Input.GetKeyDown(KeyCode.LeftArrow))  GameSetUp(-1);
        floatingText.transform.localPosition = new Vector3(floatingText.transform.localPosition.x,floatingText.transform.localPosition.y+(20-floatingText.transform.localPosition.y)*4*Time.deltaTime,floatingText.transform.localPosition.z);
        if(linkButton.active) linkButton.transform.localPosition = new Vector3(linkButton.transform.localPosition.x,linkButton.transform.localPosition.y+(-211-linkButton.transform.localPosition.y)*4*Time.deltaTime,linkButton.transform.localPosition.z);
        if(timerText.active){
            totalSecondsToday = (int)DateTime.Now.TimeOfDay.TotalSeconds;
            if(totalSecondsToday==0) Application.LoadLevel(Application.loadedLevel);
            creditsText.transform.localPosition = new Vector3(creditsText.transform.localPosition.x,creditsText.transform.localPosition.y+(-270-creditsText.transform.localPosition.y)*4*Time.deltaTime,creditsText.transform.localPosition.z);
            timerText.transform.localPosition = new Vector3(timerText.transform.localPosition.x,timerText.transform.localPosition.y-(timerText.transform.localPosition.y-257)*4*Time.deltaTime,timerText.transform.localPosition.z);
            timerText.GetComponent<TextMeshProUGUI>().text = "Next quote in "+((int)(86400-totalSecondsToday)/3600).ToString("00")+":"+((int)(86400-totalSecondsToday)%3600/60).ToString("00")+":"+((86400-totalSecondsToday)%60).ToString("00");
        }
    }

    public void LetterPressed()
    {
        if(CheckPressedLetter()){ PlayAudioClip(); CorrectLetterPressed(); }
    }

    public void PlayAudioClip()
    {
        if(PlayerPrefs.GetInt("SoundOn",1)>0.5f)
        {
            myAudioSource.PlayOneShot(myAudioClip, 0.14f);
        }
    }

    bool CheckPressedLetter()
    {
        //print(textInput.text[textInput.text.Length-1].ToString()+"-"+textInput.text);
        try{
            if(textInput.text[textInput.text.Length-1].ToString()==targetText[nextLetterIndex].ToString()) return true;
        }catch{
        }

        return false;
    }

    void CorrectLetterPressed()
    {       
        GetNewNextLetter();
        //print(htmlValue);
        //if(textInput.text[textInput.text.Length-1].ToString() == " ") MakeNextTextFloat();
        
        string hiddenWords = "";
        for(int i=1; i<targetText.Substring(nextLetterIndex).Split(" ").Length-1; i++) hiddenWords+=targetText.Substring(nextLetterIndex).Split(" ")[i]+" ";
        hiddenWords+=targetText.Substring(nextLetterIndex).Split(" ")[targetText.Substring(nextLetterIndex).Split(" ").Length-1];

        string nextTwoWords=" ";
        try{
            if(targetText[nextLetterIndex] != ' ') nextTwoWords = targetText.Substring(nextLetterIndex).Split(" ")[0] + " " + targetText.Substring(nextLetterIndex).Split(" ")[1];
            else nextTwoWords = " "+targetText.Substring(nextLetterIndex).Split(" ")[1] + " " + targetText.Substring(nextLetterIndex).Split(" ")[2];
        }catch{
            try{
                if(targetText.Substring(nextLetterIndex).Split(" ")[0].Length!=0) nextTwoWords = targetText.Substring(nextLetterIndex).Split(" ")[0];
                else nextTwoWords = " " + targetText.Substring(nextLetterIndex).Split(" ")[1];
            }
            catch{}
        }

        if(nextLetterIndex<targetText.Length && targetText[nextLetterIndex] == ' ') MakeNextTextFloat();


        textOutput.text = "<color=#5C5C5C>" + targetText.Substring(0,nextLetterIndex)+ "</color><color=#c4b9aa>" + nextTwoWords + "</color><color=#11223300> " + hiddenWords + "</color>";
        
        if(nextLetterIndex == targetText.Length) {
            gameEnded = true; 
            StartCoroutine(ShowName());
            StartCoroutine(ShowTimer());
        }
    }

    IEnumerator ShowName()
    {
        linkButton.transform.localPosition = new Vector3(linkButton.transform.localPosition.x,linkButton.transform.localPosition.y-80,linkButton.transform.localPosition.z);
        yield return new WaitForSeconds(0.25f);
        linkButton.SetActive(true);
    }
    IEnumerator ShowTimer()
    {
        timerText.transform.localPosition = new Vector3(timerText.transform.localPosition.x,timerText.transform.localPosition.y+80,timerText.transform.localPosition.z);
        creditsText.transform.localPosition = new Vector3(creditsText.transform.localPosition.x,creditsText.transform.localPosition.y-80,creditsText.transform.localPosition.z);
        yield return new WaitForSeconds(1f);
        timerText.GetComponent<TextMeshProUGUI>().text = "Next quote in "+((int)(86400-totalSecondsToday)/3600).ToString("00")+":"+((int)(86400-totalSecondsToday)%3600/60).ToString("00")+":"+((86400-totalSecondsToday)%60).ToString("00");
        timerText.SetActive(true);
        creditsText.SetActive(true);
    }
    void MakeNextTextFloat()
    {
        if(gameEnded) return;
        try{
            floatingText.text = "<color=#11223300>" + targetText.Substring(0,nextLetterIndex+1)+targetText.Substring(nextLetterIndex+1).Split(" ")[0]+" "+ targetText.Substring(nextLetterIndex+1).Split(" ")[1]+" </color><color=#c4b9aa>" + targetText.Substring(nextLetterIndex+1).Split(" ")[2] + "</color>";
            floatingText.transform.localPosition = new Vector3(floatingText.transform.localPosition.x,floatingText.transform.localPosition.y-80,floatingText.transform.localPosition.z);
        }catch{
            floatingText.gameObject.SetActive(false);
        }
    }
    void GetNewNextLetter()
    {
        nextLetterIndex++ ;
        nextLetter = textOutput.text[nextLetterIndex]; 
    }

    public void LinkPressed(){ Application.OpenURL(siteURL);}

}
