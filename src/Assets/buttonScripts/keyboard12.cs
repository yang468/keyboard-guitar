using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class keyboard12 : MonoBehaviour
{
    public Button btn;
   
    void Update() {
        if(Input.GetKeyDown(KeyCode.C))
        {
            btn.onClick.Invoke();          //即可自动产生点击动作并调用方法。
        }
    }

}