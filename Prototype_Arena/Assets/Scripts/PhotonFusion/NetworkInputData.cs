using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;

    public NetworkInputData (float x, float y, float z)
    {
        direction = new Vector3 (x, y, z);
    }
}
