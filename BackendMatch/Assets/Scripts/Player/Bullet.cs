using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    private bool isLive = false;
    private Vector3 dir = new Vector3(0, 0, 0);
    private float speed = 20.0f;

    public void Active(Vector3 pos, Vector3 dir)
    {
        isLive = true;
        this.dir = dir;
        this.transform.position = pos;
        this.gameObject.SetActive(true);
    }

    public bool IsLive()
    {
        return isLive;
    }

    void Update()
    {
        if (isLive)
        {
            this.transform.position += (dir * Time.deltaTime * speed);

            if (this.transform.position.x > 50 || this.transform.position.x < -50 ||
            this.transform.position.z > 50 || this.transform.position.z < -50)
            {
                isLive = false;
                this.gameObject.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            isLive = false;
            this.gameObject.SetActive(false);
            if (BackEndMatchManager.GetInstance().IsHost() == false)
            {
                return;
            }
            Player tmp = collider.gameObject.GetComponent<Player>();
            if (tmp)
            {
                Protocol.PlayerDamegedMessage message =
                    new Protocol.PlayerDamegedMessage(tmp.GetIndex(), this.transform.position.x, this.transform.position.y, this.transform.position.z);
                BackEndMatchManager.GetInstance().SendDataToInGame<Protocol.PlayerDamegedMessage>(message);
            }
        }
        else
        {
            var mapTransform = collider.gameObject.transform.parent.parent;
            if (mapTransform == null)
            {
                // 총알끼리 부딪혔을 때 여기서 바로 리턴됨
                return;
            }
            if (collider.gameObject.transform.parent.parent.CompareTag("Map"))
            {
                isLive = false;
                this.gameObject.SetActive(false);

                EffectManager.instance.EnableEffect(this.transform.position.x, this.transform.position.y, this.transform.position.z);
            }
        }
    }
}
