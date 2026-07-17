using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class Messages : MonoBehaviour
{
    // [SerializeField] private InputField  inputField;
    [SerializeField] private GameObject Message;
    [SerializeField] public TMP_InputField inputField;
    [SerializeField] private Transform Content;
    [SerializeField] private APIManager chat;

    public void SendMessagePrompt(){
        string prompt = inputField.text;
        
        
      
        if (!string.IsNullOrWhiteSpace(prompt)){
            GetMessage(prompt);
            chat.SendPrompt(prompt,GetMessage);
            // result = chat.getPrompt();

            
        }else{
            // GetMessage("please write me a message");
            GetMessage("please write me a message");
        }
        // Debug.Log("answered message"+result+".");
        
        
    }

    public void GetMessage(string RecieveMessage){
        GameObject newAnswer = Instantiate(Message,Content);
        TextMeshProUGUI childText = newAnswer.GetComponentInChildren<TextMeshProUGUI>();
        if (!string.IsNullOrWhiteSpace(RecieveMessage) && childText!=null){
            childText.text = RecieveMessage;
        }else{
            Debug.Log("null text");
        }
        Debug.Log("message"+RecieveMessage+".");
        
    }

}
