// 通常攻撃の見た目だけを担当。HP・SP・勝敗・セーブは変更しない。
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class BattleAttackMotion
{
    struct Pose
    {
        public Vector2 position;
        public Color color;
    }
    readonly Dictionary<Image, Pose> poses = new Dictionary<Image, Pose>();
    readonly List<TMP_Text> labels = new List<TMP_Text>();
    readonly TMP_FontAsset font;
    public BattleAttackMotion(TMP_FontAsset font)
    {
        this.font = font;
    }
    void Remember(Image image)
    {
        if (image != null && !poses.ContainsKey(image))
        {
            poses.Add(image, new Pose
            {
                position = image.rectTransform.anchoredPosition,
                color =
                image.color
            }
            );
        }
    }
    // 中断・Reset・Scene離脱でも必ず元の位置と色へ戻す。
    public void Restore()
    {
        foreach (var pair in poses)
        {
            if (pair.Key != null)
            {
                pair.Key.rectTransform.anchoredPosition =
                pair.Value.position;
                pair.Key.color = pair.Value.color;
            }
        }
        poses.Clear();
        foreach (var label in labels)
        {
            if (label != null)
            {
                label.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(label.gameObject);
            }
        }
        labels.Clear();
    }
    void Popup(RectTransform anchor, string value, Color color)
    {
        if (anchor == null || font == null)
        {
            return;
        }
        var canvas = anchor.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return;
        }
        var root = (RectTransform)canvas.rootCanvas.transform;
        var label = new GameObject("BattleDamagePopup",
        typeof(RectTransform),
        typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
        label.transform.SetParent(root, false);
        label.rectTransform.sizeDelta = new Vector2(240, 64);
        label.rectTransform.position =
        anchor.TransformPoint(anchor.rect.center);
        label.font = font;
        label.fontSize = 34;
        label.fontStyle = FontStyles.Bold;
        label.color = color;
        label.outlineColor = new Color(0.08f, 0.06f, 0.08f, 1);
        label.outlineWidth = .22f;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        label.text = value;
        labels.Add(label);
    }
    public IEnumerator Play(Image[] attackers, Image[] victims,
    RectTransform[] popupAnchors, int[] amounts, bool enemyAttack, bool
    healing, Action impact)
    {
        Restore();
        foreach (var image in attackers)
        {
            Remember(image);
        }
        int numberValue1;
        if (enemyAttack)
        {
            numberValue1 = -38;
        }
        else
        {
            numberValue1 = 30;
        }
        Vector2 travel = new Vector2(0, numberValue1);
        // 前進。UI座標なので720/1080のCanvasスケールに追従する。
        if (!healing)
        {
            for (float t = 0; t < .13f; t += Time.unscaledDeltaTime)
            {
                float rate = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / .13f));
                foreach (var image in attackers)
                {
                    if (image != null)
                    {
                        image.rectTransform.anchoredPosition =
                        poses[image].position + travel * rate;
                    }
                }
                yield return null;
            }
        }
        foreach (var image in attackers)
        {
            if (image != null)
            {
                Vector2 motionOffset2;
                if (healing)
                {
                    motionOffset2 = Vector2.zero;
                }
                else
                {
                    motionOffset2 = travel;
                }
                image.rectTransform.anchoredPosition =
                poses[image].position + (motionOffset2);
            }
        }
        impact();
        // 撃破時の灰色も、着弾後の表示色を基準に復元する。
        foreach (var image in victims)
        {
            Remember(image);
        }
        Color tint;
        if (healing)
        {
            tint = new Color(.5f, 1, .65f);
        }
        else
        {
            tint = new Color(1, .48f, .32f);
        }
        for (int i = 0; i < popupAnchors.Length; i++)
        {
            string displayValue4;
            if (healing)
            {
                displayValue4 = "+";
            }
            else
            {
                displayValue4 = "-";
            }
            Color colorValue5;
            if (healing)
            {
                colorValue5 = new Color(.5f, 1, .65f);
            }
            else
            {
                colorValue5 = new Color(1, .91f, .55f);
            }
            Popup(popupAnchors[i], (displayValue4) + amounts[i],
            colorValue5);
        }
        var popupStarts = new Vector2[labels.Count];
        for (int i = 0; i < labels.Count; i++)
        {
            popupStarts[i] = labels[i].rectTransform.anchoredPosition;
        }
        for (float t = 0; t < .32f; t += Time.unscaledDeltaTime)
        {
            float rate = Mathf.Clamp01(t / .18f);
            foreach (var image in attackers)
            {
                if (image != null)
                {
                    Vector2 motionOffset6;
                    if (healing)
                    {
                        motionOffset6 = Vector2.zero;
                    }
                    else
                    {
                        motionOffset6 = travel * (1 - Mathf.SmoothStep(0,
                        1, rate));
                    }
                    image.rectTransform.anchoredPosition =
                    poses[image].position + (motionOffset6);
                }
            }
            foreach (var image in victims)
            {
                if (image != null)
                {
                    float strength = Mathf.Max(0, 1 - t / .24f);
                    image.color = Color.Lerp(poses[image].color, tint,
                    strength * .8f);
                    Vector2 motionOffset7;
                    if (healing)
                    {
                        motionOffset7 = Vector2.zero;
                    }
                    else
                    {
                        motionOffset7 = new Vector2(Mathf.Sin(t * 85) * 6 *
                        strength, 0);
                    }
                    image.rectTransform.anchoredPosition =
                    poses[image].position + (motionOffset7);
                }
            }
            for (int i = 0; i < labels.Count; i++)
            {
                labels[i].rectTransform.anchoredPosition = popupStarts[i] +
                new Vector2(0, t * 60);
                labels[i].alpha = Mathf.Clamp01((.36f - t) / .12f);
            }
            yield return null;
        }
        Restore();
        yield return new WaitForSecondsRealtime(.06f);
    }
}