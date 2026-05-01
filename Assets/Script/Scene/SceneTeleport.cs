using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTeleport : MonoBehaviour
{
    [SerializeField] private SceneName sceneNameGoTo = SceneName.Scene1_Farm;
    [SerializeField] private Vector3 scenePositionGoTo = new Vector3();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("SceneTeleport OnTriggerEnter2D: {sceneNameGoTo}");
        if (collision != null)
        {
            PlayerManager player = collision.GetComponent<PlayerManager>();
            if (player != null)
            {
                float xPos = Mathf.Approximately(scenePositionGoTo.x, 0f) ? player.transform.position.x : scenePositionGoTo.x;
                float yPos = Mathf.Approximately(scenePositionGoTo.y, 0f) ? player.transform.position.y : scenePositionGoTo.y;
                float zPos = 0f;

                SceneChangeManager.Instance.FadeAndLoadScene(sceneNameGoTo.ToString(), new Vector3(xPos, yPos, zPos));
            }
        }
    }
}
