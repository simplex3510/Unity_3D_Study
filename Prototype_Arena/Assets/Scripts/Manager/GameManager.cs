using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manager
{
    public class GameManager : Singleton<GameManager>
    {
        public GameObject mobPrefab;

        public List<Transform> spawnPointList;

        public List<BlackKnight> spawnedMobList;
        public Queue<BlackKnight> respawnMobQueue;

        private void Awake()
        {
            spawnedMobList = new List<BlackKnight>();
            respawnMobQueue = new Queue<BlackKnight>();

            StartCoroutine(SpawnMob());
        }

        private IEnumerator SpawnMob()
        {
            while (true)
            {
                float delay = 3.0f;
                float curDelay = 0.0f;
                while (curDelay <= delay)
                {
                    curDelay += Time.deltaTime;
                    yield return null;
                }

                int randCoor = Random.Range(0, spawnPointList.Count);
                GameObject mob = Instantiate(mobPrefab);
                mob.transform.position = spawnPointList[randCoor].position;
            
                yield return null;
            }

        }
    }
}
