using System;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace EntitiesBT.Editor
{
    public sealed class SyntaxNodeView : Node, INodeView, ITickableElement, INodePropertyContainer
    {
        private ISyntaxTreeNode _node;
        private BehaviorTreeView _graph;
        private readonly VisualElement _contentContainer;
        private readonly PropertyPortSystem _propertyPortSystem;

        public SyntaxNodeView(BehaviorTreeView graph, ISyntaxTreeNode node)
            : base(Path.Combine(Core.Utilities.GetCurrentDirectoryProjectRelativePath(), "SyntaxNodeView.uxml"))
        {
            _node = node;
            _graph = graph;
            _contentContainer = this.Q<VisualElement>("contents");

            style.left = node.Position.x;
            style.top = node.Position.y;

            this.Q<VisualElement>("left-port").Add(CreateNodePort(Direction.Input));
            this.Q<VisualElement>("right-port").Add(CreateNodePort(Direction.Output));

            _propertyPortSystem = new PropertyPortSystem(_contentContainer);

            node.OnSelected += Select;

            Port CreateNodePort(Direction direction)
            {
                var port = InstantiatePort(Orientation.Horizontal, direction, Port.Capacity.Multi, node.VariantType);
                port.portName = "";
                port.AddToClassList(Utilities.VariantPortClass);
                port.AddToClassList(Utilities.VariantAccessMode(node.VariantType));
                return port;
            }
        }

        public void SyncPosition()
        {
            _node.Position = GetPosition().position;
        }

        public void Connect(ConnectableVariant variant)
        {
            _node.Connect(variant);
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

        public NodePropertyView FindByPort(Port port)
        {
            return _propertyPortSystem.Find(port);
        }
    }
}