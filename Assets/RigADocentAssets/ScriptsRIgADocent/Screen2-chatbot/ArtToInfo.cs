using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ArtToInfo : MonoBehaviour
{
    [System.Serializable]
    public class ArtPiece
    {
        public string name;
        public Sprite image;
    }

    [SerializeField]
    private List<ArtPiece> art = new List<ArtPiece>();

    [SerializeField]
    private Button imageButtonPrefab;

    [SerializeField]
    private Transform buttonParent;

    [SerializeField]
    private APIManager AI;

    private Dictionary<string, Sprite> artInfo =
        new Dictionary<string, Sprite>();

    private string currentPainting = "";

    private void Awake()
    {
        artInfo.Clear();

        foreach (ArtPiece piece in art)
        {
            if (piece == null)
            {
                Debug.LogWarning("An ArtPiece entry is null.");
                continue;
            }

            if (string.IsNullOrEmpty(piece.name))
            {
                Debug.LogWarning("An artwork has no name.");
                continue;
            }

            if (piece.image == null)
            {
                Debug.LogWarning(
                    "No sprite assigned to: " + piece.name
                );

                continue;
            }

            if (!artInfo.ContainsKey(piece.name))
            {
                artInfo.Add(piece.name, piece.image);
            }
        }

        Debug.Log("Artwork dictionary count: " + artInfo.Count);
    }

    private void Start()
    {
        LoadImages();
    }

    public void LoadImages()
    {
        if (imageButtonPrefab == null)
        {
            Debug.LogError("Image Button Prefab is not assigned.");
            return;
        }

        if (buttonParent == null)
        {
            Debug.LogError("Button Parent is not assigned.");
            return;
        }
        foreach (string identifiedName in IdentifiedArt.Instance.pieces)
        // foreach (ArtPiece piece in art)
        {
            Debug.Log("piece in list"+ identifiedName);
            artInfo.TryGetValue(identifiedName, out Sprite piece);
            if (piece == null || piece == null)
            {
                continue;
            }

            Button newButton = Instantiate(
                imageButtonPrefab,
                buttonParent
            );

            newButton.image.sprite = piece;

            string paintingName = identifiedName;
            
            newButton.onClick.AddListener(() =>
            {
                SelectPainting(paintingName);
            });

            Debug.LogWarning("Loaded artwork: " + paintingName);
        }
    }

    private void SelectPainting(string paintingName)
    {
        currentPainting = paintingName;

        if (AI != null)
        {
            AI.chooseAnArt(currentPainting);
            
        }
        else
        {
            Debug.LogWarning("APIManager is not assigned.");
        }

        Debug.Log("Selected painting: " + currentPainting);
    }
    public void SelectPaintingNull()
    {
        currentPainting = "no painting currently selected.";
        SelectPainting(currentPainting);
        
    }
    public string GetCurrentPainting()
    {
        return currentPainting;
    }
}


// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections.Generic;

// public class ArtToInfo : MonoBehaviour
// {
//     [System.Serializable]
//     public class ArtPiece
//     {
//         public string name;
//         public Sprite image;
//     }

//     public List<ArtPiece> art = new List<ArtPiece>
//     {
//         new ArtPiece
//         {
//             name = "Mona Lisa by Leonardo da Vinci"
//         },
//         new ArtPiece
//         {
//             name = "Starry Night by Vincent van Gogh"
//         },
//         new ArtPiece
//         {
//             name = "The Swing by Jean-Honoré Fragonard"
//         },
//         new ArtPiece
//         {
//             name = "The Scream by Edvard Munch"
//         },
//         new ArtPiece
//         {
//             name = "Dancers in Blue by Edgar Degas"
//         }
//     };

//     [SerializeField]
//     private Button imageButtonPrefab;

//     [SerializeField]
//     private Transform buttonParent;
//     [SerializeField]
//     private APIManager AI;
//     private Dictionary<string, Sprite> artInfo =
//         new Dictionary<string, Sprite>();

//     private string currentPainting = "";

//     private void Awake()
//     {
//         artInfo.Clear();

//         foreach (ArtPiece piece in art)
//         {
//             if (piece == null ||
//                 piece.image == null ||
//                 string.IsNullOrEmpty(piece.name))
//             {
//                 continue;
//             }

//             if (!artInfo.ContainsKey(piece.name))
//             {
//                 artInfo.Add(piece.name, piece.image);
//             }
//         }
//     }

//     private void Start()
//     {
//         LoadImages();
//     }

//     public void LoadImages()
//     {

//         if (IdentifiedArt.Instance == null)
//         {
//             Debug.LogError("IdentifiedArt instance was not found.");
//             return;
//         }

//         // foreach (string identifiedName in IdentifiedArt.Instance.pieces)
//         foreach (ArtPiece Name in art)
    
//         {
//             string identifiedName = Name.name;
//             if (artInfo.TryGetValue(identifiedName, out Sprite artSprite))
//             {
//                 Button newButton = Instantiate(
//                     imageButtonPrefab,
//                     buttonParent
//                 );

//                 newButton.image.sprite = artSprite;

//                 // Save a separate copy for this button
//                 string paintingName = identifiedName;

//                 // Runs when this specific button is clicked
//                 newButton.onClick.AddListener(() =>
//                 {
//                     SelectPainting(paintingName);
//                 });

//                 Debug.Log("Loaded artwork: " + identifiedName);
//             }
//             else
//             {
//                 Debug.LogWarning(
//                     "No artwork information found for: " +
//                     identifiedName
//                 );
//             }
//         }
//     }

//     private void SelectPainting(string paintingName)
//     {

//         currentPainting = paintingName;
//         AI.chooseAnArt(currentPainting);
//         Debug.Log("Selected painting: " + currentPainting);
//     }

//     public string GetCurrentPainting()
//     {
//         return currentPainting;
//     }
// }

// // using UnityEngine;
// // using System.Collections.Generic;
// // using UnityEngine;
// // using UnityEngine.XR.ARFoundation;
// // using UnityEngine.XR.ARSubsystems;
// // using UnityEngine.SceneManagement;
// // public class ArtToInfo : MonoBehaviour
// // {
    
// //     [System.Serializable]
// //     public class ArtPieces{
// //         public string name;
// //         public Sprite image;
        

        

// //     }
// //    public List<ArtPieces> art = new List<ArtPieces>
// //     {
// //         new ArtPieces
// //         {
// //             name = "Mona Lisa by Leonardo da Vinci"
// //         },
// //         new ArtPieces
// //         {
// //             name = "Starry Night by Vincent van Gogh"
// //         },
// //         new ArtPieces
// //         {
// //             name = "The Swing by Jean-Honoré Fragonard"
// //         },
// //         new ArtPieces
// //         {
// //             name = "The Scream by Edvard Munch"
// //         },
// //         new ArtPieces
// //         {
// //             name = "Dancers in Blue by Edgar Degas"
// //         }
// //     };
// //     [System.Serializable] public Button imageButton;
// //     [System.Serializable] public Transform ButtonParent;
// //     public Dictionary<string,Sprite> artInfo = new Dictionary<string,Sprite>();
// //     void Awake(){
// //         artInfo.Clear();
// //         foreach (ArtPieces piece in art){
// //             if (art.image == null||art ==null ||string.IsNullOrEmpty(art.name)){
// //                 continue;
// //             }
// //             else{
// //                 artInfo.Add(art.name,art.image);
// //             }
// //         }
// //     }
// //     public void loadImages(){
// //         foreach (string image in IdentifiedArt.Instance.pieces){
// //             GameObject newImage = Instantiate(imageButton,ButtonParent);
// //             newImage.sourceImage = art.image;
// //         }
// //     }
// //     // Start is called once before the first execution of Update after the MonoBehaviour is created
// //     void Start()
// //     {
        
// //     }

// //     // Update is called once per frame
// //     void Update()
// //     {
        
// //     }
// // }
