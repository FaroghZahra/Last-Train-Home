using System.IO;
using System.Linq;
using LastTrainHome.Game.Bootstrap;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;
using LastTrainHome.Game.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Editor
{
    /// <summary>
    /// Builds the game's interface into the open scene: one canvas with the title, night card, platform, planning,
    /// ride and logbook screens, every view wired to its widgets and the bootstrap wired to the views. Also points the
    /// level catalog at the content files. Safe to run again: it replaces the canvas it made last time.
    /// Layout follows the "Last Train Home — Redesign" canvas at 1.5× (it was drawn at 1280×720).
    /// </summary>
    internal static class UiBuilder
    {
        private const string CanvasName = "UI";
        private const string DataFolder = "Assets/Data";
        private const float PlanCardsWidth = 440f;
        private const float PlanPanelWidth = 400f;

        private static UiKit _kit;
        private static UiStyle _style;
        private static PassengerArt _passengerArt;
        private static KeepsakeArt _keepsakeArt;
        private static Sprite _background;

        [MenuItem("Last Train Home/Build UI")]
        public static void Build()
        {
            SetUpContent();
            LoadKit();

            Object.DestroyImmediate(GameObject.Find(CanvasName));
            Canvas canvas = CreateCanvas();
            Transform root = canvas.transform;

            PlanView plan = BuildPlan(root);
            RideView ride = BuildRide(root);
            PlatformView platform = BuildPlatform(root);
            NightCardView nightCard = BuildNightCard(root);
            LogbookView logbook = BuildLogbook(root);
            TitleScreenView title = BuildTitle(root);

            GameBootstrap bootstrap = Object.FindObjectOfType<GameBootstrap>();
            UiKit.Wire(bootstrap, "_levels", AssetDatabase.LoadAssetAtPath<LevelCatalog>($"{DataFolder}/LevelCatalog.asset"));
            UiKit.Wire(bootstrap, "_settings", AssetDatabase.LoadAssetAtPath<GameSettings>($"{DataFolder}/GameSettings.asset"));
            UiKit.Wire(bootstrap, "_railMap", Object.FindObjectOfType<RailMapView>(true));
            UiKit.Wire(bootstrap, "_route", Object.FindObjectOfType<RouteView>(true));
            UiKit.Wire(bootstrap, "_title", title);
            UiKit.Wire(bootstrap, "_nightCard", nightCard);
            UiKit.Wire(bootstrap, "_platform", platform);
            UiKit.Wire(bootstrap, "_plan", plan);
            UiKit.Wire(bootstrap, "_ride", ride);
            UiKit.Wire(bootstrap, "_logbook", logbook);
            UiKit.Wire(bootstrap, "_audio", BuildAudio(bootstrap.transform));
            FitCameraBetweenPanels();

            EditorSceneManager.MarkSceneDirty(bootstrap.gameObject.scene);
            Debug.Log("Last Train Home: UI built. Save the scene to keep it.");
        }

        /// <summary>The sounds, from Assets/Audio (made by Design/make_sounds.py), one clip per game sound.</summary>
        private static AudioView BuildAudio(Transform parent)
        {
            const string name = "Audio";
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var audio = new GameObject(name, typeof(AudioSource), typeof(AudioView));
            audio.transform.SetParent(parent, false);
            audio.GetComponent<AudioSource>().playOnAwake = false;
            AudioView view = audio.GetComponent<AudioView>();

            (GameSound Sound, string File, float Volume)[] clips =
            {
                (GameSound.StopAdded, "stop_added", 0.35f),
                (GameSound.Refused, "refused", 0.5f),
                (GameSound.Depart, "whistle", 0.6f),
                (GameSound.Boards, "boards", 0.5f),
                (GameSound.OnTime, "arrive_on_time", 0.6f),
                (GameSound.JustInTime, "just_in_time", 0.7f),
                (GameSound.Missed, "arrive_late", 0.6f),
            };
            var serialized = new SerializedObject(view);
            SerializedProperty list = serialized.FindProperty("_clips");
            list.arraySize = clips.Length;
            for (int i = 0; i < clips.Length; i++)
            {
                SerializedProperty entry = list.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("Sound").enumValueIndex = (int)clips[i].Sound;
                entry.FindPropertyRelative("AudioClip").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/{clips[i].File}.wav");
                entry.FindPropertyRelative("Volume").floatValue = clips[i].Volume;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        /// <summary>Tells the map camera how much of the screen the planning screen's side panels cover.</summary>
        private static void FitCameraBetweenPanels()
        {
            MapCameraFit fit = Object.FindObjectOfType<MapCameraFit>(true);
            var serialized = new SerializedObject(fit);
            serialized.FindProperty("_panelInsets").vector2Value = new Vector2(PlanCardsWidth + 40f, PlanPanelWidth + 40f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Points the level catalog at the five nights, the week and the UI text, and makes the style assets.</summary>
        [MenuItem("Last Train Home/Set Up Content")]
        public static void SetUpContent()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>($"{DataFolder}/LevelCatalog.asset");
            var serialized = new SerializedObject(catalog);
            string[] levels = Directory.GetFiles($"{DataFolder}/Levels", "Night*.json")
                .Select(path => path.Replace('\\', '/'))
                .OrderBy(path => path.Length).ThenBy(path => path)
                .ToArray();
            SerializedProperty list = serialized.FindProperty("_levels");
            list.arraySize = levels.Length;
            for (int i = 0; i < levels.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(levels[i]);
            }

            serialized.FindProperty("_week").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<TextAsset>($"{DataFolder}/Story/Week.json");
            serialized.FindProperty("_uiText").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<TextAsset>($"{DataFolder}/Text/UiText.json");
            serialized.ApplyModifiedPropertiesWithoutUndo();

            LoadOrCreate<UiStyle>($"{DataFolder}/UiStyle.asset");
            LoadOrCreate<KeepsakeArt>($"{DataFolder}/KeepsakeArt.asset");
            AssetDatabase.DeleteAsset($"{DataFolder}/OutcomeStyle.asset");
            AssetDatabase.SaveAssets();
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }

            return asset;
        }

        private static void LoadKit()
        {
            _style = AssetDatabase.LoadAssetAtPath<UiStyle>($"{DataFolder}/UiStyle.asset");
            _passengerArt = AssetDatabase.LoadAssetAtPath<PassengerArt>($"{DataFolder}/PassengerArt.asset");
            _keepsakeArt = AssetDatabase.LoadAssetAtPath<KeepsakeArt>($"{DataFolder}/KeepsakeArt.asset");
            _background = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Map/map_background.png");
            _kit = new UiKit(
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Caveat-Bold SDF.asset"),
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Lato-Bold SDF.asset"),
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Lato-Regular SDF.asset"),
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Lato-Italic SDF.asset"),
                SlicedSprite("Assets/Art/UI/ui_round_12.png", 14),
                SlicedSprite("Assets/Art/UI/ui_round_pill.png", 31),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Placeholder/Circle.png"));
        }

        private static Sprite SlicedSprite(string path, int border)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteBorder.x != border)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = new Vector4(border, border, border, border);
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject(CanvasName, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        /// <summary>A full-screen screen root with its view, a CanvasGroup, and optionally the station photo behind it.</summary>
        private static T Screen<T>(Transform parent, string name, Color ground, float photoBrightness) where T : ScreenView
        {
            RectTransform rect = UiKit.Fill(UiKit.Rect(name, parent));
            rect.gameObject.AddComponent<CanvasGroup>();
            if (ground.a > 0f)
            {
                _kit.Image(rect.gameObject, ground, null, raycast: true);
            }

            if (photoBrightness > 0f)
            {
                Image photo = _kit.Image(UiKit.Fill(UiKit.Rect("Photo", rect)).gameObject,
                    new Color(photoBrightness, photoBrightness, photoBrightness), _background);
                photo.preserveAspect = false;
            }

            var view = rect.gameObject.AddComponent<T>();
            return view;
        }

        private static void WireArt(Object view, bool passengers = true, bool keepsakes = false)
        {
            UiKit.Wire(view, "_style", _style);
            if (passengers)
            {
                UiKit.Wire(view, "_art", _passengerArt);
            }

            if (keepsakes)
            {
                UiKit.Wire(view, "_keepsakeArt", _keepsakeArt);
            }
        }

        // ---------------------------------------------------------------- Title

        private static TitleScreenView BuildTitle(Transform parent)
        {
            TitleScreenView view = Screen<TitleScreenView>(parent, "Title Screen", _style.Night, 0.3f);
            Transform root = view.transform;

            RectTransform column = UiKit.Pin(UiKit.Rect("Column", root), new Vector2(0f, 0.5f), new Vector2(180f, 0f), new Vector2(1500f, 760f));
            column.pivot = new Vector2(0f, 0.5f);
            UiKit.Column(column.gameObject, 30f, null, TextAnchor.MiddleLeft, expandWidth: false);

            TextMeshProUGUI title = _kit.Text(column, "Title", _kit.Hand, 198f, new Color(0.98f, 0.95f, 0.85f));
            title.lineSpacing = -30f;
            TextMeshProUGUI intro = _kit.Text(column, "Intro", _kit.Regular, 31f, new Color(0.79f, 0.74f, 0.64f));
            UiKit.Size(intro.gameObject, 1000f);
            intro.lineSpacing = 8f;

            RectTransform buttons = UiKit.Rect("Buttons", column);
            UiKit.Row(buttons.gameObject, 20f, null, TextAnchor.MiddleLeft);
            (Button begin, _, TextMeshProUGUI beginLabel) = _kit.Button(buttons, "Begin", _style.Amber, _style.Night, 30f, 84f, 340f);
            TextMeshProUGUI footnote = _kit.Text(column, "Footnote", _kit.Regular, 22f, new Color(0.66f, 0.62f, 0.53f));

            RectTransform cast = UiKit.Pin(UiKit.Rect("Cast", root), new Vector2(1f, 0f), new Vector2(-105f, 90f), new Vector2(500f, 111f));
            UiKit.Row(cast.gameObject, 18f, null, TextAnchor.LowerRight, controlWidth: false);
            PortraitView portrait = _kit.Portrait(cast, "Portrait Template", 111f, 5f);

            UiKit.Wire(view, "_title", title);
            UiKit.Wire(view, "_intro", intro);
            UiKit.Wire(view, "_footnote", footnote);
            UiKit.Wire(view, "_begin", begin);
            UiKit.Wire(view, "_beginLabel", beginLabel);
            UiKit.Wire(view, "_portraitTemplate", portrait);
            WireArt(view);
            return view;
        }

        // ----------------------------------------------------------- Night card

        private static NightCardView BuildNightCard(Transform parent)
        {
            NightCardView view = Screen<NightCardView>(parent, "Night Card", new Color(0.067f, 0.094f, 0.141f), 0f);
            Transform root = view.transform;

            RectTransform column = UiKit.Pin(UiKit.Rect("Column", root), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1240f, 980f));
            UiKit.Column(column.gameObject, 26f, null, TextAnchor.MiddleCenter, expandWidth: true);

            TextMeshProUGUI kicker = _kit.Caps(column, "Kicker", 22f, _style.Amber, TextAlignmentOptions.Center);
            TextMeshProUGUI day = _kit.Text(column, "Day", _kit.Hand, 200f, _style.Cream, TextAlignmentOptions.Center);
            day.lineSpacing = -40f;

            RectTransform featuredRow = UiKit.Rect("Featured", column);
            UiKit.Row(featuredRow.gameObject, 0f, null, TextAnchor.MiddleCenter, controlWidth: true);
            Image featuredPill = _kit.Panel(featuredRow, "Pill", new Color(0.118f, 0.165f, 0.235f));
            UiKit.Row(featuredPill.gameObject, 20f, new RectOffset(18, 30, 18, 18), TextAnchor.MiddleLeft, controlWidth: true);
            PortraitView featuredPortrait = _kit.Portrait(featuredPill.transform, "Portrait", 90f);
            TextMeshProUGUI featuredLine = _kit.Text(featuredPill.transform, "Line", _kit.Bold, 33f, _style.Cream, TextAlignmentOptions.MidlineLeft);
            featuredLine.enableWordWrapping = false;

            TextMeshProUGUI lines = _kit.Text(column, "Lines", _kit.Regular, 36f, new Color(0.9f, 0.85f, 0.76f), TextAlignmentOptions.Center);
            lines.lineSpacing = 10f;

            RectTransform week = UiKit.Rect("Week", column);
            UiKit.Size(week.gameObject, 1170f, 102f);
            HorizontalLayoutGroup weekRow = UiKit.Row(week.gameObject, 15f, null, TextAnchor.MiddleCenter, expandWidth: true);
            weekRow.childControlHeight = true;
            weekRow.childForceExpandHeight = true;
            WeekTileView tile = WeekTile(week);

            TextMeshProUGUI footnote = _kit.Text(column, "Footnote", _kit.Regular, 22f, new Color(0.66f, 0.62f, 0.53f), TextAlignmentOptions.Center);

            RectTransform buttonRow = UiKit.Rect("Buttons", column);
            UiKit.Row(buttonRow.gameObject, 0f, null, TextAnchor.MiddleCenter);
            (Button continueButton, _, TextMeshProUGUI continueLabel) = _kit.Button(buttonRow, "Continue", _style.Amber, _style.Night, 27f, 78f, 330f);

            UiKit.Wire(view, "_kicker", kicker);
            UiKit.Wire(view, "_day", day);
            UiKit.Wire(view, "_featured", featuredRow.gameObject);
            UiKit.Wire(view, "_featuredPortrait", featuredPortrait);
            UiKit.Wire(view, "_featuredLine", featuredLine);
            UiKit.Wire(view, "_lines", lines);
            UiKit.Wire(view, "_dayTemplate", tile);
            UiKit.Wire(view, "_footnote", footnote);
            UiKit.Wire(view, "_continue", continueButton);
            UiKit.Wire(view, "_continueLabel", continueLabel);
            WireArt(view);
            return view;
        }

        private static WeekTileView WeekTile(Transform parent)
        {
            Image back = _kit.Panel(parent, "Day Template", _style.Amber);
            UiKit.Size(back.gameObject, 0f, -1f, 1f);
            UiKit.Column(back.gameObject, 4f, UiKit.Pad(12, 14), TextAnchor.MiddleCenter);
            TextMeshProUGUI day = _kit.Text(back.transform, "Day", _kit.Bold, 24f, _style.Night, TextAlignmentOptions.Center);
            TextMeshProUGUI text = _kit.Text(back.transform, "Text", _kit.Regular, 20f, _style.Night, TextAlignmentOptions.Center);
            var view = back.gameObject.AddComponent<WeekTileView>();
            UiKit.Wire(view, "_back", back);
            UiKit.Wire(view, "_day", day);
            UiKit.Wire(view, "_text", text);
            return view;
        }

        // ------------------------------------------------------------- Platform

        private static PlatformView BuildPlatform(Transform parent)
        {
            PlatformView view = Screen<PlatformView>(parent, "Platform", _style.Night, 0.32f);
            Transform root = view.transform;
            RectTransform column = UiKit.Fill(UiKit.Rect("Column", root), 66f, 45f, 66f, 45f);
            UiKit.Column(column.gameObject, 30f, null, TextAnchor.UpperLeft);

            RectTransform header = UiKit.Rect("Header", column);
            UiKit.Row(header.gameObject, 40f, null, TextAnchor.UpperLeft);
            RectTransform titles = UiKit.Rect("Titles", header);
            UiKit.Column(titles.gameObject, 6f);
            UiKit.Size(titles.gameObject, -1f, -1f, 1f);
            TextMeshProUGUI kicker = _kit.Caps(titles, "Kicker", 21f, _style.Amber);
            TextMeshProUGUI headline = _kit.Text(titles, "Headline", _kit.Hand, 66f, _style.Cream);

            Image coachBox = _kit.Panel(header, "Coach", new Color(_style.Amber.r, _style.Amber.g, _style.Amber.b, 0.16f));
            UiKit.Column(coachBox.gameObject, 0f, UiKit.Pad(21, 15));
            UiKit.Size(coachBox.gameObject, 540f);
            TextMeshProUGUI coach = _kit.Text(coachBox.transform, "Text", _kit.Regular, 21f, _style.Cream);

            RectTransform cards = UiKit.Rect("Cards", column);
            UiKit.Size(cards.gameObject, -1f, -1f, -1f, 1f);
            HorizontalLayoutGroup cardRow = UiKit.Row(cards.gameObject, 24f, null, TextAnchor.UpperLeft, expandWidth: true);
            cardRow.childForceExpandHeight = true;
            PlatformCardView card = PlatformCard(cards);

            RectTransform footer = UiKit.Rect("Footer", column);
            UiKit.Row(footer.gameObject, 30f, null, TextAnchor.MiddleLeft);
            TextMeshProUGUI footerText = _kit.Text(footer, "Text", _kit.Regular, 22f, _style.CreamSoft, TextAlignmentOptions.MidlineLeft);
            UiKit.Size(footerText.gameObject, -1f, -1f, 1f);
            (Button continueButton, Image continueBack, TextMeshProUGUI continueLabel) =
                _kit.Button(footer, "Continue", _style.Amber, _style.Night, 27f, 78f, 300f);

            UiKit.Wire(view, "_kicker", kicker);
            UiKit.Wire(view, "_headline", headline);
            UiKit.Wire(view, "_coachBox", coachBox.gameObject);
            UiKit.Wire(view, "_coach", coach);
            UiKit.Wire(view, "_cardTemplate", card);
            UiKit.Wire(view, "_footer", footerText);
            UiKit.Wire(view, "_continue", continueButton);
            UiKit.Wire(view, "_continueBack", continueBack);
            UiKit.Wire(view, "_continueLabel", continueLabel);
            WireArt(view);
            return view;
        }

        private static PlatformCardView PlatformCard(Transform parent)
        {
            RectTransform card = UiKit.Rect("Card Template", parent);
            UiKit.Size(card.gameObject, 0f, -1f, 1f);
            Image ring = _kit.Panel(card, "Promise Ring", _style.Amber);
            UiKit.Fill(ring.rectTransform, -5f, -5f, -5f, -5f);
            ring.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Image back = _kit.Panel(card, "Back", _style.Paper);
            back.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            UiKit.Fill(back.rectTransform);
            UiKit.Column(card.gameObject, 15f, UiKit.Pad(24), TextAnchor.UpperLeft);

            RectTransform who = UiKit.Rect("Who", card);
            UiKit.Row(who.gameObject, 18f, null, TextAnchor.MiddleLeft);
            PortraitView portrait = _kit.Portrait(who, "Portrait", 102f);
            RectTransform names = UiKit.Rect("Names", who);
            UiKit.Column(names.gameObject, 3f);
            UiKit.Size(names.gameObject, -1f, -1f, 1f);
            TextMeshProUGUI name = _kit.Text(names, "Name", _kit.Bold, 30f, _style.Ink);
            TextMeshProUGUI subtitle = _kit.Text(names, "Subtitle", _kit.Regular, 19f, _style.InkSoft);

            TextMeshProUGUI line = _kit.Text(card, "Line", _kit.Hand, 36f, _style.Ink);
            line.lineSpacing = -12f;

            Image askedBox = _kit.Panel(card, "Asked", _style.Coach);
            UiKit.Column(askedBox.gameObject, 6f, UiKit.Pad(15, 12));
            TextMeshProUGUI askedHeader = _kit.Caps(askedBox.transform, "Header", 15f, new Color(0.48f, 0.31f, 0.05f));
            TextMeshProUGUI askedLine = _kit.Text(askedBox.transform, "Line", _kit.Regular, 21f, _style.Ink);

            RectTransform spacer = UiKit.Rect("Spacer", card);
            UiKit.Size(spacer.gameObject, -1f, 0f, -1f, 1f);

            RectTransform buttons = UiKit.Rect("Buttons", card);
            UiKit.Row(buttons.gameObject, 12f, null, TextAnchor.MiddleLeft, expandWidth: true);
            (Button ask, Image askBack, TextMeshProUGUI askLabel) = _kit.Button(buttons, "Ask", _style.PaperLight, _style.Ink, 22f, 66f);
            UiKit.Size(ask.gameObject, -1f, 66f, 1f);
            (Button promise, Image promiseBack, TextMeshProUGUI promiseLabel) = _kit.Button(buttons, "Promise", _style.PaperLight, _style.Green, 22f, 66f);
            UiKit.Size(promise.gameObject, -1f, 66f, 1f);

            var view = card.gameObject.AddComponent<PlatformCardView>();
            UiKit.Wire(view, "_promiseRing", ring.gameObject);
            UiKit.Wire(view, "_portrait", portrait);
            UiKit.Wire(view, "_name", name);
            UiKit.Wire(view, "_subtitle", subtitle);
            UiKit.Wire(view, "_line", line);
            UiKit.Wire(view, "_askedBox", askedBox.gameObject);
            UiKit.Wire(view, "_askedHeader", askedHeader);
            UiKit.Wire(view, "_askedLine", askedLine);
            UiKit.Wire(view, "_ask", ask);
            UiKit.Wire(view, "_askBack", askBack);
            UiKit.Wire(view, "_askLabel", askLabel);
            UiKit.Wire(view, "_promise", promise);
            UiKit.Wire(view, "_promiseBack", promiseBack);
            UiKit.Wire(view, "_promiseLabel", promiseLabel);
            return view;
        }

        // ----------------------------------------------------------------- Plan

        private static PlanView BuildPlan(Transform parent)
        {
            PlanView view = Screen<PlanView>(parent, "Plan", Color.clear, 0f);
            Transform root = view.transform;

            // Top centre: the night and the clock, and under it the coach line.
            Image pill = _kit.Panel(root, "Clock Pill", new Color(_style.Night.r, _style.Night.g, _style.Night.b, 0.94f), _kit.Pill);
            UiKit.Pin(pill.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(600f, 60f));
            UiKit.Row(pill.gameObject, 21f, UiKit.Pad(30, 12), TextAnchor.MiddleCenter);
            UiKit.FitBoth(pill.gameObject);
            TextMeshProUGUI night = _kit.Text(pill.transform, "Night", _kit.Bold, 22f, _style.Amber, TextAlignmentOptions.Center);
            night.enableWordWrapping = false;
            TextMeshProUGUI clock = _kit.Text(pill.transform, "Clock", _kit.Regular, 22f, _style.Cream, TextAlignmentOptions.Center);
            clock.enableWordWrapping = false;

            Image coachBox = _kit.Panel(root, "Coach", _style.Coach);
            UiKit.Pin(coachBox.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -99f), new Vector2(840f, 80f));
            UiKit.Column(coachBox.gameObject, 0f, UiKit.Pad(24, 16));
            UiKit.FitHeight(coachBox.gameObject);
            TextMeshProUGUI coach = _kit.Text(coachBox.transform, "Text", _kit.Regular, 22f, _style.Ink);

            // Left: one card per passenger.
            RectTransform cards = UiKit.Pin(UiKit.Rect("Cards", root), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(PlanCardsWidth, 1000f));
            UiKit.Column(cards.gameObject, 12f);
            PlanCardView card = PlanCard(cards);

            // Right: promise, route and buttons.
            Image panel = _kit.Panel(root, "Route Panel", _style.Paper, null, raycast: true);
            panel.rectTransform.anchorMin = new Vector2(1f, 0f);
            panel.rectTransform.anchorMax = new Vector2(1f, 1f);
            panel.rectTransform.pivot = new Vector2(1f, 0.5f);
            panel.rectTransform.offsetMin = new Vector2(-24f - PlanPanelWidth, 24f);
            panel.rectTransform.offsetMax = new Vector2(-24f, -24f);
            UiKit.Column(panel.gameObject, 18f, UiKit.Pad(24));

            Image promiseBox = _kit.Panel(panel.transform, "Promise", _style.Coach);
            UiKit.Column(promiseBox.gameObject, 5f, UiKit.Pad(18, 15));
            TextMeshProUGUI promiseHeader = _kit.Caps(promiseBox.transform, "Header", 16f, new Color(0.48f, 0.31f, 0.05f));
            TextMeshProUGUI promiseLine = _kit.Text(promiseBox.transform, "Line", _kit.Bold, 22f, _style.Ink);
            TextMeshProUGUI promiseStatus = _kit.Text(promiseBox.transform, "Status", _kit.Bold, 21f, _style.Green);

            TextMeshProUGUI routeHeader = _kit.Text(panel.transform, "Route Header", _kit.Bold, 26f, _style.Ink);
            RectTransform stopList = UiKit.Rect("Stops", panel.transform);
            UiKit.Column(stopList.gameObject, 9f);
            UiKit.Size(stopList.gameObject, -1f, -1f, -1f, 1f);
            StopRowView stop = StopRow(stopList);

            RectTransform edit = UiKit.Rect("Edit Buttons", panel.transform);
            UiKit.Row(edit.gameObject, 12f, null, TextAnchor.MiddleLeft, expandWidth: true);
            (Button undo, _, TextMeshProUGUI undoLabel) = _kit.Button(edit, "Undo", _style.Brick, Color.white, 22f, 66f);
            UiKit.Size(undo.gameObject, -1f, 66f, 1f);
            (Button clear, _, TextMeshProUGUI clearLabel) = _kit.Button(edit, "Clear", _style.Brick, Color.white, 22f, 66f);
            UiKit.Size(clear.gameObject, -1f, 66f, 1f);

            (Button depart, Image departBack, TextMeshProUGUI departLabel) = _kit.Button(panel.transform, "Depart", _style.Green, Color.white, 30f, 120f);
            UiKit.Fill(departLabel.rectTransform, 12f, 14f, 12f, 60f);
            TextMeshProUGUI departDetail = _kit.Text(depart.transform, "Detail", _kit.Regular, 19f, Color.white, TextAlignmentOptions.Top);
            UiKit.Fill(departDetail.rectTransform, 12f, 62f, 12f, 6f);

            // Bottom centre: why a click was refused.
            Image refusal = _kit.Panel(root, "Refusal", new Color(_style.Night.r, _style.Night.g, _style.Night.b, 0.94f), _kit.Pill);
            UiKit.Pin(refusal.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(800f, 60f));
            UiKit.Row(refusal.gameObject, 0f, UiKit.Pad(30, 14), TextAnchor.MiddleCenter);
            UiKit.FitBoth(refusal.gameObject);
            TextMeshProUGUI refusalText = _kit.Text(refusal.transform, "Text", _kit.Bold, 22f, _style.Cream, TextAlignmentOptions.Center);
            refusalText.enableWordWrapping = false;

            UiKit.Wire(view, "_night", night);
            UiKit.Wire(view, "_clock", clock);
            UiKit.Wire(view, "_coachBox", coachBox.gameObject);
            UiKit.Wire(view, "_coach", coach);
            UiKit.Wire(view, "_cardTemplate", card);
            UiKit.Wire(view, "_promiseBox", promiseBox.gameObject);
            UiKit.Wire(view, "_promiseBack", promiseBox);
            UiKit.Wire(view, "_promiseHeader", promiseHeader);
            UiKit.Wire(view, "_promiseLine", promiseLine);
            UiKit.Wire(view, "_promiseStatus", promiseStatus);
            UiKit.Wire(view, "_routeHeader", routeHeader);
            UiKit.Wire(view, "_stopTemplate", stop);
            UiKit.Wire(view, "_undo", undo);
            UiKit.Wire(view, "_undoLabel", undoLabel);
            UiKit.Wire(view, "_clear", clear);
            UiKit.Wire(view, "_clearLabel", clearLabel);
            UiKit.Wire(view, "_depart", depart);
            UiKit.Wire(view, "_departBack", departBack);
            UiKit.Wire(view, "_departLabel", departLabel);
            UiKit.Wire(view, "_departDetail", departDetail);
            UiKit.Wire(view, "_refusal", refusal.gameObject);
            UiKit.Wire(view, "_refusalText", refusalText);
            WireArt(view);
            return view;
        }

        private static PlanCardView PlanCard(Transform parent)
        {
            RectTransform card = UiKit.Rect("Card Template", parent);
            Image ring = _kit.Panel(card, "Promise Ring", _style.Amber);
            UiKit.Fill(ring.rectTransform, -4f, -4f, -4f, -4f);
            ring.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Image back = _kit.Panel(card, "Back", _style.PaperLight, null, raycast: true);
            back.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            UiKit.Fill(back.rectTransform);
            UiKit.Column(card.gameObject, 10f, new RectOffset(16, 16, 14, 14));

            RectTransform top = UiKit.Rect("Top", card);
            UiKit.Row(top.gameObject, 15f, null, TextAnchor.UpperLeft);
            PortraitView portrait = _kit.Portrait(top, "Portrait", 72f, 3f);
            RectTransform words = UiKit.Rect("Words", top);
            UiKit.Column(words.gameObject, 3f);
            UiKit.Size(words.gameObject, -1f, -1f, 1f);
            RectTransform nameRow = UiKit.Rect("Name Row", words);
            UiKit.Row(nameRow.gameObject, 9f, null, TextAnchor.MiddleLeft);
            TextMeshProUGUI name = _kit.Text(nameRow, "Name", _kit.Bold, 24f, _style.Ink);
            UiKit.Size(name.gameObject, -1f, -1f, 1f);
            TagView status = _kit.Tag(nameRow, "Status", 17f);
            TextMeshProUGUI quote = _kit.Text(words, "Quote", _kit.Hand, 27f, new Color(0.23f, 0.18f, 0.15f));
            quote.lineSpacing = -14f;

            Image bar = _kit.Panel(card, "Bar", new Color(0.9f, 0.85f, 0.73f), _kit.Pill);
            UiKit.Size(bar.gameObject, -1f, 12f);
            Image deadline = _kit.Image(UiKit.Rect("Deadline", bar.transform).gameObject, _style.Ink);
            deadline.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            deadline.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            deadline.rectTransform.sizeDelta = new Vector2(3f, 24f);
            Image arrival = _kit.Image(UiKit.Rect("Arrival", bar.transform).gameObject, _style.Green, _kit.Circle);
            arrival.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            arrival.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            arrival.rectTransform.sizeDelta = new Vector2(21f, 21f);

            RectTransform bottom = UiKit.Rect("Bottom", card);
            UiKit.Row(bottom.gameObject, 9f, null, TextAnchor.MiddleLeft);
            TextMeshProUGUI detail = _kit.Text(bottom, "Detail", _kit.Regular, 18f, new Color(0.29f, 0.23f, 0.18f));
            UiKit.Size(detail.gameObject, -1f, -1f, 1f);
            RectTransform pips = UiKit.Rect("Week", bottom);
            UiKit.Row(pips.gameObject, 7f, null, TextAnchor.MiddleRight, controlWidth: false);
            pips.GetComponent<HorizontalLayoutGroup>().childControlHeight = false;
            UiKit.Size(pips.gameObject, 110f, 16f);
            Image pip = _kit.Image(UiKit.Rect("Pip Template", pips).gameObject, _style.PipAhead, _kit.Circle);
            pip.rectTransform.sizeDelta = new Vector2(13f, 13f);

            var view = card.gameObject.AddComponent<PlanCardView>();
            UiKit.Wire(view, "_promiseRing", ring.gameObject);
            UiKit.Wire(view, "_portrait", portrait);
            UiKit.Wire(view, "_name", name);
            UiKit.Wire(view, "_status", status);
            UiKit.Wire(view, "_quote", quote);
            UiKit.Wire(view, "_deadline", deadline.rectTransform);
            UiKit.Wire(view, "_arrival", arrival);
            UiKit.Wire(view, "_detail", detail);
            UiKit.Wire(view, "_pipTemplate", pip);
            return view;
        }

        private static StopRowView StopRow(Transform parent)
        {
            RectTransform row = UiKit.Rect("Stop Template", parent);
            UiKit.Row(row.gameObject, 12f, null, TextAnchor.MiddleLeft);
            TextMeshProUGUI label = _kit.Text(row, "Label", _kit.Regular, 21f, _style.Ink);
            UiKit.Size(label.gameObject, -1f, -1f, 1f);
            TextMeshProUGUI time = _kit.Text(row, "Time", _kit.Regular, 21f, _style.InkSoft, TextAlignmentOptions.TopRight);
            UiKit.Size(time.gameObject, 80f);
            var view = row.gameObject.AddComponent<StopRowView>();
            UiKit.Wire(view, "_label", label);
            UiKit.Wire(view, "_time", time);
            return view;
        }

        // ----------------------------------------------------------------- Ride

        private static RideView BuildRide(Transform parent)
        {
            RideView view = Screen<RideView>(parent, "Ride", new Color(_style.Night.r, _style.Night.g, _style.Night.b, 0.5f), 0f);
            Transform root = view.transform;

            Image pill = _kit.Panel(root, "Clock Pill", new Color(_style.Night.r, _style.Night.g, _style.Night.b, 0.94f), _kit.Pill);
            UiKit.Pin(pill.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -27f), new Vector2(600f, 60f));
            UiKit.Row(pill.gameObject, 21f, UiKit.Pad(33, 15), TextAnchor.MiddleCenter);
            UiKit.FitBoth(pill.gameObject);
            TextMeshProUGUI now = _kit.Text(pill.transform, "Now", _kit.Bold, 22f, _style.Amber);
            TextMeshProUGUI station = _kit.Text(pill.transform, "Station", _kit.Regular, 22f, _style.Cream);
            TextMeshProUGUI progress = _kit.Text(pill.transform, "Progress", _kit.Regular, 22f, new Color(0.73f, 0.68f, 0.6f));
            foreach (TextMeshProUGUI text in new[] { now, station, progress })
            {
                text.enableWordWrapping = false;
            }

            // The arrival card, centred.
            RectTransform cardRoot = UiKit.Pin(UiKit.Rect("Card", root), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1170f, 420f));
            CanvasGroup cardGroup = cardRoot.gameObject.AddComponent<CanvasGroup>();
            Image ring = _kit.Panel(cardRoot, "Promise Ring", _style.Amber);
            UiKit.Fill(ring.rectTransform, -5f, -5f, -5f, -5f);
            ring.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Image cardBack = _kit.Panel(cardRoot, "Back", _style.Paper);
            cardBack.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            UiKit.Fill(cardBack.rectTransform);
            UiKit.Row(cardRoot.gameObject, 42f, new RectOffset(51, 51, 45, 45), TextAnchor.MiddleLeft);
            UiKit.FitHeight(cardRoot.gameObject);

            PortraitView portrait = _kit.Portrait(cardRoot, "Portrait", 315f, 6f);
            RectTransform words = UiKit.Rect("Words", cardRoot);
            UiKit.Column(words.gameObject, 18f);
            UiKit.Size(words.gameObject, -1f, -1f, 1f);
            RectTransform tags = UiKit.Rect("Tags", words);
            UiKit.Row(tags.gameObject, 12f, null, TextAnchor.MiddleLeft, controlWidth: true);
            TagView tag = _kit.Tag(tags, "Tag Template", 19f);
            TextMeshProUGUI title = _kit.Text(words, "Title", _kit.Bold, 33f, _style.Ink);
            TextMeshProUGUI line = _kit.Text(words, "Line", _kit.Hand, 57f, _style.Ink);
            line.lineSpacing = -18f;
            RectTransform keepsakeRow = UiKit.Rect("Keepsake", words);
            UiKit.Column(keepsakeRow.gameObject, 0f, new RectOffset(0, 0, 18, 0));
            Image rule = _kit.Image(UiKit.Rect("Rule", keepsakeRow).gameObject, new Color(0.85f, 0.78f, 0.64f));
            rule.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            rule.rectTransform.anchorMin = new Vector2(0f, 1f);
            rule.rectTransform.anchorMax = new Vector2(1f, 1f);
            rule.rectTransform.sizeDelta = new Vector2(0f, 2f);
            TextMeshProUGUI keepsake = _kit.Text(keepsakeRow, "Text", _kit.Regular, 22f, new Color(0.29f, 0.23f, 0.18f));
            TextMeshProUGUI aside = _kit.Text(words, "Aside", _kit.Italic, 22f, new Color(0.29f, 0.23f, 0.18f));

            // Bottom left: who is still aboard.
            Image aboard = _kit.Panel(root, "Still Aboard", new Color(_style.Night.r, _style.Night.g, _style.Night.b, 0.92f));
            UiKit.Pin(aboard.rectTransform, new Vector2(0f, 0f), new Vector2(36f, 36f), new Vector2(420f, 90f));
            UiKit.Row(aboard.gameObject, 15f, UiKit.Pad(21, 15), TextAnchor.MiddleLeft, controlWidth: true);
            UiKit.FitBoth(aboard.gameObject);
            TextMeshProUGUI aboardLabel = _kit.Text(aboard.transform, "Label", _kit.Regular, 21f, new Color(0.73f, 0.68f, 0.6f));
            aboardLabel.enableWordWrapping = false;
            PortraitView aboardPortrait = _kit.Portrait(aboard.transform, "Portrait Template", 60f, 3f);

            // Bottom right: countdown and continue.
            RectTransform actions = UiKit.Pin(UiKit.Rect("Actions", root), new Vector2(1f, 0f), new Vector2(-36f, 36f), new Vector2(700f, 72f));
            UiKit.Row(actions.gameObject, 18f, null, TextAnchor.MiddleRight);
            TextMeshProUGUI countdown = _kit.Text(actions, "Countdown", _kit.Regular, 21f, _style.CreamSoft, TextAlignmentOptions.MidlineRight);
            countdown.enableWordWrapping = false;
            (Button continueButton, Image continueBack, TextMeshProUGUI continueLabel) =
                _kit.Button(actions, "Continue", _style.Amber, _style.Night, 24f, 72f, 260f);

            UiKit.Wire(view, "_now", now);
            UiKit.Wire(view, "_station", station);
            UiKit.Wire(view, "_progress", progress);
            UiKit.Wire(view, "_card", cardGroup);
            UiKit.Wire(view, "_cardBack", cardBack);
            UiKit.Wire(view, "_cardRing", ring.gameObject);
            UiKit.Wire(view, "_portrait", portrait);
            UiKit.Wire(view, "_tagTemplate", tag);
            UiKit.Wire(view, "_title", title);
            UiKit.Wire(view, "_line", line);
            UiKit.Wire(view, "_keepsakeRow", keepsakeRow.gameObject);
            UiKit.Wire(view, "_keepsake", keepsake);
            UiKit.Wire(view, "_aside", aside);
            UiKit.Wire(view, "_aboard", aboard.gameObject);
            UiKit.Wire(view, "_aboardLabel", aboardLabel);
            UiKit.Wire(view, "_aboardTemplate", aboardPortrait);
            UiKit.Wire(view, "_countdown", countdown);
            UiKit.Wire(view, "_continue", continueButton);
            UiKit.Wire(view, "_continueBack", continueBack);
            UiKit.Wire(view, "_continueLabel", continueLabel);
            WireArt(view);
            return view;
        }

        // -------------------------------------------------------------- Logbook

        private static LogbookView BuildLogbook(Transform parent)
        {
            LogbookView view = Screen<LogbookView>(parent, "Logbook", _style.Night, 0f);
            Transform root = view.transform;
            RectTransform column = UiKit.Fill(UiKit.Rect("Column", root), 60f, 45f, 60f, 36f);
            UiKit.Column(column.gameObject, 24f);

            RectTransform book = UiKit.Rect("Book", column);
            UiKit.Size(book.gameObject, -1f, -1f, -1f, 1f);
            HorizontalLayoutGroup pages = UiKit.Row(book.gameObject, 9f, null, TextAnchor.UpperLeft, expandWidth: true);
            pages.childForceExpandHeight = true;

            // Left page: tonight.
            Image left = _kit.Panel(book, "Left Page", _style.Paper);
            UiKit.Size(left.gameObject, 0f, -1f, 1f);
            UiKit.Column(left.gameObject, 18f, new RectOffset(48, 48, 42, 42));
            RectTransform heading = UiKit.Rect("Heading", left.transform);
            UiKit.Row(heading.gameObject, 18f, null, TextAnchor.LowerLeft);
            TextMeshProUGUI day = _kit.Text(heading, "Day", _kit.Hand, 72f, _style.Ink);
            UiKit.Size(day.gameObject, -1f, -1f, 1f);
            TextMeshProUGUI kicker = _kit.Caps(heading, "Kicker", 18f, _style.InkSoft, TextAlignmentOptions.BottomRight);
            kicker.enableWordWrapping = false;
            RectTransform entries = UiKit.Rect("Entries", left.transform);
            UiKit.Column(entries.gameObject, 15f);
            LogEntryView entry = LogEntry(entries);
            RectTransform spacer = UiKit.Rect("Spacer", left.transform);
            UiKit.Size(spacer.gameObject, -1f, 0f, -1f, 1f);
            TextMeshProUGUI summary = _kit.Text(left.transform, "Summary", _kit.Hand, 39f, _style.Green);

            // Right page: the cab, the week ahead, tomorrow.
            Image right = _kit.Panel(book, "Right Page", _style.Paper);
            UiKit.Size(right.gameObject, 0f, -1f, 1.25f);
            UiKit.Column(right.gameObject, 21f, new RectOffset(48, 48, 42, 42));
            TextMeshProUGUI cabHeader = _kit.Caps(right.transform, "Cab Header", 18f, _style.InkSoft);
            RectTransform cab = UiKit.Rect("Cab", right.transform);
            GridLayoutGroup cabGrid = cab.gameObject.AddComponent<GridLayoutGroup>();
            cabGrid.cellSize = new Vector2(180f, 120f);
            cabGrid.spacing = new Vector2(15f, 15f);
            cabGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            cabGrid.constraintCount = 4;
            KeepsakeSlotView slot = KeepsakeSlot(cab);

            TextMeshProUGUI weekHeader = _kit.Caps(right.transform, "Week Header", 18f, _style.InkSoft);
            RectTransform week = UiKit.Rect("Week", right.transform);
            HorizontalLayoutGroup weekRow = UiKit.Row(week.gameObject, 12f, null, TextAnchor.UpperLeft, expandWidth: true);
            weekRow.childForceExpandHeight = true;
            UiKit.Size(week.gameObject);
            WeekAheadTileView tile = WeekAheadTile(week);

            Image noteBox = _kit.Panel(right.transform, "Note", _style.Coach);
            UiKit.Column(noteBox.gameObject, 6f, UiKit.Pad(24, 21));
            TextMeshProUGUI noteHeader = _kit.Caps(noteBox.transform, "Header", 17f, new Color(0.48f, 0.31f, 0.05f));
            TextMeshProUGUI note = _kit.Text(noteBox.transform, "Text", _kit.Regular, 22f, _style.Ink);

            RectTransform footer = UiKit.Rect("Footer", column);
            UiKit.Row(footer.gameObject, 30f, null, TextAnchor.MiddleLeft);
            TextMeshProUGUI footerText = _kit.Text(footer, "Text", _kit.Regular, 22f, _style.CreamSoft, TextAlignmentOptions.MidlineLeft);
            UiKit.Size(footerText.gameObject, -1f, -1f, 1f);
            (Button next, _, TextMeshProUGUI nextLabel) = _kit.Button(footer, "Next", _style.Amber, _style.Night, 27f, 78f, 420f);

            UiKit.Wire(view, "_day", day);
            UiKit.Wire(view, "_kicker", kicker);
            UiKit.Wire(view, "_entryTemplate", entry);
            UiKit.Wire(view, "_summary", summary);
            UiKit.Wire(view, "_cabHeader", cabHeader);
            UiKit.Wire(view, "_slotTemplate", slot);
            UiKit.Wire(view, "_weekHeader", weekHeader);
            UiKit.Wire(view, "_weekTemplate", tile);
            UiKit.Wire(view, "_noteBox", noteBox.gameObject);
            UiKit.Wire(view, "_noteHeader", noteHeader);
            UiKit.Wire(view, "_note", note);
            UiKit.Wire(view, "_footer", footerText);
            UiKit.Wire(view, "_next", next);
            UiKit.Wire(view, "_nextLabel", nextLabel);
            WireArt(view, keepsakes: true);
            return view;
        }

        private static LogEntryView LogEntry(Transform parent)
        {
            RectTransform row = UiKit.Rect("Entry Template", parent);
            UiKit.Row(row.gameObject, 21f, new RectOffset(0, 0, 0, 15), TextAnchor.UpperLeft);
            PortraitView portrait = _kit.Portrait(row, "Portrait", 69f, 0f);
            RectTransform words = UiKit.Rect("Words", row);
            UiKit.Column(words.gameObject, 3f);
            UiKit.Size(words.gameObject, -1f, -1f, 1f);
            RectTransform nameRow = UiKit.Rect("Name Row", words);
            UiKit.Row(nameRow.gameObject, 15f, null, TextAnchor.MiddleLeft);
            TextMeshProUGUI name = _kit.Text(nameRow, "Name", _kit.Bold, 24f, _style.Ink);
            name.enableWordWrapping = false;
            TagView tag = _kit.Tag(nameRow, "Tag", 17f);
            TextMeshProUGUI line = _kit.Text(words, "Line", _kit.Hand, 34f, _style.Ink);
            line.lineSpacing = -14f;

            var view = row.gameObject.AddComponent<LogEntryView>();
            UiKit.Wire(view, "_portrait", portrait);
            UiKit.Wire(view, "_name", name);
            UiKit.Wire(view, "_tag", tag);
            UiKit.Wire(view, "_line", line);
            return view;
        }

        private static KeepsakeSlotView KeepsakeSlot(Transform parent)
        {
            Image back = _kit.Panel(parent, "Slot Template", _style.PaperLight);
            Image picture = _kit.Image(UiKit.Rect("Picture", back.transform).gameObject, Color.white);
            picture.preserveAspect = true;
            UiKit.Fill(picture.rectTransform, 12f, 9f, 12f, 33f);
            TextMeshProUGUI label = _kit.Text(back.transform, "Label", _kit.Regular, 18f, _style.Ink, TextAlignmentOptions.Center);
            UiKit.Fill(label.rectTransform, 9f, 9f, 9f, 6f);
            label.alignment = TextAlignmentOptions.Bottom;
            var view = back.gameObject.AddComponent<KeepsakeSlotView>();
            UiKit.Wire(view, "_back", back);
            UiKit.Wire(view, "_picture", picture);
            UiKit.Wire(view, "_label", label);
            return view;
        }

        private static WeekAheadTileView WeekAheadTile(Transform parent)
        {
            RectTransform tile = UiKit.Rect("Tile Template", parent);
            UiKit.Size(tile.gameObject, 0f, -1f, 1f);
            Image ring = _kit.Panel(tile, "Next Ring", _style.Amber);
            UiKit.Fill(ring.rectTransform, -3f, -3f, -3f, -3f);
            ring.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Image back = _kit.Panel(tile, "Back", _style.PaperLight);
            back.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            UiKit.Fill(back.rectTransform);
            UiKit.Column(tile.gameObject, 6f, UiKit.Pad(15));
            TextMeshProUGUI header = _kit.Text(tile, "Header", _kit.Bold, 19f, _style.Ink);
            TextMeshProUGUI text = _kit.Text(tile, "Text", _kit.Regular, 19f, _style.Ink);
            var view = tile.gameObject.AddComponent<WeekAheadTileView>();
            UiKit.Wire(view, "_back", back);
            UiKit.Wire(view, "_nextRing", ring);
            UiKit.Wire(view, "_header", header);
            UiKit.Wire(view, "_text", text);
            return view;
        }
    }
}
