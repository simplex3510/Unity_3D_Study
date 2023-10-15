using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{

    static public BulletManager Instance;
    public GameObject bulletPrefeb;
    private GameObject bulletPool;
    private int MAX_BULLET = 150;
    private int nowIndex = 0;

    List<Bullet> bullets;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        Vector3 initPos = new Vector3(0, 0, 0);
        bulletPool = this.gameObject;
        bullets = new List<Bullet>();
        for (int i = 0; i < MAX_BULLET; ++i)
        {
            GameObject bullet = Instantiate(bulletPrefeb, initPos, Quaternion.identity, bulletPool.transform);
            bullets.Add(bullet.GetComponent<Bullet>());
            bullet.SetActive(false);
        }
    }

    public void ShootBullet(Vector3 pos, Vector3 dir)
    {
        if (nowIndex >= MAX_BULLET)
        {
            nowIndex = 0;
            bool result = false;
            for (int i = 0; i < MAX_BULLET; ++i)
            {
                if (bullets[i].IsLive())
                {
                    nowIndex = i;
                    result = true;
                    break;
                }
            }
            if (!result)
            {
                return;
            }
        }
        bullets[nowIndex].Active(pos, dir);
        nowIndex++;
    }
}
