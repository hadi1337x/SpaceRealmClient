using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    private bool isPlayerAssigned = false;

    void Start()
    {
        StartCoroutine(AssignPlayerAfterSpawn());
    }

    private IEnumerator AssignPlayerAfterSpawn()
    {
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            yield return null;
        }

        offset = transform.position;
        if (player.CompareTag("Player"))
        {
            isPlayerAssigned = true;
        }
        else
        {
            Debug.LogError("Player is not the local player or Player tag is missing.");
        }
    }
    void LateUpdate()
    {
        if (isPlayerAssigned)
        {
            transform.position = player.position + offset;
            Debug.Log($"Camera Updated to Position: {transform.position}");
        }
    }

    void Update()
    {
        if (isPlayerAssigned)
        {
            transform.position = player.position + offset;
            Debug.Log("CamCam");
        }
    }
}
