using UnityEngine;

namespace DungeonMaster
{
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Object.FindAnyObjectByType<GameManager>() != null) return;
            var root = new GameObject("GameRoot");
            root.AddComponent<GameManager>();
        }
    }
}
