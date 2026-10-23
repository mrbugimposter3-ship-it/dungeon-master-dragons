using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonMaster
{
    public class HudController : MonoBehaviour
    {
        static Sprite white;
        Font font;

        GameObject gameplay;
        GameObject menu;
        GameObject pausePanel;
        GameObject inventoryPanel;
        GameObject journalPanel;
        GameObject howToPanel;
        GameObject confirmPanel;
        GameObject narrationPanel;
        GameObject combatPanel;
        GameObject endingPanel;

        Image vignette;
        Text areaText;
        Text scoreText;
        Text promptText;
        Text bannerText;
        Text torchText;
        Text statsText;
        Text flashText;
        float flashUntil;
        string lastFlash;
        float bannerUntil;

        RectTransform mapRoot;
        readonly Image[,] mapCells = new Image[MapLayout.Size, MapLayout.Size];
        readonly Text[,] mapLetters = new Text[MapLayout.Size, MapLayout.Size];
        RectTransform playerDot;
        bool mapExpanded;

        Text confirmTitle;
        Text confirmBody;
        Text narrationBody;
        Text endingTitle;
        Text endingBody;
        Text endingScore;
        Image endingBanner;

        Text combatLogText;
        Text combatStats;
        Image playerFill;
        Image dragonFill;
        Text playerHpLabel;
        Text dragonHpLabel;
        readonly List<string> combatLines = new List<string>();

        RectTransform listRoot;
        Text detailText;
        ItemId selected;
        bool hasSelection;

        public void Build()
        {
            font = PrimitiveKit.BuiltinFont();
            var canvasGo = new GameObject("HUD");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            vignette = FullImage(canvasGo.transform, "Vignette", new Color(0f, 0f, 0f, 0f));
            vignette.raycastTarget = false;

            gameplay = new GameObject("Gameplay", typeof(RectTransform));
            gameplay.transform.SetParent(canvasGo.transform, false);
            Stretch(gameplay.GetComponent<RectTransform>());
            BuildGameplay(gameplay.transform);

            menu = BuildMenu(canvasGo.transform);
            pausePanel = BuildPause(canvasGo.transform);
            inventoryPanel = BuildInventory(canvasGo.transform);
            journalPanel = BuildTextPanel(canvasGo.transform, "Journal", StoryText.IslandStory + "\n\n" + StoryText.DragonLore, CloseTop);
            howToPanel = BuildTextPanel(canvasGo.transform, "How to play", StoryText.HowToPlay, CloseTop);
            confirmPanel = BuildConfirm(canvasGo.transform);
            narrationPanel = BuildNarration(canvasGo.transform);
            combatPanel = BuildCombat(canvasGo.transform);
            endingPanel = BuildEnding(canvasGo.transform);
            Apply(PlayState.Menu);
        }

        public void Apply(PlayState state)
        {
            bool showPlay = state == PlayState.Playing || state == PlayState.Inventory || state == PlayState.Journal
                || state == PlayState.Confirm || state == PlayState.HowTo || state == PlayState.Cutscene;
            if (gameplay != null) gameplay.SetActive(showPlay);
            if (menu != null) menu.SetActive(state == PlayState.Menu);
            if (pausePanel != null) pausePanel.SetActive(state == PlayState.Paused);
            if (inventoryPanel != null) inventoryPanel.SetActive(state == PlayState.Inventory);
            if (journalPanel != null) journalPanel.SetActive(state == PlayState.Journal);
            if (howToPanel != null) howToPanel.SetActive(state == PlayState.HowTo);
            if (confirmPanel != null) confirmPanel.SetActive(state == PlayState.Confirm);
            if (narrationPanel != null) narrationPanel.SetActive(state == PlayState.Narration);
            if (combatPanel != null) combatPanel.SetActive(state == PlayState.Combat);
            if (endingPanel != null) endingPanel.SetActive(state == PlayState.Ended);
            if (state == PlayState.Inventory) RefreshInventory();
        }

        void Update()
        {
            var game = GameManager.Instance;
            if (game == null) return;
            if (scoreText != null) scoreText.text = "Score  " + game.Score;
            if (game.Inventory != null && statsText != null && gameplay != null && gameplay.activeSelf)
            {
                double hp = CombatMath.PlayerMaxHealth;
                if (game.State == PlayState.Combat && game.Fight != null) hp = game.Fight.playerHp;
                var profile = game.Inventory.CombatProfile();
                statsText.text =
                    "Health   " + (int)hp + " / " + (int)CombatMath.PlayerMaxHealth + "\n" +
                    "Attack   " + game.Inventory.Attack + "\n" +
                    "Defence  " + game.Inventory.Defence + "\n" +
                    "Fire res " + game.Inventory.FireRes + "\n" +
                    "Breath   " + (int)profile.fireBreath;
            }
            if (torchText != null && game.Inventory != null)
            {
                if (game.Inventory.TorchLit) torchText.text = "Torch  lit";
                else torchText.text = "Dark  " + Mathf.RoundToInt(game.Darkness01 * 100f) + "%";
                torchText.color = game.Inventory.TorchLit ? new Color(1f, 0.72f, 0.35f) : new Color(0.9f, 0.3f, 0.25f);
            }
            if (vignette != null)
            {
                float a = Mathf.Clamp01(game.Darkness01);
                vignette.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.92f, a));
            }
            if (bannerText != null && Time.time > bannerUntil) bannerText.text = "";
            if (flashText != null && Time.unscaledTime > flashUntil) flashText.text = "";
            if (game.Player != null) RefreshMap(game.Player.transform.position, game.Player.yaw);
        }

        public void SetArea(string area)
        {
            if (areaText != null) areaText.text = area;
        }

        public void SetPrompt(string prompt, Color color)
        {
            if (promptText == null) return;
            promptText.text = prompt;
            promptText.color = color;
        }

        public void Banner(string text)
        {
            if (bannerText == null) return;
            bannerText.text = text;
            bannerUntil = Time.time + 5.5f;
        }

        public void Flash(string text)
        {
            if (flashText == null) return;
            if (text == lastFlash && Time.unscaledTime < flashUntil) return;
            lastFlash = text;
            flashText.text = text;
            flashUntil = Time.unscaledTime + 1.6f;
        }

        public void SetConfirm(string title, string body)
        {
            if (confirmTitle != null) confirmTitle.text = title;
            if (confirmBody != null) confirmBody.text = body;
        }

        public void SetNarration(string body)
        {
            if (narrationBody != null) narrationBody.text = body;
        }

        public void ShowEnding(string title, string body, int score, EndingKind kind)
        {
            if (endingTitle != null) endingTitle.text = title;
            if (endingBody != null) endingBody.text = body;
            if (endingScore != null) endingScore.text = "Final Score   " + score;
            if (endingBanner != null)
            {
                if (kind == EndingKind.Win) endingBanner.color = new Color(0.45f, 0.22f, 0.08f, 0.95f);
                else if (kind == EndingKind.Escape) endingBanner.color = new Color(0.1f, 0.28f, 0.26f, 0.95f);
                else endingBanner.color = new Color(0.28f, 0.06f, 0.06f, 0.95f);
            }
        }

        public void BeginCombat(CombatMath.Profile profile, double playerHp, double dragonHp)
        {
            combatLines.Clear();
            if (combatLogText != null) combatLogText.text = "The dragon is awake.";
            RefreshCombat(playerHp, dragonHp, profile);
        }

        public void CombatLog(string line)
        {
            combatLines.Add(line);
            while (combatLines.Count > 8) combatLines.RemoveAt(0);
            if (combatLogText != null) combatLogText.text = string.Join("\n", combatLines.ToArray());
        }

        public void RefreshCombat(double playerHp, double dragonHp, CombatMath.Profile profile)
        {
            SetFill(playerFill, (float)(playerHp / CombatMath.PlayerMaxHealth));
            SetFill(dragonFill, (float)(dragonHp / CombatMath.DragonMaxHealth));
            if (playerHpLabel != null) playerHpLabel.text = "You  " + (int)playerHp;
            if (dragonHpLabel != null) dragonHpLabel.text = "Dragon  " + (int)dragonHp;
            if (combatStats != null)
            {
                combatStats.text =
                    "Attack " + (GameManager.Instance != null ? GameManager.Instance.Inventory.Attack : 0) +
                    "    Defence " + (GameManager.Instance != null ? GameManager.Instance.Inventory.Defence : 0) +
                    "    Fire res " + (GameManager.Instance != null ? GameManager.Instance.Inventory.FireRes : 0) +
                    "\nWhirlwind " + (int)profile.whirlWind +
                    "    Doom Hammer " + (int)profile.doomHammer +
                    "    Cobra Strike " + (int)profile.cobraStrike +
                    "\nFire breath " + (int)profile.fireBreath +
                    "    Bone Crush " + (int)profile.boneCrush +
                    "    Wing Smash " + (int)profile.wingSmash;
            }
        }

        public void ToggleMap()
        {
            mapExpanded = !mapExpanded;
            if (mapRoot != null)
                mapRoot.localScale = Vector3.one * (mapExpanded ? 1.85f : 1f);
        }

        void RefreshMap(Vector3 world, float yaw)
        {
            int col;
            int row;
            bool inside = MapLayout.WorldToCell(world, out col, out row);
            for (int r = 0; r < MapLayout.Size; r++)
            {
                for (int c = 0; c < MapLayout.Size; c++)
                {
                    var area = MapLayout.At(c, r);
                    var color = MapLayout.ColorOf(area);
                    bool here = inside && c == col && r == row;
                    if (here) color = Color.Lerp(color, new Color(1f, 0.72f, 0.35f), 0.55f);
                    if (mapCells[c, r] != null) mapCells[c, r].color = color;
                }
            }
            if (playerDot != null && inside)
            {
                float cell = mapExpanded ? 34f : 34f;
                float origin = -((MapLayout.Size - 1) * cell) * 0.5f;
                playerDot.anchoredPosition = new Vector2(origin + col * cell, -origin - row * cell);
                playerDot.localEulerAngles = new Vector3(0f, 0f, -yaw);
            }
        }

        void RefreshInventory()
        {
            if (listRoot == null || GameManager.Instance == null) return;
            for (int i = listRoot.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(listRoot.GetChild(i).gameObject);
            var items = GameManager.Instance.Inventory.items;
            if (items.Count == 0)
            {
                hasSelection = false;
                if (detailText != null) detailText.text = "Empty hands, aside from whatever you have not dropped.";
                return;
            }
            if (!hasSelection || !items.Contains(selected))
            {
                selected = items[0];
                hasSelection = true;
            }
            for (int i = 0; i < items.Count; i++)
            {
                ItemId id = items[i];
                var def = ItemCatalog.Get(id);
                var btn = MakeButton(listRoot, def.displayName, new Vector2(0f, 0f), new Vector2(280f, 36f), () => Select(id), id == selected ? new Color(0.55f, 0.24f, 0.1f) : new Color(0.18f, 0.1f, 0.08f));
                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 40f;
                le.preferredHeight = 40f;
            }
            ShowDetail();
        }

        void Select(ItemId id)
        {
            selected = id;
            hasSelection = true;
            ShowDetail();
            if (listRoot == null) return;
            for (int i = 0; i < listRoot.childCount; i++)
            {
                var image = listRoot.GetChild(i).GetComponent<Image>();
                var label = listRoot.GetChild(i).GetComponentInChildren<Text>();
                if (image == null || label == null) continue;
                bool on = label.text == ItemCatalog.Get(id).displayName;
                image.color = on ? new Color(0.55f, 0.24f, 0.1f) : new Color(0.18f, 0.1f, 0.08f);
            }
        }

        void ShowDetail()
        {
            if (detailText == null || !hasSelection) return;
            var def = ItemCatalog.Get(selected);
            detailText.text = def.displayName + "\n\n" + def.blurb + "\n\n" + ItemCatalog.StatLine(def);
        }

        public void DropSelected()
        {
            if (!hasSelection) return;
            GameManager.Instance.RequestDrop(selected);
        }

        public void UseSelected()
        {
            if (!hasSelection) return;
            GameManager.Instance.RequestUse(selected);
        }

        void BuildGameplay(Transform parent)
        {
            areaText = MakeText(parent, "Volcanic Wilds", 28, BoneColor(), TextAnchor.UpperLeft, new Vector2(24f, -18f), new Vector2(520f, 40f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            scoreText = MakeText(parent, "Score  10", 26, EmberColor(), TextAnchor.UpperCenter, new Vector2(0f, -18f), new Vector2(320f, 40f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            bannerText = MakeText(parent, "", 22, BoneColor(), TextAnchor.UpperCenter, new Vector2(0f, -62f), new Vector2(1100f, 70f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            promptText = MakeText(parent, "", 26, BoneColor(), TextAnchor.LowerCenter, new Vector2(0f, 36f), new Vector2(900f, 50f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            flashText = MakeText(parent, "", 22, new Color(1f, 0.45f, 0.35f), TextAnchor.LowerCenter, new Vector2(0f, 90f), new Vector2(900f, 40f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            torchText = MakeText(parent, "Torch", 20, EmberColor(), TextAnchor.LowerRight, new Vector2(-24f, 24f), new Vector2(220f, 36f), new Vector2(1f, 0f), new Vector2(1f, 0f));

            var stats = Panel(parent, "Stats", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(16f, 16f), new Vector2(250f, 170f), new Color(0.05f, 0.03f, 0.03f, 0.78f));
            statsText = MakeText(stats, "", 20, BoneColor(), TextAnchor.UpperLeft, new Vector2(12f, -10f), new Vector2(226f, 150f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            mapRoot = Panel(parent, "Map", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -16f), new Vector2(190f, 210f), new Color(0.04f, 0.03f, 0.03f, 0.8f));
            mapRoot.pivot = new Vector2(1f, 1f);
            float cell = 34f;
            float origin = -((MapLayout.Size - 1) * cell) * 0.5f;
            for (int r = 0; r < MapLayout.Size; r++)
            {
                for (int c = 0; c < MapLayout.Size; c++)
                {
                    var cellRt = Panel(mapRoot, "C", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(origin + c * cell, -origin - r * cell), new Vector2(30f, 30f), MapLayout.ColorOf(MapLayout.At(c, r)));
                    mapCells[c, r] = cellRt.GetComponent<Image>();
                    var letter = MapLayout.Letter(MapLayout.At(c, r));
                    mapLetters[c, r] = MakeText(cellRt, letter, 16, Color.white, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(30f, 30f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                }
            }
            var dot = Panel(mapRoot, "You", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10f, 14f), Color.white);
            playerDot = dot;
            MakeText(mapRoot, "N", 14, EmberColor(), TextAnchor.UpperCenter, new Vector2(0f, -4f), new Vector2(30f, 18f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        }

        GameObject BuildMenu(Transform parent)
        {
            var root = FullScreen(parent, "Menu", new Color(0.02f, 0.01f, 0.012f, 0.45f));
            var card = Panel(root.transform, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 560f), new Color(0.06f, 0.03f, 0.03f, 0.94f));
            MakeText(card, StoryText.Title, 54, EmberColor(), TextAnchor.UpperCenter, new Vector2(0f, -28f), new Vector2(600f, 70f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            MakeText(card, StoryText.Subtitle, 26, BoneColor(), TextAnchor.UpperCenter, new Vector2(0f, -100f), new Vector2(600f, 36f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            MakeText(card, StoryText.Credit, 16, new Color(0.75f, 0.68f, 0.58f), TextAnchor.UpperCenter, new Vector2(0f, -146f), new Vector2(560f, 50f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            MakeButton(card, "Play", new Vector2(0f, 20f), new Vector2(280f, 52f), () => GameManager.Instance.StartGame(), EmberColor());
            MakeButton(card, "How to play", new Vector2(0f, -48f), new Vector2(280f, 48f), () => GameManager.Instance.OpenHowTo(), new Color(0.22f, 0.12f, 0.09f));
            MakeButton(card, "Quit", new Vector2(0f, -112f), new Vector2(280f, 48f), () => GameManager.Instance.Quit(), new Color(0.16f, 0.08f, 0.08f));
            MakeText(card, "Unity 6.3 LTS", 14, new Color(0.6f, 0.52f, 0.45f), TextAnchor.LowerCenter, new Vector2(0f, 16f), new Vector2(400f, 24f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            return root;
        }

        GameObject BuildPause(Transform parent)
        {
            var root = FullScreen(parent, "Pause", new Color(0f, 0f, 0f, 0.55f));
            var card = Panel(root.transform, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460f, 380f), new Color(0.06f, 0.03f, 0.03f, 0.95f));
            MakeText(card, "Paused", 40, EmberColor(), TextAnchor.UpperCenter, new Vector2(0f, -24f), new Vector2(400f, 50f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            MakeButton(card, "Resume", new Vector2(0f, 30f), new Vector2(260f, 48f), () => GameManager.Instance.Resume(), EmberColor());
            MakeButton(card, "How to play", new Vector2(0f, -30f), new Vector2(260f, 48f), () => GameManager.Instance.OpenHowTo(), new Color(0.22f, 0.12f, 0.09f));
            MakeButton(card, "Quit to title", new Vector2(0f, -90f), new Vector2(260f, 48f), () => GameManager.Instance.ToMenu(), new Color(0.16f, 0.08f, 0.08f));
            return root;
        }

        GameObject BuildInventory(Transform parent)
        {
            var root = FullScreen(parent, "Inventory", new Color(0f, 0f, 0f, 0.35f));
            var card = Panel(root.transform, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 560f), new Color(0.06f, 0.035f, 0.03f, 0.96f));
            MakeText(card, "Pack", 32, EmberColor(), TextAnchor.UpperLeft, new Vector2(20f, -16f), new Vector2(300f, 40f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            var list = Panel(card, "List", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(16f, 70f), new Vector2(320f, 420f), new Color(0f, 0f, 0f, 0.25f));
            var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            listRoot = list;
            detailText = MakeText(card, "", 20, BoneColor(), TextAnchor.UpperLeft, new Vector2(360f, -70f), new Vector2(470f, 300f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            MakeButton(card, "Drop", new Vector2(-180f, -230f), new Vector2(160f, 44f), DropSelected, new Color(0.35f, 0.14f, 0.1f));
            MakeButton(card, "Use / Eat", new Vector2(20f, -230f), new Vector2(180f, 44f), UseSelected, EmberColor());
            MakeButton(card, "Close", new Vector2(220f, -230f), new Vector2(160f, 44f), () => GameManager.Instance.CloseOverlay(), new Color(0.16f, 0.1f, 0.08f));
            return root;
        }

        GameObject BuildTextPanel(Transform parent, string title, string body, UnityEngine.Events.UnityAction close)
        {
            var root = FullScreen(parent, title, new Color(0f, 0f, 0f, 0.62f));
            var card = Panel(root.transform, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880f, 760f), new Color(0.06f, 0.035f, 0.03f, 0.97f));
            MakeText(card, title, 32, EmberColor(), TextAnchor.UpperCenter, new Vector2(0f, -16f), new Vector2(800f, 42f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            var scrollHost = Panel(card, "ScrollHost", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(820f, 580f), new Color(0f, 0f, 0f, 0.2f));
            scrollHost.pivot = new Vector2(0.5f, 1f);
            AddScroll(scrollHost, body);
            MakeButton(card, "Close", new Vector2(0f, -340f), new Vector2(200f, 44f), close, EmberColor());
            return root;
        }

        GameObject BuildConfirm(Transform parent)
        {
            var root = FullScreen(parent, "Confirm", new Color(0f, 0f, 0f, 0.62f));
            var card = Panel(root.transform, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 520f), new Color(0.07f, 0.04f, 0.03f, 0.97f));
            confirmTitle = MakeText(card, "", 32, EmberColor(), TextAnchor.UpperCenter, new Vector2(0f, -20f), new Vector2(700f, 44f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            confirmBody = MakeText(card, "", 20, BoneColor(), TextAnchor.UpperLeft, new Vector2(28f, -80f), new Vector2(700f, 300f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            MakeButton(card, "Yes", new Vector2(-120f, -210f), new Vector2(180f, 48f), () => GameManager.Instance.ConfirmYes(), new Color(0.55f, 0.16f, 0.1f));
            MakeButton(card, "No", new Vector2(120f, -210f), new Vector2(180f, 48f), () => GameManager.Instance.ConfirmNo(), new Color(0.16f, 0.12f, 0.1f));
            return root;
        }

        GameObject BuildNarration(Transform parent)
        {
            var root = FullScreen(parent, "Narration", new Color(0f, 0f, 0f, 0.72f));
            var card = Panel(root.transform, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 560f), new Color(0.08f, 0.03f, 0.02f, 0.97f));
            narrationBody = MakeText(card, "", 22, BoneColor(), TextAnchor.UpperLeft, new Vector2(28f, -24f), new Vector2(840f, 400f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            MakeButton(card, "Fight", new Vector2(0f, -230f), new Vector2(220f, 50f), () => GameManager.Instance.ContinueNarration(), EmberColor());
            return root;
        }

        GameObject BuildCombat(Transform parent)
        {
            var root = FullScreen(parent, "Combat", new Color(0.05f, 0.01f, 0.01f, 0.28f));
            var top = Panel(root.transform, "Bars", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(1100f, 130f), new Color(0.05f, 0.02f, 0.02f, 0.8f));
            top.pivot = new Vector2(0.5f, 1f);
            playerHpLabel = MakeText(top, "You", 18, BoneColor(), TextAnchor.UpperLeft, new Vector2(16f, -8f), new Vector2(400f, 24f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            var pBack = Panel(top, "PBack", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -36f), new Vector2(480f, 22f), new Color(0.15f, 0.08f, 0.08f));
            pBack.pivot = new Vector2(0f, 1f);
            playerFill = BarFill(pBack, new Color(0.75f, 0.2f, 0.15f));
            dragonHpLabel = MakeText(top, "Dragon", 18, EmberColor(), TextAnchor.UpperRight, new Vector2(-16f, -8f), new Vector2(400f, 24f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            var dBack = Panel(top, "DBack", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -36f), new Vector2(480f, 22f), new Color(0.15f, 0.08f, 0.08f));
            dBack.pivot = new Vector2(1f, 1f);
            dragonFill = BarFill(dBack, new Color(1f, 0.42f, 0.1f));
            combatStats = MakeText(top, "", 16, BoneColor(), TextAnchor.LowerLeft, new Vector2(16f, 8f), new Vector2(1060f, 58f), new Vector2(0f, 0f), new Vector2(0f, 0f));

            var log = Panel(root.transform, "Log", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16f, 40f), new Vector2(420f, 280f), new Color(0.04f, 0.02f, 0.02f, 0.78f));
            log.pivot = new Vector2(1f, 0.5f);
            combatLogText = MakeText(log, "", 18, BoneColor(), TextAnchor.UpperLeft, new Vector2(12f, -12f), new Vector2(396f, 256f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            MakeButton(root.transform, "1  Whirlwind", new Vector2(-280f, -470f), new Vector2(240f, 54f), () => GameManager.Instance.FightMove("ww"), new Color(0.42f, 0.16f, 0.1f));
            MakeButton(root.transform, "2  Doom Hammer", new Vector2(0f, -470f), new Vector2(260f, 54f), () => GameManager.Instance.FightMove("dh"), EmberColor());
            MakeButton(root.transform, "3  Cobra Strike", new Vector2(280f, -470f), new Vector2(240f, 54f), () => GameManager.Instance.FightMove("cs"), new Color(0.15f, 0.28f, 0.22f));
            return root;
        }

        GameObject BuildEnding(Transform parent)
        {
            var root = FullScreen(parent, "Ending", new Color(0f, 0f, 0f, 0.72f));
            var card = Panel(root.transform, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 460f), new Color(0.08f, 0.03f, 0.03f, 0.97f));
            endingBanner = card.GetComponent<Image>();
            endingTitle = MakeText(card, "", 40, BoneColor(), TextAnchor.UpperCenter, new Vector2(0f, -24f), new Vector2(700f, 56f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            endingBody = MakeText(card, "", 22, BoneColor(), TextAnchor.UpperLeft, new Vector2(32f, -100f), new Vector2(690f, 200f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            endingScore = MakeText(card, "", 28, EmberColor(), TextAnchor.MiddleCenter, new Vector2(0f, 168f), new Vector2(500f, 40f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            MakeButton(card, "Play again", new Vector2(-120f, -175f), new Vector2(200f, 48f), () => GameManager.Instance.PlayAgain(), EmberColor());
            MakeButton(card, "Title", new Vector2(120f, -175f), new Vector2(200f, 48f), () => GameManager.Instance.ToMenu(), new Color(0.16f, 0.1f, 0.08f));
            return root;
        }

        void AddScroll(RectTransform host, string body)
        {
            var scroll = host.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(host, false);
            var vp = viewport.GetComponent<RectTransform>();
            Stretch(vp);
            var vpImage = viewport.AddComponent<Image>();
            vpImage.sprite = White();
            vpImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var crt = content.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.anchoredPosition = Vector2.zero;
            crt.sizeDelta = new Vector2(0f, 1600f);
            var text = MakeText(crt, body, 20, BoneColor(), TextAnchor.UpperLeft, new Vector2(0f, 0f), new Vector2(-24f, 1560f), new Vector2(0f, 1f), new Vector2(1f, 1f));
            text.rectTransform.offsetMin = new Vector2(16f, -1560f);
            text.rectTransform.offsetMax = new Vector2(-16f, -8f);
            scroll.viewport = vp;
            scroll.content = crt;
        }

        void CloseTop()
        {
            if (GameManager.Instance != null) GameManager.Instance.CloseOverlay();
        }

        static void SetFill(Image fill, float amount)
        {
            if (fill == null) return;
            var rt = fill.rectTransform;
            rt.anchorMax = new Vector2(Mathf.Clamp01(amount), 1f);
        }

        static Image BarFill(RectTransform back, Color color)
        {
            var fill = Panel(back, "Fill", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, color);
            Stretch(fill);
            return fill.GetComponent<Image>();
        }

        static Image FullImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.sprite = White();
            image.color = color;
            Stretch(go.GetComponent<RectTransform>());
            return image;
        }

        static GameObject FullScreen(Transform parent, string name, Color color)
        {
            var image = FullImage(parent, name, color);
            return image.gameObject;
        }

        static RectTransform Panel(Transform parent, string name, Vector2 anchor, Vector2 unused, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.sprite = White();
            image.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        Text MakeText(Transform parent, string content, int size, Color color, TextAnchor align, Vector2 pos, Vector2 dims, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = align;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = content;
            text.raycastTarget = false;
            var rt = text.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(anchorMin.x, anchorMax.y);
            rt.anchoredPosition = pos;
            rt.sizeDelta = dims;
            return text;
        }

        Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction click, Color color)
        {
            var rt = Panel(parent, label, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, color);
            var button = rt.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.8f, 0.55f);
            colors.pressedColor = new Color(0.7f, 0.4f, 0.25f);
            button.colors = colors;
            button.onClick.AddListener(click);
            MakeText(rt, label, 22, BoneColor(), TextAnchor.MiddleCenter, Vector2.zero, size, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            return button;
        }

        static Color BoneColor()
        {
            return new Color(0.94f, 0.88f, 0.76f);
        }

        static Color EmberColor()
        {
            return new Color(0.95f, 0.48f, 0.18f);
        }

        static Sprite White()
        {
            if (white == null)
            {
                var tex = Texture2D.whiteTexture;
                white = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            return white;
        }
    }
}
