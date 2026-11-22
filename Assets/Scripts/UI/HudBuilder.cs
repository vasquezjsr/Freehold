using Freehold.Items;
using Freehold.Needs;
using UnityEngine;
using UnityEngine.UI;

namespace Freehold.UI
{
    public static class HudBuilder
    {
        public static NeedsHUD Build(PlayerNeeds needs, Inventory inventory, out InteractionPrompt prompt)
        {
            var canvasGo = new GameObject("HUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            CreateTitle(canvasGo.transform);

            var hunger = CreateBar(canvasGo.transform, "Hunger", new Vector2(24, -24), new Color(0.2f, 0.15f, 0.1f));
            var thirst = CreateBar(canvasGo.transform, "Thirst", new Vector2(24, -58), new Color(0.1f, 0.15f, 0.25f));
            var stamina = CreateBar(canvasGo.transform, "Stamina", new Vector2(24, -92), new Color(0.1f, 0.2f, 0.12f));

            var inventoryText = CreateText(canvasGo.transform, "InventoryText", new Vector2(24, -140), 18, TextAnchor.UpperLeft, new Vector2(900, 80));
            var hintText = CreateText(canvasGo.transform, "HintText", new Vector2(24, -220), 16, TextAnchor.UpperLeft, new Vector2(1100, 40));
            hintText.color = new Color(1f, 1f, 1f, 0.75f);

            var promptText = CreateText(canvasGo.transform, "Prompt", new Vector2(0, 120), 26, TextAnchor.LowerCenter, new Vector2(900, 60));
            promptText.alignment = TextAnchor.MiddleCenter;
            promptText.fontStyle = FontStyle.Bold;

            var hudHost = canvasGo.AddComponent<NeedsHUD>();
            hudHost.Configure(needs, inventory, hunger, thirst, stamina, inventoryText, hintText);

            prompt = canvasGo.AddComponent<InteractionPrompt>();
            prompt.Bind(promptText);
            return hudHost;
        }

        static void CreateTitle(Transform parent)
        {
            var title = CreateText(parent, "Title", new Vector2(0, -20), 28, TextAnchor.UpperCenter, new Vector2(600, 40));
            title.alignment = TextAnchor.UpperCenter;
            title.fontStyle = FontStyle.Bold;
            title.text = "Freehold";
            title.color = new Color(0.95f, 0.92f, 0.85f);

            var tag = CreateText(parent, "Tagline", new Vector2(0, -52), 16, TextAnchor.UpperCenter, new Vector2(700, 30));
            tag.alignment = TextAnchor.UpperCenter;
            tag.text = "Claim your ground. Make your own way.";
            tag.color = new Color(0.85f, 0.85f, 0.8f, 0.85f);
        }

        static Image CreateBar(Transform parent, string label, Vector2 anchoredPos, Color bg)
        {
            var root = new GameObject(label + "Bar");
            root.transform.SetParent(parent, false);
            var rootRt = root.AddComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0, 1);
            rootRt.anchorMax = new Vector2(0, 1);
            rootRt.pivot = new Vector2(0, 1);
            rootRt.anchoredPosition = anchoredPos;
            rootRt.sizeDelta = new Vector2(280, 22);

            var bgImage = root.AddComponent<Image>();
            bgImage.color = bg;

            var labelText = CreateText(root.transform, "Label", new Vector2(6, 0), 14, TextAnchor.MiddleLeft, new Vector2(80, 22));
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.text = label;
            var labelRt = labelText.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0, 0.5f);
            labelRt.anchorMax = new Vector2(0, 0.5f);
            labelRt.pivot = new Vector2(0, 0.5f);
            labelRt.anchoredPosition = new Vector2(6, 0);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(root.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0, 0);
            fillRt.anchorMax = new Vector2(1, 1);
            fillRt.offsetMin = new Vector2(78, 3);
            fillRt.offsetMax = new Vector2(-4, -3);
            var fill = fillGo.AddComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            fill.color = Color.white;
            return fill;
        }

        static Text CreateText(Transform parent, string name, Vector2 anchoredPos, int fontSize, TextAnchor anchor, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();

            if (anchor == TextAnchor.UpperLeft || anchor == TextAnchor.MiddleLeft)
            {
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
            }
            else if (anchor == TextAnchor.UpperCenter)
            {
                rt.anchorMin = new Vector2(0.5f, 1);
                rt.anchorMax = new Vector2(0.5f, 1);
                rt.pivot = new Vector2(0.5f, 1);
            }
            else if (anchor == TextAnchor.LowerCenter)
            {
                rt.anchorMin = new Vector2(0.5f, 0);
                rt.anchorMax = new Vector2(0.5f, 0);
                rt.pivot = new Vector2(0.5f, 0);
            }

            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                        ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }
    }
}
