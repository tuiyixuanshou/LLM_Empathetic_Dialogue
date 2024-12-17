using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebGLSupport;

public class GetUseInfo : MonoBehaviour
{
    public InputField UserNameInput;
    public InputField AINameInput;
    public InputField AICharacterInput;


    public void testfunc()
    {
        Debug.Log("µ÷ÓÃOn End");
    }

    public void SaveUseName()
    {
        Settings.Instance.UserName = UserNameInput.text;
    }

    public void SaveAIName()
    {
        Settings.Instance.AIName = AINameInput.text;
    }

    public void SaveAICharactor()
    {
        Settings.Instance.AICharacter = AICharacterInput.text;
    }
}
