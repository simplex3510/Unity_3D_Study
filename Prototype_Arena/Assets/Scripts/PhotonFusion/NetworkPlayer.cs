using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    private NetworkCharacterControllerPrototype _netCharCntl;

    private void Awake()
    {
        _netCharCntl = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _netCharCntl.Move(5 * data.direction * Runner.DeltaTime);
        }
    }
}
