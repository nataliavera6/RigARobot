using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CharacterRender : MonoBehaviour
{
    public GameObject[] characterPrefab; // The character prefab to instantiate
    private int currentCharacterIndex = 0; // Index of the currently selected character
    public Vector3 position;
    public Quaternion rotation;
    private GameObject currentCharacter;
    public MultiARMarkersToRigTargets markerRigController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowCharacter(currentCharacterIndex);
    }
    public GameObject getRigs(){
        return currentCharacter;
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void NextCharacter(){
        currentCharacterIndex += 1;
        if (currentCharacterIndex >= characterPrefab.Length){
            currentCharacterIndex = 0;
        }
        ShowCharacter(currentCharacterIndex);
        

    }
    public void PreviousCharacter(){
        currentCharacterIndex -= 1;
        if (currentCharacterIndex <0){
            currentCharacterIndex = characterPrefab.Length-1;
        }
        ShowCharacter(currentCharacterIndex);
        
    }
    private void ShowCharacter(int index)
    {   
        Debug.Log(index);
        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }

        currentCharacter = Instantiate(characterPrefab[index],position ,rotation );
        Dictionary<string, Transform> dictionary=currentCharacter.GetComponentInChildren<markertorig>(true).getRigtoMarkerSetup();
        markerRigController.setRigTargets(dictionary);
  
    }
}
