using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweetManager : MonoBehaviour
{
    public GameObject tweetPrefab;

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            Transform tweet = Instantiate(tweetPrefab).transform;
            tweet.SetParent(transform, true);

            tweet.position = new Vector3(Random.Range(-2000, 2000), 0, Random.Range(-2000, 2000));
        }
    }
}