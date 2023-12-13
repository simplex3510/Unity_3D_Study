using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Manager
{
    public class GameManager : Singleton<GameManager>
    {
        public List<Transform> spawnPointList;

        public List<BlackKnight> spawnedList;
        public Queue<BlackKnight> respawnQueue;

        public GameObject mobPrefab;

        private void Awake()
        {
            respawnQueue = new Queue<BlackKnight>();
        }
    }
}
