using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class Messages : MonoBehaviour
{
    // [SerializeField] private InputField  inputField;
    [SerializeField] private GameObject Message;
    [SerializeField] private Transform Content;
    [SerializeField] private APIManager chat;

    public void SendMessagePrompt(){
        chat.SendPrompt("what is 6 + 4?");
        string answer = chat.getPrompt();
        GetMessage(answer);
    }

    public void GetMessage(string RecieveMessage){
        GameObject newAnswer = Instantiate(Message,Content);
        TextMeshProUGUI childText = newAnswer.GetComponentInChildren<TextMeshProUGUI>();
        if (childText!=null){
            childText.text = RecieveMessage;
        }
        
    }

}
