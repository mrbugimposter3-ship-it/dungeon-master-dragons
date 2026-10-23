using UnityEngine;

namespace DungeonMaster
{
    public class DragonAvatar : MonoBehaviour
    {
        Transform wingL;
        Transform wingR;
        Transform jaw;
        Transform neck;
        ParticleSystem fire;
        float moodUntil;
        bool dead;
        int mood;

        public static DragonAvatar Build(Transform parent, Vector3 localPos)
        {
            var rootGo = new GameObject("Dragon");
            rootGo.transform.SetParent(parent, false);
            rootGo.transform.localPosition = localPos;
            rootGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var avatar = rootGo.AddComponent<DragonAvatar>();
            avatar.Assemble();
            return avatar;
        }

        void Assemble()
        {
            var hide = PrimitiveKit.Mat(new Color(0.28f, 0.06f, 0.05f), 0.25f, 0.42f);
            var belly = PrimitiveKit.Mat(new Color(0.55f, 0.22f, 0.12f), 0.15f, 0.35f);
            var horn = PrimitiveKit.Mat(new Color(0.16f, 0.12f, 0.1f), 0.2f, 0.3f);
            var eye = PrimitiveKit.Mat(new Color(1f, 0.45f, 0.05f), 0f, 0.5f, true, 4f);

            PrimitiveKit.Make(PrimitiveType.Capsule, "Body", transform, new Vector3(0f, 1.7f, -0.2f), Quaternion.Euler(90f, 0f, 0f), new Vector3(1.5f, 1.7f, 1.35f), hide, true);
            PrimitiveKit.Make(PrimitiveType.Sphere, "Belly", transform, new Vector3(0f, 1.35f, -0.1f), new Vector3(1.1f, 0.7f, 2.2f), belly, false);

            neck = new GameObject("Neck").transform;
            neck.SetParent(transform, false);
            neck.localPosition = new Vector3(0f, 2.1f, 1.3f);
            PrimitiveKit.Make(PrimitiveType.Capsule, "NeckMesh", neck, new Vector3(0f, 0.3f, 0.45f), Quaternion.Euler(70f, 0f, 0f), new Vector3(0.55f, 0.7f, 0.55f), hide, false);

            var head = new GameObject("Head").transform;
            head.SetParent(neck, false);
            head.localPosition = new Vector3(0f, 0.55f, 1.15f);
            PrimitiveKit.Make(PrimitiveType.Cube, "Skull", head, Vector3.zero, new Vector3(0.7f, 0.48f, 0.9f), hide, false);
            PrimitiveKit.Make(PrimitiveType.Cube, "Snout", head, new Vector3(0f, -0.05f, 0.7f), new Vector3(0.38f, 0.28f, 0.7f), hide, false);
            jaw = new GameObject("Jaw").transform;
            jaw.SetParent(head, false);
            jaw.localPosition = new Vector3(0f, -0.18f, 0.45f);
            PrimitiveKit.Make(PrimitiveType.Cube, "JawMesh", jaw, new Vector3(0f, 0f, 0.25f), new Vector3(0.32f, 0.1f, 0.55f), belly, false);
            PrimitiveKit.Make(PrimitiveType.Cube, "HornL", head, new Vector3(-0.22f, 0.32f, -0.1f), Quaternion.Euler(0f, 0f, 25f), new Vector3(0.08f, 0.45f, 0.08f), horn, false);
            PrimitiveKit.Make(PrimitiveType.Cube, "HornR", head, new Vector3(0.22f, 0.32f, -0.1f), Quaternion.Euler(0f, 0f, -25f), new Vector3(0.08f, 0.45f, 0.08f), horn, false);
            PrimitiveKit.Make(PrimitiveType.Sphere, "EyeL", head, new Vector3(-0.22f, 0.08f, 0.38f), Vector3.one * 0.1f, eye, false);
            PrimitiveKit.Make(PrimitiveType.Sphere, "EyeR", head, new Vector3(0.22f, 0.08f, 0.38f), Vector3.one * 0.1f, eye, false);

            wingL = Wing("WingL", -1f, hide);
            wingR = Wing("WingR", 1f, hide);

            for (int i = 0; i < 4; i++)
            {
                float side = i < 2 ? -0.55f : 0.55f;
                float z = i % 2 == 0 ? 0.7f : -1.1f;
                PrimitiveKit.Make(PrimitiveType.Capsule, "Leg", transform, new Vector3(side, 0.7f, z), new Vector3(0.28f, 0.55f, 0.28f), hide, false);
            }

            var tail = new GameObject("Tail");
            tail.transform.SetParent(transform, false);
            for (int i = 0; i < 5; i++)
            {
                float s = 0.55f - i * 0.08f;
                PrimitiveKit.Make(PrimitiveType.Sphere, "Tail" + i, tail.transform, new Vector3(0f, 1.5f - i * 0.05f, -1.8f - i * 0.55f), new Vector3(s, s * 0.7f, s), hide, false);
            }

            for (int i = 0; i < 4; i++)
            {
                PrimitiveKit.Make(PrimitiveType.Cube, "Spike", transform, new Vector3(0f, 2.55f, -0.8f + i * 0.55f), new Vector3(0.08f, 0.28f, 0.08f), horn, false);
            }

            var firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(head, false);
            firePoint.localPosition = new Vector3(0f, 0f, 1.1f);
            fire = PrimitiveKit.Embers(firePoint, new Color(1f, 0.45f, 0.08f, 0.9f), 0f, 0.25f, 0.45f, 4f, 0.05f);
        }

        Transform Wing(string name, float sign, Material hide)
        {
            var wing = new GameObject(name).transform;
            wing.SetParent(transform, false);
            wing.localPosition = new Vector3(sign * 0.8f, 2.3f, -0.2f);
            var membrane = PrimitiveKit.Mat(new Color(0.45f, 0.1f, 0.08f, 0.85f), 0.05f, 0.3f, false, 1f, true);
            PrimitiveKit.Make(PrimitiveType.Cube, "Arm", wing, new Vector3(sign * 1.1f, 0.1f, 0.2f), Quaternion.Euler(0f, 0f, sign * -18f), new Vector3(2.2f, 0.08f, 0.12f), hide, false);
            PrimitiveKit.Make(PrimitiveType.Cube, "Membrane", wing, new Vector3(sign * 1.3f, -0.35f, 0f), new Vector3(2.1f, 1.3f, 0.05f), membrane, false);
            return wing;
        }

        public void Flinch()
        {
            if (dead) return;
            mood = 1;
            moodUntil = Time.time + 0.35f;
        }

        public void Breathe()
        {
            if (dead) return;
            mood = 2;
            moodUntil = Time.time + 0.7f;
            if (fire != null)
            {
                var emission = fire.emission;
                emission.rateOverTime = 40f;
            }
        }

        public void Smash()
        {
            if (dead) return;
            mood = 3;
            moodUntil = Time.time + 0.55f;
        }

        public void Die()
        {
            dead = true;
            mood = 4;
        }

        void Update()
        {
            if (!dead && Time.time > moodUntil) mood = 0;
            float t = Time.time;
            if (mood == 4)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0f, 180f, 80f), Time.deltaTime * 1.5f);
                return;
            }

            float bob = Mathf.Sin(t * 1.4f) * 0.08f;
            transform.localPosition = new Vector3(transform.localPosition.x, bob, transform.localPosition.z);

            float flap = Mathf.Sin(t * (mood == 3 ? 8f : 1.6f));
            if (wingL != null) wingL.localRotation = Quaternion.Euler(flap * (mood == 3 ? 50f : 18f), 0f, 12f);
            if (wingR != null) wingR.localRotation = Quaternion.Euler(-flap * (mood == 3 ? 50f : 18f), 0f, -12f);
            if (jaw != null)
            {
                float open = mood == 2 ? 28f : 4f + Mathf.Sin(t * 1.2f) * 2f;
                jaw.localRotation = Quaternion.Euler(open, 0f, 0f);
            }
            if (neck != null)
            {
                float dip = mood == 2 ? 18f : mood == 1 ? -10f : Mathf.Sin(t * 0.8f) * 4f;
                neck.localRotation = Quaternion.Euler(dip, 0f, 0f);
            }
            if (fire != null && mood != 2)
            {
                var emission = fire.emission;
                float rate = emission.rateOverTime.constant;
                if (rate > 0.1f) emission.rateOverTime = Mathf.MoveTowards(rate, 0f, Time.deltaTime * 30f);
            }
        }
    }
}
