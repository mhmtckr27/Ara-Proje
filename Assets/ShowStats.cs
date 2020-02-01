/*
Code by Hayri Cakir
www.hayricakir.com
*/
using TMPro;
using UnityEngine;

public class ShowStats : MonoBehaviour
{
    [SerializeField] private Animal parentAnimal;
    [SerializeField] private TextMeshPro priorityText;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        priorityText.text = parentAnimal.currentPriority.ToString();
    }
}
