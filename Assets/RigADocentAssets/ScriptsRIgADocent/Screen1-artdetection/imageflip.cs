using UnityEngine;

public class imageflip : MonoBehaviour
{
    [SerializeField] GameObject imageCam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    //    imageCam = Instantiate(imageCam);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
   
    public void stopMirroringCamera(){
        RectTransform transformation = imageCam.GetComponent<RectTransform>();
        Vector3 scale = transformation.localScale;
        scale.x=-scale.x;

        transformation.localScale=scale;
        Debug.Log("flipped");
    }
}
