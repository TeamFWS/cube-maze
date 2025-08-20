using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static event Action OnEndReached;

    public static void EndReached()
    {
        OnEndReached?.Invoke();
    }
}