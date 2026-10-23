using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonMaster
{
    public enum PlayState
    {
        Menu,
        Playing,
        Inventory,
        Journal,
        HowTo,
        Paused,
        Confirm,
        Narration,
        Cutscene,
        Combat,
        Ended
    }

    public enum EndingKind
    {
        Win,
        Death,
        Escape
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static bool pendingAutoStart;

        public Inventory Inventory { get; private set; }
        public HudController Hud { get; private set; }
        public DragonFight Fight { get; private set; }
        public ExplorerController Player { get; private set; }
        public WorldRefs Refs { get; private set; }
        public PlayState State { get; private set; }
        public int Score;
        public float Darkness01 { get; private set; }

        Camera cam;
        float orbit = 1.2f;
        float trauma;
        float scoreAcc;
        float darkness;
        bool runStarted;
        bool storyShown;
        bool capeSpawned;
        bool brazierDoused;
        int dropCount;
        bool cinematic;
        PlayState returnState = PlayState.Menu;
        System.Action pendingYes;

        const float InteractRange = 3.5f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ConfigureAtmosphere();
            Inventory = gameObject.AddComponent<Inventory>();
            gameObject.AddComponent<SoundBoard>();
            var world = new GameObject("Island");
            var builder = world.AddComponent<IslandBuilder>();
            builder.Build();
            Refs = builder.refs;
            Player = Refs.player;
            cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                camGo.AddComponent<AudioListener>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.02f, 0.018f);
            cam.nearClipPlane = 0.08f;
            cam.farClipPlane = 450f;
            cam.fieldOfView = 62f;
            AssignWorldCanvases();

            Fight = gameObject.AddComponent<DragonFight>();
            Fight.avatar = Refs.dragon;
            Hud = gameObject.AddComponent<HudController>();
            Hud.Build();
            Score = 10;
            if (pendingAutoStart)
            {
                pendingAutoStart = false;
                StartGame();
            }
            else
            {
                EnterMenu();
            }
        }

        void ConfigureAtmosphere()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.07f, 0.025f, 0.02f);
            RenderSettings.fogDensity = 0.011f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.07f, 0.045f, 0.05f);
            RenderSettings.ambientEquatorColor = new Color(0.05f, 0.025f, 0.02f);
            RenderSettings.ambientGroundColor = new Color(0.02f, 0.01f, 0.008f);

            var moonGo = new GameObject("Moon");
            var moon = moonGo.AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.62f, 0.68f, 0.82f);
            TryLux(moon);
            moon.intensity = 0.28f;
            moon.shadows = LightShadows.Soft;
            moonGo.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
        }

        static void TryLux(Light light)
        {
            var prop = typeof(Light).GetProperty("lightUnit");
            if (prop == null) return;
            try { prop.SetValue(light, System.Enum.Parse(prop.PropertyType, "Lux"), null); }
            catch (System.Exception) { }
        }

        void AssignWorldCanvases()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i].renderMode == RenderMode.WorldSpace)
                    canvases[i].worldCamera = cam;
            }
        }

        void Update()
        {
            if (runStarted && State != PlayState.Menu && State != PlayState.Paused && State != PlayState.Ended)
            {
                scoreAcc += Time.deltaTime;
                while (scoreAcc >= 1f)
                {
                    scoreAcc -= 1f;
                    Score += 2;
                }
            }

            if (TicksDark())
            {
                if (Inventory.TorchLit)
                {
                    darkness = Mathf.MoveTowards(darkness, 0f, Time.deltaTime * 0.8f);
                }
                else
                {
                    darkness += Time.deltaTime / 10f;
                    if (darkness >= 1f)
                    {
                        Finish(EndingKind.Death, "The dark", StoryText.DarknessBody, false);
                    }
                }
            }
            Darkness01 = Inventory != null && Inventory.TorchLit ? 0f : Mathf.Clamp01(darkness);
            RenderSettings.fogDensity = Mathf.Lerp(0.011f, 0.07f, Darkness01);
            if (SoundBoard.Instance != null)
                SoundBoard.Instance.SetTorchVolume(Inventory != null && Inventory.TorchLit && !cinematic ? 0.14f : 0f);

            if (State == PlayState.Playing)
            {
                TrackArea();
                var near = Nearest();
                if (near != null) Hud.SetPrompt(near.prompt, near.promptColor);
                else Hud.SetPrompt(Inventory.TorchLit ? "" : StoryText.PitchBlack, new Color(1f, 0.7f, 0.4f));
                if (Input.GetKeyDown(KeyCode.E) && near != null) Use(near);
                if (Input.GetKeyDown(KeyCode.I)) OpenInventory();
                if (Input.GetKeyDown(KeyCode.J)) OpenJournal();
                if (Input.GetKeyDown(KeyCode.V) && Player != null) Player.firstPerson = !Player.firstPerson;
                if (Input.GetKeyDown(KeyCode.M)) Hud.ToggleMap();
                if (Input.GetKeyDown(KeyCode.Escape)) Pause();
                if (Cursor.lockState != CursorLockMode.Locked && Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            else if (State == PlayState.Combat)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) FightMove("ww");
                if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) FightMove("dh");
                if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) FightMove("cs");
                if (Input.GetKeyDown(KeyCode.Escape)) Pause();
            }
            else if (State == PlayState.Inventory || State == PlayState.Journal || State == PlayState.HowTo)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.I) && State == PlayState.Inventory || Input.GetKeyDown(KeyCode.J) && State == PlayState.Journal)
                    CloseOverlay();
            }
            else if (State == PlayState.Paused && Input.GetKeyDown(KeyCode.Escape))
            {
                Resume();
            }
            else if (State == PlayState.Confirm && Input.GetKeyDown(KeyCode.Escape))
            {
                ConfirmNo();
            }
            else if (State == PlayState.Narration && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E)))
            {
                ContinueNarration();
            }
            else if (State == PlayState.Menu && Input.GetKeyDown(KeyCode.Return))
            {
                StartGame();
            }
        }

        void LateUpdate()
        {
            if (cam == null) return;
            if (State == PlayState.Menu || (State == PlayState.HowTo && !runStarted))
                Orbit();
            else if (cinematic)
                CombatCam();
            else if (Player != null)
                Player.PlaceCamera(cam);

            if (trauma > 0f)
            {
                float s = trauma * trauma * 0.45f;
                cam.transform.position += new Vector3(
                    (Mathf.PerlinNoise(Time.unscaledTime * 35f, 0f) - 0.5f) * s,
                    (Mathf.PerlinNoise(0f, Time.unscaledTime * 35f) - 0.5f) * s,
                    0f);
                trauma = Mathf.Max(0f, trauma - Time.unscaledDeltaTime * 1.3f);
            }
        }

        public void Shake(float amount)
        {
            trauma = Mathf.Clamp01(trauma + amount);
        }

        void Orbit()
        {
            orbit += Time.unscaledDeltaTime * 0.12f;
            Vector3 focus = Refs != null && Refs.volcano != null ? Refs.volcano.position : new Vector3(0f, 10f, 120f);
            cam.transform.position = focus + new Vector3(Mathf.Sin(orbit) * 74f, 34f, Mathf.Cos(orbit) * 74f);
            cam.transform.LookAt(focus + Vector3.up * 14f);
        }

        void CombatCam()
        {
            if (Refs == null || Refs.dragon == null) return;
            Vector3 focus = Refs.dragon.transform.position + Vector3.up * 2.4f;
            cam.transform.position = focus + new Vector3(3.2f, 4.2f, -13f);
            cam.transform.LookAt(focus);
        }

        public void EnterMenu()
        {
            State = PlayState.Menu;
            cinematic = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Hud.Apply(State);
        }

        public void StartGame()
        {
            runStarted = true;
            Score = 10;
            scoreAcc = 0f;
            darkness = 0f;
            State = PlayState.Playing;
            cinematic = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (Player != null)
            {
                Player.FaceYaw(0f);
                Player.transform.position = new Vector3(0f, 0.2f, 0f);
            }
            Hud.Apply(State);
            Hud.SetArea("Volcanic Wilds");
            Hud.Banner(StoryText.PitchBlack);
        }

        public void Pause()
        {
            returnState = State == PlayState.Combat ? PlayState.Combat : PlayState.Playing;
            State = PlayState.Paused;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Hud.Apply(State);
        }

        public void Resume()
        {
            Time.timeScale = 1f;
            State = returnState == PlayState.Combat ? PlayState.Combat : PlayState.Playing;
            if (State == PlayState.Playing)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            Hud.Apply(State);
        }

        public void OpenInventory()
        {
            returnState = PlayState.Playing;
            State = PlayState.Inventory;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Hud.Apply(State);
        }

        public void OpenJournal()
        {
            returnState = State == PlayState.Menu ? PlayState.Menu : PlayState.Playing;
            State = PlayState.Journal;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Hud.Apply(State);
        }

        public void OpenHowTo()
        {
            returnState = State == PlayState.Paused ? PlayState.Paused : (runStarted && State != PlayState.Menu ? PlayState.Playing : PlayState.Menu);
            if (State == PlayState.Paused) returnState = PlayState.Paused;
            State = PlayState.HowTo;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Hud.Apply(State);
        }

        public void CloseOverlay()
        {
            if (returnState == PlayState.Paused)
            {
                State = PlayState.Paused;
                Hud.Apply(State);
                return;
            }
            if (returnState == PlayState.Menu || !runStarted)
            {
                EnterMenu();
                return;
            }
            State = PlayState.Playing;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Hud.Apply(State);
        }

        public void RequestDrop(ItemId id)
        {
            if (id == ItemId.Torch)
            {
                Ask("Drop the torch?", "The darkness will close in. You can still grab it if you are quick.", "Drop it", () => Drop(id));
                return;
            }
            Drop(id);
        }

        public void RequestUse(ItemId id)
        {
            if (id == ItemId.PoisonCheese)
            {
                Ask("Eat the poison cheese?", "It looks tasty. It is not.", "Eat", () =>
                {
                    Inventory.Remove(ItemId.PoisonCheese);
                    Finish(EndingKind.Death, "Poison", StoryText.CheeseBody, false);
                });
                return;
            }
            if (id == ItemId.BucketOfWater)
            {
                Hud.Flash("Use the bucket at the brazier beside the Lava Falls.");
                return;
            }
            Hud.Flash("Nothing happens.");
        }

        void Drop(ItemId id)
        {
            if (!Inventory.Remove(id)) return;
            if (id == ItemId.Torch)
            {
                Vector3 feet = Player.transform.position + Player.transform.forward * 0.9f + Vector3.up * 0.3f;
                ItemDrop.Spawn(ItemId.Torch, feet, Refs.transform);
                darkness = Mathf.Max(darkness, 0.12f);
                Hud.Banner("The torch is on the ground.");
            }
            else
            {
                Vector3 slot = Refs.dropAnchor.position + new Vector3((dropCount % 4) * 1.35f, 0.7f, (dropCount / 4) * 1.35f);
                dropCount++;
                ItemDrop.Spawn(id, slot, Refs.transform);
                Hud.Banner(ItemCatalog.Get(id).displayName + " dropped. Find it in the Drop Room.");
            }
            if (SoundBoard.Instance != null) SoundBoard.Instance.Play("ui");
            if (State == PlayState.Inventory) Hud.Apply(State);
        }

        public void FightMove(string move)
        {
            if (Fight != null) Fight.Choose(move);
        }

        public void ConfirmYes()
        {
            var action = pendingYes;
            pendingYes = null;
            State = PlayState.Playing;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Hud.Apply(State);
            if (action != null) action();
        }

        public void ConfirmNo()
        {
            pendingYes = null;
            State = PlayState.Playing;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Hud.Apply(State);
        }

        public void ContinueNarration()
        {
            if (State != PlayState.Narration) return;
            BeginCombat();
        }

        public void WinDragon()
        {
            Finish(EndingKind.Win, "You win", StoryText.WinBody, true);
        }

        public void LoseDragon()
        {
            Finish(EndingKind.Death, "Slain", StoryText.DragonLossBody, false);
        }

        public void PlayAgain()
        {
            Reload(true);
        }

        public void ToMenu()
        {
            Reload(false);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void Reload(bool auto)
        {
            pendingAutoStart = auto;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("Main");
        }

        void Ask(string title, string body, string yes, System.Action onYes)
        {
            pendingYes = onYes;
            returnState = PlayState.Playing;
            State = PlayState.Confirm;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Hud.SetConfirm(title, body);
            Hud.Apply(State);
        }

        void Use(WorldInteract interact)
        {
            switch (interact.kind)
            {
                case InteractKind.Torch:
                case InteractKind.Pickup:
                    Take(interact);
                    break;
                case InteractKind.Story:
                    OpenJournal();
                    break;
                case InteractKind.Basilisk:
                    Ask("Kill?", StoryText.BasiliskBody, "Yes", () => Finish(EndingKind.Death, "The basilisk", StoryText.BasiliskDeath, false));
                    break;
                case InteractKind.Lava:
                    Ask("Jump?", StoryText.LavaBody, "Jump", () => Finish(EndingKind.Death, "The falls", StoryText.LavaDeath, false));
                    break;
                case InteractKind.Brazier:
                    UseBrazier(interact);
                    break;
                case InteractKind.Raft:
                    Ask("Ride away?", StoryText.RaftBody, "Ride", () => Finish(EndingKind.Escape, "Escaped", StoryText.RaftEscape, false));
                    break;
                case InteractKind.Gate:
                    TryGate();
                    break;
                case InteractKind.Chest:
                    TryChest(interact);
                    break;
            }
        }

        void Take(WorldInteract interact)
        {
            if (!Inventory.Add(interact.item))
            {
                Hud.Flash("You already carry that.");
                return;
            }
            if (interact.item != ItemId.Torch) Score += 25;
            if (SoundBoard.Instance != null) SoundBoard.Instance.Play("pickup");
            var taken = interact.item;
            Destroy(interact.gameObject);
            Hud.Banner("Took " + ItemCatalog.Get(taken).displayName + ".");
            if (taken == ItemId.Torch && !storyShown)
            {
                storyShown = true;
                OpenJournal();
            }
        }

        void UseBrazier(WorldInteract interact)
        {
            var brazier = interact.GetComponent<Brazier>();
            if (brazier != null && brazier.doused)
            {
                Hud.Flash("The spout is already out.");
                return;
            }
            if (!Inventory.Has(ItemId.BucketOfWater))
            {
                Hud.Flash("A bucket of water would smother this fire.");
                return;
            }
            if (brazier != null) brazier.Douse();
            brazierDoused = true;
            Score += 40;
            Hud.Banner(StoryText.BrazierClue);
            if (SoundBoard.Instance != null) SoundBoard.Instance.Play("ui");
        }

        void TryChest(WorldInteract interact)
        {
            if (!Inventory.Has(ItemId.ChestKey))
            {
                Hud.Flash("You need the chest key.");
                return;
            }
            if (capeSpawned)
            {
                Hud.Flash("The chest is empty.");
                return;
            }
            capeSpawned = true;
            if (Refs.chestLid != null) Refs.chestLid.Open();
            Vector3 pos = interact.transform.position + Vector3.up * 1.4f + Vector3.forward * 0.4f;
            ItemDrop.Spawn(ItemId.AntiFireCape, pos, Refs.transform);
            Score += 40;
            Hud.Banner("The chest key turns. An anti-fire cape lies in the chest.");
            if (SoundBoard.Instance != null) SoundBoard.Instance.Play("pickup");
        }

        void TryGate()
        {
            if (!Inventory.Has(ItemId.SewerKey))
            {
                Hud.Flash("You need a sewer key.");
                return;
            }
            if (!Inventory.Has(ItemId.MithrilAxe))
            {
                Hud.Flash("You need a mithril axe.");
                return;
            }
            StartCoroutine(OpenGate());
        }

        IEnumerator OpenGate()
        {
            State = PlayState.Cutscene;
            cinematic = true;
            Hud.SetPrompt("", Color.white);
            Hud.Apply(State);
            if (Player != null) Player.FaceYaw(0f);
            Vector3 left = Refs.gateLeft.localPosition;
            Vector3 right = Refs.gateRight.localPosition;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 1.5f;
                float slide = Mathf.SmoothStep(0f, 4.2f, Mathf.Clamp01(t));
                Refs.gateLeft.localPosition = left + Vector3.left * slide;
                Refs.gateRight.localPosition = right + Vector3.right * slide;
                yield return null;
            }
            if (Player != null) Player.SetTorchSuppressed(true);
            if (Refs.lairLight != null) Refs.lairLight.enabled = true;
            if (SoundBoard.Instance != null) SoundBoard.Instance.Play("roar");
            State = PlayState.Narration;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Hud.SetNarration(StoryText.GateBody);
            Hud.Apply(State);
        }

        void BeginCombat()
        {
            State = PlayState.Combat;
            cinematic = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Fight.ResetFight();
            var profile = Inventory.CombatProfile();
            Hud.BeginCombat(profile, Fight.playerHp, Fight.dragonHp);
            Hud.Apply(State);
        }

        void Finish(EndingKind kind, string title, string body, bool dragonBonus)
        {
            if (State == PlayState.Ended) return;
            if (dragonBonus) Score += 500;
            State = PlayState.Ended;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (kind == EndingKind.Death && SoundBoard.Instance != null) SoundBoard.Instance.Play("lose");
            Hud.ShowEnding(title, body, Score, kind);
            Hud.Apply(State);
        }

        bool TicksDark()
        {
            if (!runStarted || Inventory == null) return false;
            switch (State)
            {
                case PlayState.Playing:
                case PlayState.Inventory:
                case PlayState.Journal:
                case PlayState.HowTo:
                case PlayState.Confirm:
                    return true;
                default:
                    return false;
            }
        }

        void TrackArea()
        {
            int col;
            int row;
            if (!MapLayout.WorldToCell(Player.transform.position, out col, out row)) return;
            var area = MapLayout.At(col, row);
            Hud.SetArea(MapLayout.DisplayName(area));
            string key = col + ":" + row;
            if (key != lastCell)
            {
                lastCell = key;
                string line = MapLayout.ArrivalLine(area);
                if (!string.IsNullOrEmpty(line)) Hud.Banner(line);
            }
        }

        string lastCell = "";

        WorldInteract Nearest()
        {
            WorldInteract best = null;
            float bestDist = InteractRange;
            Vector3 p = Player.transform.position;
            for (int i = 0; i < WorldInteract.All.Count; i++)
            {
                var candidate = WorldInteract.All[i];
                if (candidate == null) continue;
                float d = Vector3.Distance(p, candidate.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = candidate;
                }
            }
            return best;
        }
    }
}
