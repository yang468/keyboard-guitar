using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class keyboard18 : MonoBehaviour
{
    public Button btn;
   
    void Update() {
        if(Input.GetKeyDown(KeyCode.T))
        {
            btn.onClick.Invoke();          //即可自动产生点击动作并调用方法。
        }
    }

}