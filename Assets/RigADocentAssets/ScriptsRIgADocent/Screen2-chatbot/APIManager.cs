using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;




public class APIManager : MonoBehaviour
{
    [SerializeField] private string gasURL="None";
    [SerializeField] private GasURLScript URL;
    [SerializeField] public string prompt;
    [SerializeField] private GameObject currentArt;
    public string fullprompt=   "You are a museum guide.Rules:Do not discuss politics, religion, medical advice, legal advice, or personal opinions.Keep answers under 100 words.Only answer the question provided.";
    static string context = "You are a museum guide.Rules:Do not discuss politics, religion, medical advice, legal advice, or personal opinions.Keep answers under 100 words.Only answer the question provided.";
    public string currprompt;
    public string prevPrompts = context;
    private string answer;
    public string piece;
    private void Awake()
    {
        if (gasURL=="None"||string.IsNullOrEmpty(gasURL)){
            gasURL=URL.getGasURL();
        }else{
            Debug.Log("input AI URL in gasURL");
        }
        
        // Debug.Log("APIManager Awake");

        // StartCoroutine(SendDataToGAS());
    }

    // public void SendReq()
    // {
    //     Debug.Log("SendReq called");

    //     StartCoroutine(SendDataToGAS());
    // }
    public void SendPrompt(string Prompt, Action<string>OnCompleted)
    {
        Debug.Log("SendReq called");
        prompt = Prompt;
        StartCoroutine(SendDataToGAS(OnCompleted));

    }
    public string getAnswer(){
        return answer;
    }
    public void setAnswer(string Answer){
        answer = Answer;
    }

    public void chooseAnArt(string art){
        piece = art;
        //fullprompt = "You are a museum guide.Rules:Only answer questions about the artworks in the museum.If asked about anything else, reply:I can only answer questions about the artworks in this exhibit.Do not discuss politics, religion, medical advice, legal advice, or personal opinions.Keep answers under 100 words.talk about this art piece:"+art+"question:"+prompt;
        //Debug.Log("prompt from chooseanart"+fullprompt);
    }
    public string getfullprompt(){
        fullprompt += "answer context art piece:"+piece+"question:"+prompt;
        return fullprompt;
    }
    public string setPrevPrompt(string prev, string question, string art, string ans ){

        string result= prev + "-start question- context painting:"+art+"user:"+question+"agent response:"+ans+"-end question-";
        return result;
    }
    public string setcurrPrompt(string prev, string question, string art ){

        string result= "answer this Prompt : " + question + "context art piece:"+ art +"previous conversation:"+prev;
        return result;
    }
    private IEnumerator SendDataToGAS(Action<string>OnCompleted)
    {

        Debug.Log("Coroutine started");

        if (string.IsNullOrWhiteSpace(gasURL))
        {
            Debug.LogError("Gas URL is empty in the Inspector.");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(prompt))
        {
            Debug.LogError("Prompt is empty in the Inspector.");
            yield break;
        }
        

        Debug.Log("Sending request to: " + gasURL);
        Debug.Log("answer this Prompt : " + prompt+ "context art piece:"+piece+"previous conversation:"+prevPrompts);
        currprompt = setcurrPrompt(prevPrompts, prompt, piece);
        WWWForm form = new WWWForm();
        form.AddField("parameter", currprompt);
        
        using UnityWebRequest www =
            UnityWebRequest.Post(gasURL, form);

        yield return www.SendWebRequest();
        answer = www.downloadHandler.text;
        Debug.Log("HTTP code: " + www.responseCode);
        Debug.Log("Response: " + answer);
        OnCompleted?.Invoke(answer);
        setAnswer(answer);

        prevPrompts=setPrevPrompt(prevPrompts,prompt,piece,answer);

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "Unity request error: " + www.error
            );
        }
    }

}
// public class APIManager : MonoBehaviour
// {
//     [SerializeField] private string gasURL;
//     [SerializeField] private string prompt;
//     private void Awake(){
//         Debug.Log("space");
//        StartCoroutine(SendDataToGAS());
//     }
//     // private void Update(){
//     //     if (Input.GetKeyDown(KeyCode.Space)){
//     //         Debug.Log("space");
//     //         StartCoroutine(SendDataToGAS());
//     //     }
//     // }
//     public void sendReq(){
//         StartCoroutine(SendDataToGAS());
//     }
//     private IEnumerator SendDataToGAS()
// {
//     WWWForm form = new WWWForm();
//     form.AddField("parameter", prompt);

//     using UnityWebRequest www =
//         UnityWebRequest.Post(gasURL, form);

//     yield return www.SendWebRequest();

//     Debug.Log("HTTP code: " + www.responseCode);
//     Debug.Log("Response: " + www.downloadHandler.text);

//     if (www.result != UnityWebRequest.Result.Success)
//     {
//         Debug.LogError("Unity request error : " + www.error);
//     }
// }
//     // private IEnumerator SendDataToGAS(){
//     //     WWWForm form = new WWWForm();
//     //     form.AddField("parameter",prompt);
//     //     UnityWebRequest www = UnityWebRequest.Post(gasURL, form);
//     //     yield return www.SendWebRequest();
//     //     string response= " ";
//     //     if (www.result == UnityWebRequest.Result.Success){
//     //         response = www.downloadHandler.text;
//     //     }else{
//     //         response = "error buu";
//     //     }
//     //     Debug.Log(response);
//     // }
// }
