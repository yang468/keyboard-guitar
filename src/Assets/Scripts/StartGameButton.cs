using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class StartGameButton : MonoBehaviour
{
    Button button;
    public bool state;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(StartGame);
    }
     
    // Update is called once per frame
    void Update()
    {
        
    }
    private void StartGame()
    {
        if (state==true)
        {
            SceneManager.LoadScene(1);
        }
        if (state==false)
        {
            SceneManager.LoadScene(2);
        }
    }
}
