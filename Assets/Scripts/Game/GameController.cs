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
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private GameObject emptyObject;
    void Start()
    {
        //for(int i = 0; i < 500; i++)
        //{
        //    float x = Random.Range(0, 501);
        //    float z = Random.Range(0, 501);
        //    Instantiate(grass, new Vector3(x, 0, z), Quaternion.identity);
        //}
        for (int i = 150; i < 350; i++)
        {
            for (int j = 150; j < 350; j++)
            {
                RaycastHit hit;

                if (Physics.Raycast(new Vector3(i, -1, j), Vector3.down, out hit, Mathf.Infinity, waterLayer) && hit.distance > 18.6 && hit.distance < 18.9)
                {
                    //GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = new Vector3(i, hit.point.y, j);
                    Instantiate(emptyObject).transform.position = new Vector3(i, hit.point.y, j);
                }
            }
        }
        //for(int i = 0; i < 250; i++)
        //{
        //    float x = Random.Range(150, 350);
        //    float z = Random.Range(150, 350);

        //    RaycastHit hit;
        //    if (Physics.Raycast(new Vector3(x, -1, z), Vector3.down, out hit, Mathf.Infinity, groundLayer) && hit.distance < 19)
        //    {
        //        Instantiate(bunny, hit.point + Vector3.up * 0.25f, Quaternion.identity);
        //    }
        //}     
        ////for(int i = 0; i < 100; i++)
        ////{
        ////    float x = Random.Range(200, 320);
        ////    float z = Random.Range(200, 320);

        ////    RaycastHit hit;
        ////    if (Physics.Raycast(new Vector3(x, -1, z), Vector3.down, out hit, groundLayer))
        ////    {
        ////        Instantiate(fox, hit.point + Vector3.up * 0.25f, Quaternion.identity);
        ////    }
        ////}        
        for (int i = 0; i < 125; i++)
        {
            float x = Random.Range(150, 350);
            float z = Random.Range(150, 350);

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x, -1, z), Vector3.down, out hit, Mathf.Infinity, groundLayer) && hit.distance < 19)
            {
                Instantiate(grass, hit.point + Vector3.down * .05f, Quaternion.identity).transform.up = hit.normal;
            }
        }
        //for(int i = 0; i < 100; i++)
        //{
        //    float x = Random.Range(0, 501);
        //    float z = Random.Range(0, 501);
        //    Instantiate(fox, new Vector3(x, 0, z), Quaternion.identity);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
