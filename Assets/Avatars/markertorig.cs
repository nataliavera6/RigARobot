using UnityEngine;
using System.Collections.Generic;
public class markertorig : MonoBehaviour
{
    [System.Serializable]
    public class MarkertoRig{
        public string markerName;
        public Transform RigTarget; 
    }

    public List<MarkertoRig> MarkerRigList = new List<MarkertoRig>();
    
    private Dictionary<string,Transform> MarkerRigDic= new Dictionary<string,Transform>();
    

    //base marker names so user doesn't have to retype it every time the add an avatar. will show up in inspector when you import the script to a character scene 
    private List<MarkertoRig> CreatePreestablishedMarkers(){
        List<MarkertoRig> defaultMarkers = new List<MarkertoRig>();
        defaultMarkers.Add(new MarkertoRig{markerName="leftArm",RigTarget=null });
        defaultMarkers.Add(new MarkertoRig{markerName="rightArm",RigTarget=null });
        defaultMarkers.Add(new MarkertoRig{markerName="LeftLegMarker",RigTarget=null });
        defaultMarkers.Add(new MarkertoRig{markerName="RightLegMarker",RigTarget=null });
        defaultMarkers.Add(new MarkertoRig{markerName="HeadMarker",RigTarget=null });
        defaultMarkers.Add(new MarkertoRig{markerName="SelfMarker",RigTarget=null });
        return defaultMarkers;
    }
    //call the marker name template creation
    private void Reset(){
        MarkerRigList = CreatePreestablishedMarkers();
    }
    //add list to dictionary with marker name as key and rigtarget as value 
    private void Awake(){
        MarkerRigDic.Clear();
        foreach(MarkertoRig binding in MarkerRigList){
            if(binding==null||string.IsNullOrEmpty(binding.markerName)|| binding.RigTarget==null){
               continue;
            }else{
                MarkerRigDic.Add(binding.markerName, binding.RigTarget);
            }
        }
    }
    //used in ARTrackedImagetoIKTarget to get the new avatar's rig and marker setup
    public Dictionary<string, Transform> getRigtoMarkerSetup(){
        return MarkerRigDic;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
