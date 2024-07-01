using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ls : MonoBehaviour
{
    public Button loadButton;
    // Start is called before the first frame update
    void Start()
    {
        loadButton.onClick.AddListener(loadScene);
    }

    void loadScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
