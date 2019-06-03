using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

public class PointerConfigurationExample : MonoBehaviour
{
    /* Turns off all far interaction pointers */
    public void TurnOffFarPointers()
    {
        Debug.Log("Line pointers off");
        SetFarPointersDisabled(true);
    }

    public void TurnOnFarPointers()
    {
        Debug.Log("Line pointers on");
        SetFarPointersDisabled(false);
    }

    private void SetFarPointersDisabled(bool isDisabled)
    {
        FocusProvider focusProvider = (FocusProvider) MixedRealityToolkit.InputSystem.FocusProvider;
        if (focusProvider != null)
        {
            foreach(var mediator in focusProvider.PointerMediators)
            {
                // Note: you could check here to make sure you only disable pointers for hands
                CustomPointerMediator myMediator = (CustomPointerMediator) (mediator.Value);
                if (myMediator != null)
                {
                    myMediator.FarPointersDisabled = isDisabled;
                }
            }
        }
    }

}
