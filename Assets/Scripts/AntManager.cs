using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntManager : MonoBehaviour
{
    [SerializeField]
    private Ant antPrefab;

    [SerializeField]
    private Pheromone pheromonePrefab;

    [SerializeField]
    private Food foodPrefab;

    [SerializeField]
    private BoxCollider2D bounds;

    [SerializeField]
    private float boundsForceFactor = 5;

    [SerializeField]
    private float spawnRadius = 10;


    [SerializeField]
    private int antCount = 10;

    [SerializeField]
    public float separationForceFactor = 1;

    [SerializeField]
    public float neighborRadius = 2;

    [SerializeField]
    public float dragForce = 1;

    [SerializeField]
    public float maxSpeed = 5;

    [SerializeField]
    public float minSpeed = 1;

    [SerializeField]
    public float wanderStrength = 1;

    [SerializeField]
    public float pheromoneSteerStrength = 1;

    [SerializeField]
    public float pheromoneSpawnInterval = 1;
    [SerializeField]
    public int pheromoneSize = 3;
    [SerializeField]
    public float pheromoneDecay = 0.8f;
    [SerializeField]
    public float pheromoneDespawnThreshold = 0.1f;

    [SerializeField]
    public int sampleRadius = 10;

    [SerializeField]
    public float visionRange = 1f;

    [SerializeField]
    public float visionStrength = 1f;

    [SerializeField]
    public int foodCountPerCluster = 100;
    [SerializeField]
    public int foodClusters = 10;
    [SerializeField]
    public float foodClusterRadius = 5;
    [SerializeField]
    public float foodDistance = 1;
    [SerializeField]
    public float pickupRadius = 0.1f;

    public int foodCollected = 0;

    private TextMesh foodCounterText;

    private List<Ant> ants;

    [SerializeField]
    public Color[] pheromoneMap;

    private Texture2D pheromoneTexture;
    private int mapWidth, mapHeight; //in pixels

    

    void Start()
    {

        //get box collider bounds

        ants = new List<Ant>();

        SpawnFood();
        //SpawnPheromones();

        //set size to be same as camera resolution
        mapWidth = (int)Camera.main.pixelWidth;
        mapHeight = (int)Camera.main.pixelHeight;
        pheromoneMap = new Color[mapWidth * mapHeight];
        pheromoneTexture = new Texture2D(mapWidth, mapHeight);
        pheromoneTexture.wrapMode = TextureWrapMode.Mirror;
        pheromoneTexture.filterMode = FilterMode.Point;
    
        //set main renderer
        transform.GetChild(2).GetComponent<Renderer>().material.mainTexture = pheromoneTexture;
        foodCounterText = transform.GetChild(1).GetComponent<TextMesh>();
    }

    void FixedUpdate()
    {
        //spawn ants until we have the desired number
        if (ants.Count < antCount)
        {
            SpawnAnt();
        }

        //update pheromone map
        UpdatePheromones();
    }

    void Update()
    {
        foodCounterText.text = foodCollected.ToString();
        //update pheromone map
        pheromoneTexture.SetPixels(pheromoneMap);
        pheromoneTexture.Apply();
    }


    public void SpawnAnt()
    {

        Vector2 spawnPoint = transform.position + spawnRadius * Random.insideUnitSphere;

        for (int j = 0; j < 2; ++j)
            spawnPoint[j] = Mathf.Clamp(spawnPoint[j], bounds.bounds.min[j], bounds.bounds.max[j]);

        Ant ant = Instantiate(antPrefab, new Vector3(spawnPoint.x, spawnPoint.y, 0), antPrefab.transform.rotation) as Ant;
        ant.pos = spawnPoint;
        ant.manager = this;
        ant.vel = Random.insideUnitCircle * maxSpeed;
        ant.hasFood = false;
        ants.Add(ant);

    }

    // public void SpawnPheromones() {
    //     for (int i = 0; i < maxPheromones; ++i) {
    //         Pheromone p = Instantiate(pheromonePrefab, new Vector3(0, 0, 1), pheromonePrefab.transform.rotation) as Pheromone;
    //         p.strength = 0;
    //         p.manager = this;
    //         pheromones.Enqueue(p);
    //     }
    // }

    // public void SpawnPheromone(Ant ant) {
    //     Vector2 spawnPoint = ant.transform.position;

    //     Pheromone pheromone;
    //     if (pheromones.Count >= maxPheromones) {
    //         pheromone = pheromones.Dequeue();
    //     } else {
    //         pheromone = Instantiate(pheromonePrefab, new Vector3(spawnPoint.x, spawnPoint.y, 1), pheromonePrefab.transform.rotation) as Pheromone;
    //     }
    //     pheromone.pos = spawnPoint;
    //     pheromone.manager = this;
    //     pheromone.strength = 1;
    //     pheromone.type = ant.hasFood ? Pheromone.Type.Food : Pheromone.Type.Home;
    //     pheromones.Enqueue(pheromone);
    // }


    int PheromoneIndex(int x, int y)
    {
        return x + y * mapWidth;
    }

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

    public void SpawnFood()
    {
        for (int i = 0; i < foodClusters; i++)
        {
            //pick random point in circle
            Vector2 spawnPoint = Vector2.zero;
            while (spawnPoint.magnitude < foodDistance - 1)
            {
                spawnPoint = (Vector2)transform.position + foodDistance * Random.insideUnitCircle;
            }
            for (int j = 0; j < 2; ++j)
                spawnPoint[j] = Mathf.Clamp(spawnPoint[j], bounds.bounds.min[j], bounds.bounds.max[j]);

            for (int j = 0; j < foodCountPerCluster; j++)
            {
                //pick random point from spawn point within circle
                Vector2 foodPoint = spawnPoint + Random.insideUnitCircle * Random.Range(0.1f, foodClusterRadius);
                foodPoint.x = Mathf.Clamp(foodPoint.x, bounds.bounds.min.x, bounds.bounds.max.x);
                foodPoint.y = Mathf.Clamp(foodPoint.y, bounds.bounds.min.y, bounds.bounds.max.y);

                //instantiate food
                Instantiate(foodPrefab, new Vector3(foodPoint.x, foodPoint.y, 0), foodPrefab.transform.rotation);
            }
        }
    }

    public Vector2 GetBoundaryForce(Ant ant)
    {
        //add larger force to the opposite direction depending on the distance from boundary
        Vector2 force = new Vector2();
        Vector2 centerToPos = ant.pos - (Vector2)(transform.position);
        Vector2 minDiff = centerToPos + bounds.size * 0.5f;
        Vector2 maxDiff = centerToPos - bounds.size * 0.5f;
        float friction = 0.0f;

        for (int i = 0; i < 2; ++i)
        {
            if (minDiff[i] < 0)
                force[i] = minDiff[i];
            else if (maxDiff[i] > 0)
                force[i] = maxDiff[i];
            else
                force[i] = 0;

            friction += Mathf.Abs(force[i]);
        }

        force += friction * (Vector2)ant.vel;
        return -boundsForceFactor * force;
    }

    public IEnumerable<Ant> GetNeighbours(Ant ant)
    {
        //get ant collider
        Collider2D antCollider = ant.GetComponentInChildren<Collider2D>();
        //get collider of child 
        Collider2D nestCollider = transform.GetComponentInChildren<Collider2D>();

        //if antcollider and nestcollider collides

        Collider2D[] colliders = Physics2D.OverlapCircleAll(ant.pos, neighborRadius, 1 << LayerMask.NameToLayer("Ant"));
        foreach (Collider2D collider in colliders)
        {
            Ant other = collider.GetComponentInParent<Ant>();
            if (other != ant && other != null)
                yield return other;
        }

    }

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

}
