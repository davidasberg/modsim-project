---
layout: post
title: "Step 1: Creating wandering ants with evasive behavior"
date: 2022-03-07 12:13:00 -0000
---

The first step is to create a wandering ant with an evasive behavior. 
This is done using a simple algorithm, which applies the following directional forces to each ant:  
    
Separation: Separate from other ants with force $F_{sep}$ for each neighbouring ant $p_i$, where $k_{sep}$ is a constant, and $d$ is the distance between the two ants.  
$F_{sep} = k_{sep} * \sum_{P_i \in Adj} \frac{r_{sep} - d}{d}(p_i - p)$

Drag: A drag force is applied to the ant to slow it down. The force is $F_{drag} = -k_{drag}v$ where $k_{drag}$ is a constant and $v$ is the velocity of the ant. 

Random walk: A random walk is applied to the ant to make it move in a random direction. A random point is chosen in the unit circle, and the ant is forced to move towards that point. The force is  
$F_{random} = \frac{(d + r * w)}{\sqrt{(d_x + r_x * w)^2 + (d_y + r_u.y * w)^2} * } * s_{max}$ where $d$ is the previous direction, $r$ is the random direction and $w$ is the wander strength. Lastly, the force is scaled by the maximum speed of the ant.

Bounds: The ant is constrained to the bounds of the world. The force is $F_{bounds} = -k_{bounds}(p - p_b)$ where $k_{bounds}$ is a constant, $p$ is the position of the ant, and $p_b$ is the position of the boundary.

[This video](https://youtu.be/xRWB8DnfkYc) shows how the ants move in the world.
