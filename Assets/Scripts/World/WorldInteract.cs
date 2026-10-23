using System.Collections.Generic;
using UnityEngine;

namespace DungeonMaster
{
    public enum InteractKind
    {
        Torch,
        Pickup,
        Story,
        Basilisk,
        Lava,
        Brazier,
        Raft,
        Gate,
        Chest
    }

    public class WorldInteract : MonoBehaviour
    {
        public static readonly List<WorldInteract> All = new List<WorldInteract>();

        public InteractKind kind;
        public ItemId item;
        public string prompt = "Interact";
        public Color promptColor = new Color(0.93f, 0.88f, 0.78f, 1f);

        void OnEnable()
        {
            if (!All.Contains(this)) All.Add(this);
        }

        void OnDisable()
        {
            All.Remove(this);
        }
    }

    public class Bob : MonoBehaviour
    {
        public float amplitude = 0.12f;
        public float speed = 2f;
        Vector3 origin;
        bool ready;

        void OnEnable()
        {
            origin = transform.localPosition;
            ready = true;
        }

        void Update()
        {
            if (!ready) return;
            var p = origin;
            p.y += Mathf.Sin(Time.time * speed + transform.position.x) * amplitude;
            transform.localPosition = p;
        }
    }

    public class LavaStream : MonoBehaviour
    {
        public float speed = 2.4f;
        public float length = 6f;
        Vector3 origin;

        void OnEnable()
        {
            origin = transform.localPosition;
        }

        void Update()
        {
            var p = transform.localPosition;
            p.y -= speed * Time.deltaTime;
            if (origin.y - p.y > length) p.y = origin.y;
            transform.localPosition = p;
        }
    }

    public class TorchGate : MonoBehaviour
    {
        public Vector3 push = Vector3.back;

        void OnTriggerStay(Collider other)
        {
            var explorer = other.GetComponentInParent<ExplorerController>();
            if (explorer == null) return;
            var game = GameManager.Instance;
            if (game == null || game.State != PlayState.Playing) return;
            if (game.Inventory != null && game.Inventory.TorchLit) return;
            var cc = explorer.GetComponent<CharacterController>();
            if (cc != null) cc.Move(push.normalized * 6f * Time.deltaTime);
            game.Hud.Flash("You need a torch to enter.");
        }
    }
}
