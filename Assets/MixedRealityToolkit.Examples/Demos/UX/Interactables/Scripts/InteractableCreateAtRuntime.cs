using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Create 100 buttons of the given prefab, as well as 
/// </summary>
public class InteractableCreateAtRuntime : MonoBehaviour
{
    public Interactable[] interactablePrefabs = new Interactable[3];
    public float cellSize = 0.3f;
    public Vector3 gridBottomLeft = new Vector3(0, 0, 1);
    public int rows = 10;
    public int cols = 10;


    void Start()
    {
        // Create 100 buttons in a grid, demonstrates how to make interactables at runtime that respond to click events, using the default prefab
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 pos = gridBottomLeft + new Vector3(col * cellSize, row * cellSize, 0);
                int idx = (row * 10 + col) % interactablePrefabs.Length;
                var button = GameObject.Instantiate<Interactable>(interactablePrefabs[idx], pos, Quaternion.identity, transform);
                var r2 = row;
                var c2 = col;
                // Note: it is not very easy to access or add events on interactables, interactable is mostly used to drive visual themes.
                // To listen for OnClick events or OnTouchDown events, use something like below

                // Not recommended:
                // button.OnClick.AddListener(() => Debug.Log($"Clicked row: {r2} col: {c2}!"));
                // 
                // Recommended:
                // TO DO: add a simple class for handling pointer events
                //var pointerClickHandler = button.gameObject.AddComponent<PointerClickHandler>();
                //if (!button.gameObject.GetComponent<NearInteractionTouchable>())
                //{
                //    button.gameObject.AddComponent<NearInteractionTouchable>();
                //}
                
            }
        }

        // Instantiate an interactable purely in code and add it to a gameobject.
        // Notice that with this behavior it is not as easy to configure the themes
        // The recommended method to instantiate an interactable from code in MRTK is to load a prefab,
        // as demonstrated above
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = Vector3.one * 0.2f;
        cube.transform.position = Vector3.forward;
        var interactable = cube.AddComponent<Interactable>();
        interactable.OnClick.AddListener(() => Debug.Log($"Clicked the cube"));

    }
}