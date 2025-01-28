using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZone : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
       Destroy(collision.gameObject);
       if (collision.gameObject.tag == "Player")
       {
           SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

       }
    }
}
