using System;
using EntitiesBT.Attributes;
using EntitiesBT.Core;
using Unity.Entities;

namespace EntitiesBT.Variant
{
    [Serializable]
    public class SerializedVariantRW<T> : ISerializedVariantRW<T> where T : unmanaged
    {
        [UnityEngine.SerializeField]
        private bool _isLinked = true;
        public bool IsLinked => _isLinked;

        [UnityEngine.SerializeReference]
        [HideIf(nameof(_isLinked), false)]
        [SerializeReferenceDrawer(TypeRestrictionBySiblingProperty = nameof(ReaderAndWriter))]
        private object _readerAndWriter;
        public IVariantReaderAndWriter<T> ReaderAndWriter => (IVariantReaderAndWriter<T>)_readerAndWriter;

        [UnityEngine.SerializeReference]
        [HideIf(nameof(_isLinked))]
        [SerializeReferenceDrawer(TypeRestrictionBySiblingProperty = nameof(Reader))]
        private object _reader;
        public IVariantReader<T> Reader => (IVariantReader<T>)_reader;

        [UnityEngine.SerializeReference]
        [HideIf(nameof(_isLinked))]
        [SerializeReferenceDrawer(TypeRestrictionBySiblingProperty = nameof(Writer))]
        private object _writer;
        public IVariantWriter<T> Writer => (IVariantWriter<T>)_writer;
    }

    [Serializable]
    public class SerializedVariantRO<T> : IVariantReader<T> where T : unmanaged
    {
        [UnityEngine.SerializeReference]
        [SerializeReferenceDrawer(TypeRestrictionBySiblingProperty = nameof(Reader))]
        private object _reader;
        public IVariantReader<T> Reader => (IVariantReader<T>)_reader;

        public IntPtr Allocate(ref BlobBuilder builder, ref BlobVariant blobVariant, INodeDataBuilder self, ITreeNode<INodeDataBuilder>[] tree)
        {
            return Reader.Allocate(ref builder, ref blobVariant, self, tree);
        }
    }

    [Serializable]
    public class SerializedVariantWO<T> : IVariantWriter<T> where T : unmanaged
    {
        [UnityEngine.SerializeReference]
        [SerializeReferenceDrawer(TypeRestrictionBySiblingProperty = nameof(Writer))]
        private object _writer;
        public IVariantWriter<T> Writer => (IVariantWriter<T>)_writer;

        public IntPtr Allocate(ref BlobBuilder builder, ref BlobVariant blobVariant, INodeDataBuilder self, ITreeNode<INodeDataBuilder>[] tree)
        {
            return Writer.Allocate(ref builder, ref blobVariant, self, tree);
        }
    }
}