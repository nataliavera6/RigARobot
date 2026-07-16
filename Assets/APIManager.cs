using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    [SerializeField] private string gasURL;
    [SerializeField] private string prompt;
    private string answer = null;
    private void Awake()
    {
        Debug.Log("APIManager Awake");

        StartCoroutine(SendDataToGAS());
    }

    public void SendReq()
    {
        Debug.Log("SendReq called");

        StartCoroutine(SendDataToGAS());
    }
    public void SendPrompt(string Prompt)
    {
        Debug.Log("SendReq called");
        prompt = Prompt;
        StartCoroutine(SendDataToGAS());

    }
    public string getPrompt(){
        return answer;
    }
    private IEnumerator SendDataToGAS()
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

        // Debug.Log("Sending request to: " + gasURL);
        // Debug.Log("Prompt : " + prompt);

        WWWForm form = new WWWForm();
        form.AddField("parameter", prompt);

        using UnityWebRequest www =
            UnityWebRequest.Post(gasURL, form);

        yield return www.SendWebRequest();
        answer = www.downloadHandler.text;
        Debug.Log("HTTP code: " + www.responseCode);
        Debug.Log("Response: " + answer);
        
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
