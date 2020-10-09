using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKey(KeyCode.W))
        {
            transform.Translate(new Vector3(0, 10*Time.deltaTime, 0));
        }
        if(Input.GetKey(KeyCode.S))
        {
            transform.Translate(new Vector3(0, -10*Time.deltaTime, 0));
        }
        if(Input.GetKey(KeyCode.A))
        {
            transform.Translate(new Vector3(-10*Time.deltaTime, 0, 0));
        }
        if(Input.GetKey(KeyCode.D))
        {
            transform.Translate(new Vector3(10*Time.deltaTime, 0, 0));
        }
    
    
    }
}
