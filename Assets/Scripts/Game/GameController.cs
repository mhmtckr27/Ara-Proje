/*
Code by Hayri Cakir
www.hayricakir.com
*/
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject grass;
    [SerializeField] private GameObject bunny;
    [SerializeField] private GameObject fox;
    void Start()
    {
        for(int i = 0; i < 500; i++)
        {
            float x = Random.Range(0, 501);
            float z = Random.Range(0, 501);
            Instantiate(grass, new Vector3(x, 0, z), Quaternion.identity);
        }
        for(int i = 0; i < 500; i++)
        {
            float x = Random.Range(0, 501);
            float z = Random.Range(0, 501);
            Instantiate(bunny, new Vector3(x, 0, z), Quaternion.identity);
        }
        for(int i = 0; i < 100; i++)
        {
            float x = Random.Range(0, 501);
            float z = Random.Range(0, 501);
            Instantiate(fox, new Vector3(x, 0, z), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
