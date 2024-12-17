using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour
{
    public Vector3 from;
    public Vector3 to;
    public float speed = 1.0f;
    public float duration = 1.0f;

    private float time = 0.0f;
    private int pingPong = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        time += Time.deltaTime;
        if (time > duration)
        {
            time = 0;
            pingPong *= -1;
        }
        float t = time / duration;
        if (pingPong == -1)
        {
            t = 1 - t;
        }
        transform.position = Vector3.Lerp(from, to, t);
    }
}
