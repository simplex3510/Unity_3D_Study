using UnityEngine;

public class BringToFront : MonoBehaviour {

    private void OnEnable()
    {
        transform.SetAsLastSibling();
    }
}
