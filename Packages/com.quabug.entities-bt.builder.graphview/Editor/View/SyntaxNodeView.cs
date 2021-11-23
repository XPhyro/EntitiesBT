using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace EntitiesBT.Editor
{
    public sealed class SyntaxNodeView : Node, INodeView, ITickableElement, IConnectableVariantViewContainer, IVariantPortContainer
    {
        private ISyntaxTreeNode _node;
        private BehaviorTreeView _graph;
        private readonly VisualElement _contentContainer;
        private readonly PropertyPortSystem _propertyPortSystem;

        public IReadOnlyList<Port> Ports { get; }

        public SyntaxNodeView(BehaviorTreeView graph, ISyntaxTreeNode node)
            : base(Path.Combine(Core.Utilities.GetCurrentDirectoryProjectRelativePath(), "SyntaxNodeView.uxml"))
        {
            _node = node;
            _graph = graph;
            _contentContainer = this.Q<VisualElement>("contents");

            style.left = node.Position.x;
            style.top = node.Position.y;

            Ports = new[]
            {
                Utilities.CreateVariantPort(Direction.Input, Port.Capacity.Multi, node.VariantType),
                Utilities.CreateVariantPort(Direction.Output, Port.Capacity.Multi, node.VariantType)
            };
            this.Q<VisualElement>("left-port").Add(Ports[0]);
            this.Q<VisualElement>("right-port").Add(Ports[1]);

            _propertyPortSystem = new PropertyPortSystem(_contentContainer);

            node.OnSelected += Select;
        }

        public void SyncPosition()
        {
            _node.Position = GetPosition().position;
        }

        public void Connect(ConnectableVariant variant, int variantPortIndex, Port syntaxNodePort)
        {
            var syntaxNodePortIndex = Ports.IndexOf(syntaxNodePort);
            _node.Connect(variant, variantPortIndex: variantPortIndex, syntaxNodePortIndex: syntaxNodePortIndex);
        }

        public void Disconnect(ConnectableVariant variant)
        {
            _node.Disconnect(variant);
        }

        public void Dispose()
        {
            _node.OnSelected -= Select;
            _node.Dispose();
        }

        public void Tick()
        {
            title = _node.Name;
            _propertyPortSystem.Refresh(_node);
            var variantType = _node.VariantType;
            foreach (var port in Ports.Where(p => p.portType != variantType)) port.portType = variantType;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            _node.IsSelected = true;
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            _node.IsSelected = false;
        }

        private void Select()
        {
            if (!_graph.selection.Contains(this))
            {
                Select(_graph, additive: false);
                _graph.FrameSelection();
            }
        }

        public ConnectableVariantView FindByPort(Port port)
        {
            return _propertyPortSystem.Find(port);
        }
    }
}