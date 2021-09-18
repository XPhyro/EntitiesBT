using System;
using System.Collections.Generic;
using EntitiesBT.Core;
using JetBrains.Annotations;
using UnityEngine;

namespace EntitiesBT.Editor
{
    public interface IBehaviorTreeNode : IDisposable
    {
        int Id { get; }
        string Name { get; }
        Vector2 Position { get; set; }
        BehaviorNodeType BehaviorType { get; }
        Type NodeType { get; }
        bool IsSelected { get; set; }

        void SetParent([CanBeNull] IBehaviorTreeNode node);
        IEnumerable<IBehaviorTreeNode> Children { get; }

        event Action OnSelected;
    }
}