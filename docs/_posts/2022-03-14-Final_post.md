---
layout: post
title: "Finalizing the project"
date: 2022-03-14 12:00:00 -0000
---

The final step of the project is to get the ants to follow the trails 
and tweak the parameters until we get a simulation that looks good.

In order to get the ants to follow the trails, we need to sample the 
concentration of pheromones at different locations. In order to do this,
we need to create a function that given the ant's postion, returns the 
concentration of pheromones right in front of the ant, left of the ant,
and right of the ant. Given the each concentration of pheromones in 
each direction, we steer the ant towards the direction with the highest
concentration of pheromones.

```c#
public IEnumerable<Color> GetPheromonesWithinRadius(Vector2 pos, int radius)
    {
        //convert pos to camera space
        pos = Camera.main.WorldToScreenPoint(pos);

        //get the index of the pheromone map
        for (int x = (int)pos.x - radius; x <= pos.x + radius; x++)
        {
            for (int y = (int)pos.y - radius; y <= pos.y + radius; y++)
            {
                int index = PheromoneIndex(x, y);
                if (index >= 0 && index < pheromoneMap.Length && pheromoneMap[index].a > pheromoneDespawnThreshold)
                {
                    yield return pheromoneMap[index];
                }
            }
        }
    }
```

```c#
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
        foreach (Color color in manager.GetPheromonesWithinRadius(posForward, manager.sampleRadius))
        {
            strengthForward += (hasFood ? color.g : color.r) * color.a / 255;
        }

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
       
        return dir.normalized * manager.pheromoneSteerStrength;
    }
```

[Final results](https://youtu.be/OpUZZdKfxb4)

