using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pheromone : MonoBehaviour
{

    
    public AntManager manager { get; set; }
    public Vector2 pos { get; set; }
    public float strength { get; set; }

    public enum Type
    {
        Food,
        Home,
    }

    public Type type;
 
    // Update is called once per frame
    private void UpdateStrength()
    {
        // use exponential decay
        strength *= manager.pheromoneDecay;
    }

    private void UpdateColor() {
        GetComponent<SpriteRenderer>().color = type == Type.Food ? Color.green : Color.red;
        //set opacity based on strength
        GetComponent<SpriteRenderer>().color = new Color(GetComponent<SpriteRenderer>().color.r, GetComponent<SpriteRenderer>().color.g, GetComponent<SpriteRenderer>().color.b, strength );
    }
    
    public void FixedUpdate()
    {
        // update strength
        UpdateStrength();
        
    }

    public void Update()
    {
        UpdateColor();
        transform.position = pos;
    }
}
