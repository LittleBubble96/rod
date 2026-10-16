using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Rod
{
    /// <summary>
    /// View 私有的保留式 UI 节点池。按父节点、名称及同名出现顺序复用，
    /// 不跨父节点搬移对象，避免弹窗、手牌与首页之间产生层级循环。
    /// 每次 Begin/End 包围一次完整布局；未借出的节点隐藏并清除业务回调。
    /// 仅允许有限的布局名称，不能用关卡 ID 等无界数据作为节点名称。
    /// 所有 GameObject 归 View 层级所有，随 View 销毁，不持有全局资源句柄。
    /// </summary>
    internal sealed class RodUINodePool
    {
        internal enum Kind { Rect, Panel, Button, Text, Icon, Group }

        /// <summary>缓存组件与唯一点击监听；刷新只替换 Callback，不叠加监听。</summary>
        internal sealed class Node
        {
            internal RectTransform Rect;
            internal Image Image;
            internal Button Button;
            internal TextMeshProUGUI Text;
            internal RodIconGraphic Icon;
            internal CanvasGroup Group;
            internal Kind Type;
            internal Action Callback;
            internal ulong Generation;

            internal void Click()
            {
                if (Rect.gameObject.activeInHierarchy && Button.interactable)
                    Callback?.Invoke();
            }
        }

        private sealed class Group
        {
            internal readonly List<Node> Nodes = new List<Node>();
            internal ulong Generation;
            internal int Used;
        }

        private sealed class Parent
        {
            internal readonly Dictionary<string, Group> Groups = new Dictionary<string, Group>();
            internal ulong Generation;
            internal int Sibling;
        }

        private readonly Dictionary<Transform, Parent> parents = new Dictionary<Transform, Parent>();
        private readonly List<Node> nodes = new List<Node>();
        private ulong generation;

        /// <summary>开始一次布局事务。原有可见节点继续保留，避免整页开关触发布局重建。</summary>
        internal void Begin() => generation++;

        /// <summary>取得本次布局节点；同一父节点下同名的多枚算筹各自有独立实例。</summary>
        internal Node Rent(Transform parent, string name, Kind kind)
        {
            if (!parents.TryGetValue(parent, out var owner))
            {
                owner = new Parent();
                parents.Add(parent, owner);
            }
            if (owner.Generation != generation)
            {
                owner.Generation = generation;
                owner.Sibling = 0;
            }
            if (!owner.Groups.TryGetValue(name, out var group))
            {
                group = new Group();
                owner.Groups.Add(name, group);
            }
            if (group.Generation != generation)
            {
                group.Generation = generation;
                group.Used = 0;
            }
            if (group.Used == group.Nodes.Count)
            {
                var created = Create(parent, name, kind);
                group.Nodes.Add(created);
                nodes.Add(created);
            }
            var node = group.Nodes[group.Used++];
            if (node.Type != kind)
                throw new InvalidOperationException("UI 节点名称对应的组件类型发生变化：" + name);
            node.Generation = generation;
            int sibling = owner.Sibling++;
            if (node.Rect.GetSiblingIndex() != sibling)
                node.Rect.SetSiblingIndex(sibling);
            if (!node.Rect.gameObject.activeSelf)
                node.Rect.gameObject.SetActive(true);
            return node;
        }

        /// <summary>归还本轮未使用的节点，包括其子节点，防止隐藏页面保留旧状态闭包。</summary>
        internal void End()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node.Generation == generation) continue;
                node.Callback = null;
                if (node.Rect.gameObject.activeSelf) node.Rect.gameObject.SetActive(false);
            }
        }

        private static Node Create(Transform parent, string name, Kind kind)
        {
            var node = new Node { Type = kind };
            node.Rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            node.Rect.SetParent(parent, false);
            if (kind == Kind.Panel || kind == Kind.Button)
                node.Image = node.Rect.gameObject.AddComponent<Image>();
            if (kind == Kind.Button)
            {
                node.Button = node.Rect.gameObject.AddComponent<Button>();
                node.Button.targetGraphic = node.Image;
                node.Button.onClick.AddListener(node.Click);
            }
            if (kind == Kind.Text) node.Text = node.Rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (kind == Kind.Icon) node.Icon = node.Rect.gameObject.AddComponent<RodIconGraphic>();
            if (kind == Kind.Group) node.Group = node.Rect.gameObject.AddComponent<CanvasGroup>();
            return node;
        }
    }
}
