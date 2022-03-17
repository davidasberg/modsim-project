---
layout: post
title: "Adding trails"
date: 2022-03-09 11:14:00 -0000
---

The next step is adding pheromone trails that are laid down by 
the ants. The will lay one pheromone when they are searching for food,
and another when they are returning to their nest.

I first tried to implement this using game objects, but I ran into
performance issues. I ended up treating each pixel on the screen
as a pheromone with a specific color. When an ant lays down a 
pheromone, it will convert the ants world space coordinates to
screen space coordinates and then change the color of the pixel. 
This was much faster than traditional game objects. 

```c#
    public void SpawnPheromone(Ant ant)
    {
        Vector2 spawnPoint = ant.transform.position;

        //convert to camera coordinates
        spawnPoint = Camera.main.WorldToScreenPoint(spawnPoint);

        //get all points within radius
        for (int x = (int)spawnPoint.x - pheromoneSize; x <= (int)spawnPoint.x + pheromoneSize; ++x)
        {
            for (int y = (int)spawnPoint.y - pheromoneSize; y <= (int)spawnPoint.y + pheromoneSize; ++y)
            {
                if (Vector2.Distance(new Vector2(x, y), spawnPoint) <= pheromoneSize
                    && x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                {
                    int index = PheromoneIndex(x, y);

                    //set color based on food status
                    if (ant.hasFood)
                    {
                        pheromoneMap[index] += Color.red;
                    }
                    else
                    {
                        pheromoneMap[index] += Color.green;
                    }
                    pheromoneMap[index].a = 1;
                }
            }
        }
    }
```

I then update the alpha channel each iteration to make the pheromone
fade away over time. I also added a threshold to make sure that the
pheromone does not need to be updated if it is below a certain value.
This update follows the exponential decay equation:
$S = S_0 * e^{-\lambda * t}$

```c#
   void UpdatePheromones()
    {
        //use exponential decay for each pixel in the pheromone map
        for (int x = 0; x < pheromoneMap.Length; x++)
        {
            if (pheromoneMap[x].a >= pheromoneDespawnThreshold)
            {
                pheromoneMap[x].a *= pheromoneDecay;
            }
        }
    }
```