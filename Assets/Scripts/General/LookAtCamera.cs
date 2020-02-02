/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;
using UnityEngine.SceneManagement;

public class LookAtCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //transform.rotation = UnityEditor.SceneView.lastActiveSceneView.camera.transform.rotation;
        transform.rotation = Camera.main.transform.rotation;
    }
}
