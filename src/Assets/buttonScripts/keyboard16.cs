using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class keyboard16 : MonoBehaviour
{
    public Button btn;
   
    void Update() {
        if(Input.GetKeyDown(KeyCode.V))
        {
            btn.onClick.Invoke();          //即可自动产生点击动作并调用方法。
        }
    }

}