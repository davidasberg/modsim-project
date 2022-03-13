using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : MonoBehaviour
{


    public Vector2 pos, vel, desiredDir;
    public AntManager manager { get; set; }

    private float lastPheromoneSpawnTime;

    public bool hasFood;

    
    GameObject holdingFood;

    public void Start() {
        //get food child
        holdingFood = transform.Find("Food").gameObject;
        holdingFood.GetComponent<SpriteRenderer>().enabled = false;
        hasFood = false;
    }

    public void FixedUpdate()
    {
        // desiredDir = (desiredDir + Random.insideUnitCircle * manager.wanderStrength).normalized;

        // Vector2 desiredVel = desiredDir;
        // Vector2 steer = (desiredVel - vel) * manager.steerStrength;
        // Vector2 acc = Vector2.ClampMagnitude(steer, manager.steerStrength);

        Vector2 acc = Vector2.zero;

        acc += GetWanderingForce();
        acc += GetSeparationForce();
        acc += GetPheromoneForce();
        acc += GetVisionForce();
        acc += manager.GetBoundaryForce(this);
        acc += GetConstraintSpeedForce();


        vel = Vector2.ClampMagnitude(vel + acc * Time.deltaTime, manager.maxSpeed);

        HandleFood();
        HandlePheromones();

    
        pos += vel * Time.deltaTime;

    }

    void Update()
    {
        transform.position = pos;
        float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    Vector2 GetVisionForce()
    {
        Vector2 visionForce = Vector2.zero;
        //if close enough to food, walk towards it
        if (!hasFood)
        {
            //check for overlap

            Collider2D food = Physics2D.OverlapCircle(transform.position, manager.visionRange, 1 << LayerMask.NameToLayer("Food"));
            if (food != null)
            {
                //add force to walk towards food
                visionForce = (food.transform.position - transform.position).normalized;
            }
        }
        else {
            //check for overlap
            Collider2D nest = Physics2D.OverlapCircle(transform.position, manager.visionRange, 1 << LayerMask.NameToLayer("Home"));
            if (nest != null)
            {
                //add force to walk towards nest
                visionForce = (nest.transform.position - transform.position).normalized;
            }
        }

        return visionForce * manager.visionStrength;
    }


    void HandleFood() {
        if (!hasFood)
        {
            Collider2D[] foodColliders = Physics2D.OverlapCircleAll(pos, manager.pickupRadius, 1 << LayerMask.NameToLayer("Food"));
            if (foodColliders.Length > 0)
            {
                Food food = foodColliders[0].GetComponent<Food>();
                if (food != null)
                {
                    hasFood = true;
                    holdingFood.GetComponent<SpriteRenderer>().enabled = true;
                    Destroy(food.gameObject);
                    //reverse the velocity
                    vel = -vel;
                }
            }
        }
        else
        {
            Collider2D[] homeColliders = Physics2D.OverlapCircleAll(pos, manager.pickupRadius, 1 << LayerMask.NameToLayer("Home"));
            if (homeColliders.Length > 0)
            {
                hasFood = false;
                holdingFood.GetComponent<SpriteRenderer>().enabled = false;
                manager.foodCollected++;
                //reverse the velocity
                vel = -vel;
            }
        }
    }

    void HandlePheromones() {
        //if enough time has passed, spawn a pheromone
        if (Time.time > lastPheromoneSpawnTime + manager.pheromoneSpawnInterval)
        {
            lastPheromoneSpawnTime = Time.time;
            manager.SpawnPheromone(this);
        }

    }

    Vector2 GetWanderingForce()
    {
        return (vel + Random.insideUnitCircle).normalized * manager.wanderStrength;
    }

    Vector2 GetSeparationForce()
    {
        Vector2 sum = Vector2.zero;
        Collider2D[] otherAnts = Physics2D.OverlapCircleAll(pos, manager.neighborRadius, 1 << LayerMask.NameToLayer("Ant"));
        foreach (Collider2D collider in otherAnts)
        {
            if (collider == GetComponentInChildren<Collider2D>())
            {
                continue;
            }
            //get ant from collider
            Ant otherAnt = collider.GetComponentInParent<Ant>();
            float distance = (otherAnt.pos - pos).magnitude;
            sum += manager.separationForceFactor * ((manager.neighborRadius - distance) / distance) * (pos - otherAnt.pos);
        }
        return sum;
    }

    Vector2 GetConstraintSpeedForce()
    {
        Vector2 force = Vector3.zero;

        //Apply drag
        force -= manager.dragForce * vel;

        float velMag = vel.magnitude;
        if (velMag == 0)
        {
            Debug.Log("VelMag is 0");
        }
        if (velMag > manager.maxSpeed)
        {
            //If speed is above the maximum allowed speed, apply extra friction force
            force -= (5.0f * (velMag - manager.maxSpeed) / velMag) * vel;
        }
        else if (velMag < manager.minSpeed)
        {
            //Increase the speed slightly in the same direction if it is below the minimum
            force += (1.0f * (manager.minSpeed - velMag) / velMag) * vel;
        }

        return force;
    }

    Vector2 GetPheromoneForce()
    {

        //sample all pheromones within a radius
        //get all pixels within radius
        //get the average pheromone strength
        //get the average pheromone direction

        Vector2 dir = Vector2.zero;
        Vector2 posForward = transform.GetChild(1).position;
        Vector2 posLeft = transform.GetChild(2).position;
        Vector2 posRight = transform.GetChild(3).position;

        float strengthForward = 0f;
        float strengthLeft = 0f;
        float strengthRight = 0f;

        //get all pixels within radius
        IEnumerable sample = manager.GetPheromonesWithinRadius(posForward, manager.sampleRadius);
        foreach (Color color in sample)
        {
            strengthForward += (hasFood ? color.g : color.r) * color.a / 255;
        }

        // if(strengthForward > 0){
        //     Debug.Log("Strength forward: " + strengthForward);
        // }

        foreach (Color color in manager.GetPheromonesWithinRadius(posLeft, manager.sampleRadius))
        {
            strengthLeft += (hasFood ? color.g : color.r) * color.a / 255;
        }

        foreach (Color color in manager.GetPheromonesWithinRadius(posRight, manager.sampleRadius))
        {
            strengthRight += (hasFood ? color.g : color.r) * color.a / 255;
        }

        //if right is stronger, go right
        if (strengthRight > strengthLeft && strengthRight > strengthForward)
        {
            dir = (posRight - pos);
        }
        //if left is stronger, go left
        else if (strengthLeft > strengthRight && strengthLeft > strengthForward)
        {
            dir = (posLeft - pos);
        }
        //if forward is stronger, go forward
        else if (strengthForward > strengthLeft && strengthForward > strengthRight)
        {
            dir = (posForward - pos);
        }
        // if (sum.magnitude > 0)
        // {
        //     Debug.Log("Sum magnitude: " + sum.magnitude);
        // }
        return dir.normalized * manager.pheromoneSteerStrength;
    }


    // Vector2 GetPheromoneForce()
    // {
    //     //sample all pheromones in 3 areas in front of the ant
    //     Vector2 sum = Vector2.zero;

    //     //get pos of the 3 areas
    // 	//get child using index
    //     Vector2 posForward = transform.GetChild(0).position;
    //     Vector2 posLeft = transform.GetChild(1).position;
    //     Vector2 posRight = transform.GetChild(2).position;

    //     //get pheromones in the 3 areas
    //     Collider2D[] colllidersForward = Physics2D.OverlapCircleAll(posForward, manager.sampleRadius, 1 << LayerMask.NameToLayer("Pheromone"));
    //     Collider2D[] collidersLeft = Physics2D.OverlapCircleAll(posLeft, manager.sampleRadius, 1 << LayerMask.NameToLayer("Pheromone"));
    //     Collider2D[] collidersRight = Physics2D.OverlapCircleAll(posRight, manager.sampleRadius, 1 << LayerMask.NameToLayer("Pheromone"));

    //     //if the ant has food, it will try to go to the nest
    //     float forwardStrength = 0;
    //     foreach (Collider2D collider in colllidersForward)
    //     {
    //         //if the collider is not a pheromone, skip it
    //         Pheromone p = collider.GetComponent<Pheromone>();
    //         if (p.type == (hasFood ? Pheromone.Type.Home : Pheromone.Type.Food))
    //         {
    //             forwardStrength += p.strength;
    //         }
    //     }

    //     float leftStrength = 0;
    //     foreach (Collider2D collider in collidersLeft)
    //     {
    //         Pheromone p = collider.GetComponent<Pheromone>();
    //         if (p.type == (hasFood ? Pheromone.Type.Home : Pheromone.Type.Food))
    //         {
    //             leftStrength += p.strength;
    //         }
    //     }

    //     float rightStrength = 0;
    //     foreach (Collider2D collider in collidersRight)
    //     {
    //         Pheromone p = collider.GetComponent<Pheromone>();
    //         if (p.type == (hasFood ? Pheromone.Type.Home : Pheromone.Type.Food))
    //         {
    //             rightStrength += p.strength;
    //         }
    //     }

    //     //add force towards the largest of the 3 pheromone strengths
    //     if (forwardStrength > leftStrength && forwardStrength > rightStrength)
    //     {
    //         sum += manager.pheromoneSteerStrength * (posForward - pos).normalized;
    //     }
    //     else if (leftStrength > forwardStrength && leftStrength > rightStrength)
    //     {
    //         sum += manager.pheromoneSteerStrength * (posLeft - pos).normalized;
    //     }
    //     else if (rightStrength > forwardStrength && rightStrength > leftStrength)
    //     {
    //         sum += manager.pheromoneSteerStrength * (posRight - pos).normalized;
    //     }

    //     return sum;
    // }
}
