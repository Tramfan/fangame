using UnityEngine;

[DisallowMultipleComponent]
public sealed class MenuScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject firstSelectedObject;

    public GameObject FirstSelectedObject =>
        firstSelectedObject;
}