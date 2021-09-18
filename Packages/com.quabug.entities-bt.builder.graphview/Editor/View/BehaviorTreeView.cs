using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EntitiesBT.Core;
using JetBrains.Annotations;
using Shtif;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace EntitiesBT.Editor
{
    public class BehaviorTreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BehaviorTreeView, UxmlTraits> {}

        private IBehaviorTreeGraph _graph;

        public BehaviorTreeView()
        {
            Insert(0, new GridBackground { name = "grid" });

            var miniMap = new MiniMap();
            miniMap.graphView = this;
            miniMap.windowed = true;
            miniMap.name = "minimap";
            Add(miniMap);

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public void Reset([CanBeNull] IBehaviorTreeGraph graph)
        {
            graphViewChanged -= OnGraphChanged;

            _graph = graph;
            DeleteElements(graphElements.ToList());

            if (_graph != null)
            {
                _graph.RecreateData();
                Debug.Log($"open behavior tree graph: {_graph.Name}");
                foreach (var node in _graph.RootNodes) CreateNode(node);
            }

            graphViewChanged += OnGraphChanged;

            GraphViewChange OnGraphChanged(GraphViewChange @event)
            {
                if (@event.elementsToRemove != null)
                {
                    foreach (var edge in @event.elementsToRemove.OfType<Edge>()) OnEdgeDeleted(edge);
                    foreach (var node in @event.elementsToRemove.OfType<NodeView>().ToArray())
                    {
                        node.Dispose();
                        @event.elementsToRemove.AddRange(edges.ToList().Where(edge => edge.input.node == node || edge.output.node == node));
                    }
                }

                if (@event.edgesToCreate != null)
                {
                    foreach (var edge in @event.edgesToCreate) OnEdgeCreated(edge);
                }

                if (@event.movedElements != null)
                {
                    foreach (var node in @event.movedElements.OfType<NodeView>()) node.SyncPosition();
                }

                return @event;
            }

            void OnEdgeCreated(Edge edge)
            {
                edge.showInMiniMap = true;
                if (edge.input.node is NodeView inputNode && edge.output.node is NodeView outputNode)
                    outputNode.ConnectTo(inputNode);
            }

            void OnEdgeDeleted(Edge edge)
            {
                if (edge.input.node is NodeView inputNode && edge.output.node is NodeView outputNode)
                    inputNode.DisconnectFrom(outputNode);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (port.direction != startPort.direction && port.node != startPort.node && port.portType == startPort.portType)
                    compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var context = new GenericMenu();
            FillContextMenu();
            var popup = GenericMenuPopup.Get(context, "");
            popup.showSearch = true;
            popup.showTooltip = false;
            popup.resizeToContent = true;
            popup.Show(evt.mousePosition);

            void FillContextMenu()
            {
                if (selection != null && selection.Any())
                {
                    context.AddItem(new GUIContent("Delete"), false, () => DeleteSelection());
                    context.AddSeparator("");
                }

                var types = TypeCache.GetTypesWithAttribute<BehaviorNodeAttribute>();
                foreach (var (type, attribute) in (
                    from type in types
                    from attribute in type.GetCustomAttributes<BehaviorNodeAttribute>()
                    where !attribute.Ignore
                    select (type, attribute)
                ).OrderBy(t => t.type.Name))
                {
                    var path = $"{attribute.Type}/{type.Name}";
                    var action = AddNode(type, viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition));
                    context.AddItem(new GUIContent(path), false, action);
                }
            }

            GenericMenu.MenuFunction AddNode(Type type, Vector2 position)
            {
                return () =>
                {
                    var node = _graph.AddNode(type, position);
                    CreateNode(node);
                };
            }
        }

        // TODO: optimize?
        private NodeView CreateNode(IBehaviorTreeNode node)
        {
            var nodeView = new NodeView(node);
            AddElement(nodeView);
            foreach (var child in node.Children)
            {
                var childView = CreateNode(child);
                var edge = nodeView.Output.ConnectTo(childView.Input);
                AddElement(edge);
            }
            return nodeView;
        }
    }
}