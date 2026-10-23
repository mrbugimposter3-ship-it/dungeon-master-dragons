using UnityEngine;

namespace DungeonMaster
{
    public class ExplorerController : MonoBehaviour
    {
        public float walkSpeed = 6.3f;
        public float sprintSpeed = 10.4f;
        public float lookSensitivity = 3.2f;
        public bool firstPerson;
        public float yaw;
        public float pitch = 10f;
        public bool suppressTorchLight;

        CharacterController controller;
        float verticalVelocity;
        Transform visual;
        Transform gearRoot;
        Transform torchRoot;
        Light torchLight;
        Light torchSpot;

        public Transform CameraPivot { get; private set; }

        public void BuildBody()
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.38f;
            controller.center = new Vector3(0f, 0.92f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 48f;
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0) gameObject.layer = playerLayer;

            CameraPivot = new GameObject("CameraPivot").transform;
            CameraPivot.SetParent(transform, false);
            CameraPivot.localPosition = new Vector3(0f, 1.55f, 0f);

            visual = new GameObject("Body").transform;
            visual.SetParent(transform, false);
            var cloth = PrimitiveKit.Mat(new Color(0.32f, 0.2f, 0.16f), 0.05f, 0.25f);
            var skin = PrimitiveKit.Mat(new Color(0.62f, 0.45f, 0.34f), 0f, 0.3f);
            PrimitiveKit.Make(PrimitiveType.Capsule, "Torso", visual, new Vector3(0f, 0.95f, 0f), new Vector3(0.5f, 0.52f, 0.36f), cloth, false);
            PrimitiveKit.Make(PrimitiveType.Sphere, "Head", visual, new Vector3(0f, 1.62f, 0f), Vector3.one * 0.3f, skin, false);

            gearRoot = new GameObject("Gear").transform;
            gearRoot.SetParent(visual, false);
            torchRoot = new GameObject("TorchHand").transform;
            torchRoot.SetParent(visual, false);
            torchRoot.localPosition = new Vector3(-0.36f, 1.12f, 0.32f);

            if (GameManager.Instance != null && GameManager.Instance.Inventory != null)
                GameManager.Instance.Inventory.Changed += RefreshGear;
            RefreshGear();
        }

        public void RefreshGear()
        {
            Clear(gearRoot);
            Clear(torchRoot);
            torchLight = null;
            torchSpot = null;
            var inv = GameManager.Instance != null ? GameManager.Instance.Inventory : null;
            if (inv == null) return;

            if (inv.Has(ItemId.ClothArmour))
            {
                var mat = PrimitiveKit.Mat(ItemCatalog.Get(ItemId.ClothArmour).color, 0.05f, 0.2f);
                PrimitiveKit.Make(PrimitiveType.Cube, "Cloth", gearRoot, new Vector3(0f, 1.02f, 0f), new Vector3(0.58f, 0.62f, 0.4f), mat, false);
            }
            if (inv.Has(ItemId.Kiteshield))
            {
                var mat = PrimitiveKit.Mat(ItemCatalog.Get(ItemId.Kiteshield).color, 0.4f, 0.55f);
                PrimitiveKit.Make(PrimitiveType.Cube, "Shield", gearRoot, new Vector3(0.4f, 1.05f, 0.08f), new Vector3(0.08f, 0.65f, 0.42f), mat, false);
            }
            if (inv.Has(ItemId.AntiFireCape))
            {
                var mat = PrimitiveKit.Mat(ItemCatalog.Get(ItemId.AntiFireCape).color, 0.1f, 0.35f, true, 0.45f);
                PrimitiveKit.Make(PrimitiveType.Cube, "Cape", gearRoot, new Vector3(0f, 1.05f, -0.26f), new Vector3(0.5f, 0.85f, 0.05f), mat, false);
            }

            if (inv.Has(ItemId.IceWhip)) Whip();
            else if (inv.Has(ItemId.IronSword)) Sword();
            else if (inv.Has(ItemId.MithrilAxe)) Axe();

            if (inv.TorchLit && !suppressTorchLight) Torch();
        }

        public void SetTorchSuppressed(bool suppressed)
        {
            suppressTorchLight = suppressed;
            RefreshGear();
        }

        static void Clear(Transform root)
        {
            if (root == null) return;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i).gameObject;
                if (Application.isPlaying) Object.DestroyImmediate(child);
                else Object.Destroy(child);
            }
        }

        void Sword()
        {
            var hand = new GameObject("Sword").transform;
            hand.SetParent(gearRoot, false);
            hand.localPosition = new Vector3(0.36f, 1.0f, 0.25f);
            hand.localRotation = Quaternion.Euler(8f, 0f, -18f);
            PrimitiveKit.Make(PrimitiveType.Cube, "Blade", hand, new Vector3(0f, 0.42f, 0f), new Vector3(0.05f, 0.62f, 0.1f), PrimitiveKit.Mat(new Color(0.8f, 0.8f, 0.84f), 0.8f, 0.65f), false);
            PrimitiveKit.Make(PrimitiveType.Cylinder, "Grip", hand, Vector3.zero, new Vector3(0.05f, 0.1f, 0.05f), PrimitiveKit.Mat(new Color(0.28f, 0.16f, 0.1f)), false);
        }

        void Axe()
        {
            var hand = new GameObject("Axe").transform;
            hand.SetParent(gearRoot, false);
            hand.localPosition = new Vector3(0.38f, 0.95f, 0.22f);
            PrimitiveKit.Make(PrimitiveType.Cylinder, "Haft", hand, new Vector3(0f, 0.3f, 0f), new Vector3(0.045f, 0.32f, 0.045f), PrimitiveKit.Mat(new Color(0.32f, 0.2f, 0.12f)), false);
            PrimitiveKit.Make(PrimitiveType.Cube, "Head", hand, new Vector3(0.1f, 0.62f, 0f), new Vector3(0.22f, 0.14f, 0.07f), PrimitiveKit.Mat(new Color(0.65f, 0.88f, 0.95f), 0.85f, 0.7f, true, 0.4f), false);
        }

        void Whip()
        {
            var hand = new GameObject("Whip").transform;
            hand.SetParent(gearRoot, false);
            hand.localPosition = new Vector3(0.34f, 1.15f, 0.28f);
            var ice = PrimitiveKit.Mat(new Color(0.5f, 0.95f, 1f), 0.2f, 0.8f, true, 0.9f);
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                PrimitiveKit.Make(PrimitiveType.Sphere, "Link", hand, new Vector3(0.04f, -t * 0.32f, t * 0.16f), Vector3.one * 0.06f, ice, false);
            }
        }

        void Torch()
        {
            PrimitiveKit.Make(PrimitiveType.Cylinder, "Stick", torchRoot, new Vector3(0f, 0.16f, 0f), new Vector3(0.035f, 0.2f, 0.035f), PrimitiveKit.Mat(new Color(0.35f, 0.2f, 0.1f)), false);
            PrimitiveKit.Make(PrimitiveType.Sphere, "Flame", torchRoot, new Vector3(0f, 0.4f, 0f), Vector3.one * 0.09f, PrimitiveKit.Mat(new Color(1f, 0.55f, 0.12f), 0f, 0.4f, true, 3.2f), false);
            torchLight = PrimitiveKit.AddLight(torchRoot, new Vector3(0f, 0.42f, 0f), new Color(1f, 0.64f, 0.3f), 14f, 16f);
            torchSpot = PrimitiveKit.AddLight(torchRoot, new Vector3(0f, 0.38f, 0.05f), new Color(1f, 0.7f, 0.4f), 18f, 20f, LightType.Spot);
            torchSpot.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            torchSpot.spotAngle = 68f;
            PrimitiveKit.Embers(torchRoot, new Color(1f, 0.5f, 0.15f, 0.85f), 7f, 0.04f, 0.55f, 0.35f, 0.04f);
        }

        void Update()
        {
            var game = GameManager.Instance;
            if (game == null || game.State != PlayState.Playing || controller == null) return;
            Look();
            Move();
            Flicker();
        }

        void Look()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;
            yaw += Input.GetAxis("Mouse X") * lookSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            pitch = Mathf.Clamp(pitch, -32f, 68f);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (CameraPivot != null) CameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        void Move()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 wish = transform.forward * v + transform.right * h;
            if (wish.sqrMagnitude > 1f) wish.Normalize();
            float speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? sprintSpeed : walkSpeed;
            if (controller.isGrounded)
            {
                verticalVelocity = -1.4f;
                if (Input.GetKeyDown(KeyCode.Space)) verticalVelocity = 5.1f;
            }
            else
            {
                verticalVelocity += -20f * Time.deltaTime;
            }
            var velocity = wish * speed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
            if (transform.position.y < -6f)
                transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z);
        }

        void Flicker()
        {
            if (torchLight == null) return;
            float n = Mathf.PerlinNoise(Time.time * 8.5f, 1.7f);
            torchLight.intensity = 11f + n * 6f;
            if (torchSpot != null) torchSpot.intensity = 14f + n * 7f;
        }

        public void PlaceCamera(Camera cam)
        {
            if (cam == null || CameraPivot == null) return;
            int playerLayer = LayerMask.NameToLayer("Player");
            int mask = Physics.DefaultRaycastLayers;
            if (playerLayer >= 0) mask &= ~(1 << playerLayer);

            if (firstPerson)
            {
                SetBodyVisible(false);
                cam.transform.position = CameraPivot.position;
                cam.transform.rotation = CameraPivot.rotation;
                return;
            }

            SetBodyVisible(true);
            Vector3 origin = CameraPivot.position;
            Vector3 desired = CameraPivot.TransformPoint(new Vector3(0.55f, 0.28f, -4.5f));
            Vector3 delta = desired - origin;
            float dist = delta.magnitude;
            if (dist > 0.05f && Physics.Raycast(origin, delta / dist, out RaycastHit hit, dist, mask, QueryTriggerInteraction.Ignore))
                desired = hit.point + hit.normal * 0.2f;
            cam.transform.position = desired;
            Vector3 lookTarget = CameraPivot.position + CameraPivot.forward * 1.5f;
            Vector3 lookDir = lookTarget - desired;
            if (lookDir.sqrMagnitude > 0.001f)
                cam.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }

        void SetBodyVisible(bool visible)
        {
            if (visual == null) return;
            var renderers = visual.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                bool torchPart = torchRoot != null && renderers[i].transform.IsChildOf(torchRoot);
                renderers[i].enabled = visible || torchPart;
            }
        }

        public void FaceYaw(float newYaw)
        {
            yaw = newYaw;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.collider != null && hit.collider.CompareTag("Boundary"))
            {
                var game = GameManager.Instance;
                if (game != null && game.Hud != null)
                    game.Hud.Flash("The way is blocked by molten rocks.");
            }
        }
    }
}
