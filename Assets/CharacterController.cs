using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            transform.Translate(new Vector3());
        }
    }
}
