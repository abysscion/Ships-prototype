using UnityEngine;

/// <summary>
/// Don't destroy on load hack
/// </summary>
public class DDOL : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
