using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayerScript : MonoBehaviour
{
    AudioSource Speaker;
    public AudioClip[] clips;

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.FindGameObjectsWithTag("Music").Length > 1)
        {
            Destroy(this.gameObject);
        }
        Speaker = GetComponent<AudioSource>();
        Speaker.volume = .22f;
        DontDestroyOnLoad(transform.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Speaker.isPlaying)
        {
            int temp = Random.Range(0, clips.Length);
            Speaker.clip = clips[temp];
            Speaker.PlayOneShot(Speaker.clip);
        }
    }
}
