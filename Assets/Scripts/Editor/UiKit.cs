using LastTrainHome.Game.Presentation;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LastTrainHome.Editor
{
    /// <summary>
    /// Small builders for uGUI objects, so <see cref="UiBuilder"/> reads as a description of each screen. Sizes are in
    /// canvas units at the 1920×1080 reference resolution.
    /// </summary>
    internal sealed class UiKit
    {
        public UiKit(TMP_FontAsset hand, TMP_FontAsset bold, TMP_FontAsset regular, TMP_FontAsset italic,
            Sprite rounded, Sprite pill, Sprite circle)
        {
            Hand = hand;
            Bold = bold;
            Regular = regular;
            Italic = italic;
            Rounded = rounded;
            Pill = pill;
            Circle = circle;
        }

        public TMP_FontAsset Hand { get; }

        public TMP_FontAsset Bold { get; }

        public TMP_FontAsset Regular { get; }

        public TMP_FontAsset Italic { get; }

        public Sprite Rounded { get; }

        public Sprite Pill { get; }

        public Sprite Circle { get; }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        /// <summary>Stretches to the parent's edges, inset by the given margins.</summary>
        public static RectTransform Fill(RectTransform rect, float left = 0f, float top = 0f, float right = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        /// <summary>Pins to one point of the parent (anchor and pivot alike), offset and sized.</summary>
        public static RectTransform Pin(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public Image Image(GameObject go, Color color, Sprite sprite = null, bool raycast = false)
        {
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycast;
            if (sprite != null && sprite.border != Vector4.zero)
            {
                image.type = UnityEngine.UI.Image.Type.Sliced;
            }

            return image;
        }

        public Image Panel(Transform parent, string name, Color color, Sprite sprite = null, bool raycast = false) =>
            Image(Rect(name, parent).gameObject, color, sprite ?? Rounded, raycast);

        public TextMeshProUGUI Text(Transform parent, string name, TMP_FontAsset font, float size, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft, FontStyles style = FontStyles.Normal)
        {
            RectTransform rect = Rect(name, parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            text.richText = true;
            text.text = name;
            return text;
        }

        /// <summary>A label in small capitals with wide letter spacing, as section headers are drawn.</summary>
        public TextMeshProUGUI Caps(Transform parent, string name, float size, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
        {
            TextMeshProUGUI text = Text(parent, name, Bold, size, color, alignment, FontStyles.UpperCase);
            text.characterSpacing = 8f;
            return text;
        }

        public static VerticalLayoutGroup Column(GameObject go, float spacing, RectOffset padding = null,
            TextAnchor alignment = TextAnchor.UpperLeft, bool expandWidth = true)
        {
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset();
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = expandWidth;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static HorizontalLayoutGroup Row(GameObject go, float spacing, RectOffset padding = null,
            TextAnchor alignment = TextAnchor.MiddleLeft, bool expandWidth = false, bool controlWidth = true)
        {
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset();
            layout.childAlignment = alignment;
            layout.childControlWidth = controlWidth;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = expandWidth;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static LayoutElement Size(GameObject go, float width = -1f, float height = -1f, float flexibleWidth = -1f,
            float flexibleHeight = -1f, float minHeight = -1f)
        {
            LayoutElement element = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            element.preferredWidth = width;
            element.preferredHeight = height;
            element.minWidth = width;
            element.minHeight = minHeight >= 0f ? minHeight : height;
            // Zero rather than "ignore": a layout group that force-expands its children reports itself as flexible,
            // which would stretch it in its own parent.
            element.flexibleWidth = Mathf.Max(0f, flexibleWidth);
            element.flexibleHeight = Mathf.Max(0f, flexibleHeight);
            return element;
        }

        public static ContentSizeFitter FitHeight(GameObject go)
        {
            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return fitter;
        }

        public static ContentSizeFitter FitBoth(GameObject go)
        {
            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return fitter;
        }

        public static RectOffset Pad(int all) => new RectOffset(all, all, all, all);

        public static RectOffset Pad(int horizontal, int vertical) => new RectOffset(horizontal, horizontal, vertical, vertical);

        /// <summary>A button: a rounded background with a centred label. Its height comes from the layout.</summary>
        public (Button Button, Image Back, TextMeshProUGUI Label) Button(Transform parent, string name, Color back,
            Color textColor, float fontSize, float height, float width = -1f)
        {
            Image image = Panel(parent, name, back, Rounded, raycast: true);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0.55f);
            button.colors = colors;
            Size(image.gameObject, width, height);
            TextMeshProUGUI label = Text(image.transform, "Label", Bold, fontSize, textColor, TextAlignmentOptions.Center);
            Fill(label.rectTransform, 12f, 0f, 12f, 0f);
            return (button, image, label);
        }

        /// <summary>A portrait in a frame of the passenger's colour.</summary>
        public PortraitView Portrait(Transform parent, string name, float size, float frame = 4f)
        {
            Image back = Panel(parent, name, Color.white, Rounded);
            Size(back.gameObject, size, size);
            Pin(back.rectTransform, new Vector2(0f, 1f), Vector2.zero, new Vector2(size, size));
            Image picture = Panel(back.transform, "Picture", Color.white, Rounded);
            Fill(picture.rectTransform, frame, frame, frame, frame);
            picture.preserveAspect = false;
            var view = back.gameObject.AddComponent<PortraitView>();
            Wire(view, "_frame", back);
            Wire(view, "_portrait", picture);
            return view;
        }

        /// <summary>A pill-shaped tag that fits its text.</summary>
        public TagView Tag(Transform parent, string name, float fontSize)
        {
            Image back = Panel(parent, name, Color.white, Pill);
            Row(back.gameObject, 0f, Pad(Mathf.RoundToInt(fontSize * 0.7f), Mathf.RoundToInt(fontSize * 0.25f)), TextAnchor.MiddleCenter);
            TextMeshProUGUI text = Text(back.transform, "Text", Bold, fontSize, Color.black, TextAlignmentOptions.Center);
            text.enableWordWrapping = false;
            var view = back.gameObject.AddComponent<TagView>();
            Wire(view, "_back", back);
            Wire(view, "_text", text);
            return view;
        }

        /// <summary>Sets a serialized field, private or not, as the Inspector would.</summary>
        public static void Wire(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
            {
                throw new System.ArgumentException($"{target.GetType().Name} has no serialized field '{field}'.");
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
