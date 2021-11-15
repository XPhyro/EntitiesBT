using System;
using System.Collections.Generic;
using System.Linq;
using EntitiesBT.Components;
using EntitiesBT.Variant;
using JetBrains.Annotations;
using Nuwa.Blob;
using Nuwa.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace EntitiesBT.Editor
{
    public interface INodePropertyCreator
    {
        int Order { get; }
        bool CanCreate([NotNull] SerializedProperty property);
        [NotNull] IEnumerable<NodePropertyView> Create([NotNull] SerializedProperty property);
    }

    public static class NodePropertyHandler
    {
        private static readonly IReadOnlyList<INodePropertyCreator> _propertyCreators;

        static NodePropertyHandler()
        {
            var propertyCreators = TypeCache.GetTypesDerivedFrom<INodePropertyCreator>();
            _propertyCreators = propertyCreators
                .Select(type => (INodePropertyCreator)Activator.CreateInstance(type))
                .OrderBy(creator => creator.Order)
                .ToArray()
            ;
        }

        [NotNull, Pure]
        public static IEnumerable<NodePropertyView> CreateNodeProperty([NotNull] this SerializedProperty property)
        {
            return _propertyCreators
                .Select(creator => creator.Create(property))
                .FirstOrDefault(nodeProperty => nodeProperty.Any())
                ?? Enumerable.Empty<NodePropertyView>()
            ;
        }

        [CanBeNull]
        internal static INodePropertyCreator FindCreator([NotNull] this SerializedProperty property)
        {
            return _propertyCreators.FirstOrDefault(creator => creator.CanCreate(property));
        }
    }

    public class NodeAssetPropertyCreator : INodePropertyCreator
    {
        public int Order => 0;

        public bool CanCreate(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Generic && property.type == nameof(NodeAsset);
        }

        public IEnumerable<NodePropertyView> Create(SerializedProperty property)
        {
            if (!CanCreate(property)) yield break;

            var dynamicBuilder = property.FindPropertyRelative(nameof(NodeAsset.Builder));
            var builders = dynamicBuilder.FindPropertyRelative(nameof(DynamicBlobDataBuilder.Builders));
            var fields = dynamicBuilder.FindPropertyRelative(nameof(DynamicBlobDataBuilder.FieldNames));

            for (var i = 0; i < builders.arraySize; i++)
            {
                var builder = builders.GetArrayElementAtIndex(i);
                var creator = builder.FindCreator();
                if (creator == null) continue;

                foreach (var element in creator.Create(builder))
                {
                    element.Title = fields.GetArrayElementAtIndex(i).stringValue;
                    yield return element;
                }
            }
        }
    }

    public class VariantRWPropertyCreator : INodePropertyCreator
    {
        public int Order => 0;

        public bool CanCreate(SerializedProperty property)
        {
            var builderType = typeof(BlobVariantLinkedRWBuilder);
            return property.propertyType == SerializedPropertyType.ManagedReference &&
                   property.managedReferenceFullTypename == $"EntitiesBT.Runtime {builderType.FullName}";
        }

        public IEnumerable<NodePropertyView> Create(SerializedProperty property)
        {
            if (CanCreate(property))
            {
                var readerType = Type.GetType(property.FindPropertyRelative("_reader._variantTypeName").stringValue);
                var readerView = new NodePropertyView(readerType, Direction.Input);
                readerView.AddToClassList("reader");
                yield return readerView;

                var writerType = Type.GetType(property.FindPropertyRelative("_writer._variantTypeName").stringValue);
                var writerView = new NodePropertyView(writerType, Direction.Output);
                writerView.AddToClassList("writer");
                yield return writerView;
            }
        }
    }
}