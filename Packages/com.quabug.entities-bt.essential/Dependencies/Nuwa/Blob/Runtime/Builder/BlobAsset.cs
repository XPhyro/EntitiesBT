using System;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Nuwa.Blob
{
    [Serializable]
    public class BlobAsset<T> : IDisposable where T : unmanaged
    {
        [SerializeReference, UnboxSingleProperty, UnityDrawProperty] internal IBuilder Builder;

        BlobAssetReference<T> _blobAssetReference;

        public BlobAssetReference<T> Reference
        {
            get
            {
                if (!_blobAssetReference.IsCreated) _blobAssetReference = Create();
                return _blobAssetReference;
            }
        }

        public ref T Value => ref Reference.Value;

        public IBuilder FindBuilderByPath(string path)
        {
            var pathList = path.Split('.');
            return pathList.Aggregate(Builder, (builder, name) => builder.GetBuilder(name));
        }

        private unsafe BlobAssetReference<T> Create()
        {
            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<T>();
            Builder.Build(builder, new IntPtr(UnsafeUtility.AddressOf(ref root)));
            return builder.CreateBlobAssetReference<T>(Allocator.Persistent);
        }

        public void Dispose()
        {
            if (_blobAssetReference.IsCreated) _blobAssetReference.Dispose();
        }
    }
}