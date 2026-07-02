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
    
    private List<MarkertoRig> CreatePreestablishedMarkers(){
        List<MarkertoRig> defaultMarkers = new List<MarkertoRig>();
        defaultMarkers.Add(new MarkertoRig{markerName="marker1",RigTarget=null });
        defaultMarkers.Add(new MarkertoRig{markerName="rightArm",RigTarget=null });
        defaultMarkers.Add(new MarkertoRig{markerName="LeftLegMarker",RigTarget=null });
        defaultMarkers.Add(new MarkertoRig{markerName="RightLegMarker",RigTarget=null });
        defaultMarkers.Add(new MarkertoRig{markerName="HeadMarker",RigTarget=null });
        defaultMarkers.Add(new MarkertoRig{markerName="SelfMarker",RigTarget=null });
        return defaultMarkers;
    }
    private void Reset(){
        MarkerRigList = CreatePreestablishedMarkers();
    }
    
  
    private void Awake(){
        MarkerRigDic.Clear();
        foreach(MarkertoRig binding in MarkerRigList){
            if(binding==null||string.IsNullOrEmpty(binding.markerName)|| binding.RigTarget==null){
               continue;
            }
            MarkerRigDic.Add(binding.markerName, binding.RigTarget);
        }
    }
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
