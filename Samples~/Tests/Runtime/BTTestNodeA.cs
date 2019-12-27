using EntitiesBT.Core;
using EntitiesBT.Editor;
using UnityEngine;

namespace EntitiesBT.Test
{
    public static class NodeA
    {
        public static int Id = 100;

        static NodeA()
        {
            VirtualMachine.Register(Id, Reset, Tick);
        }
        
        public struct Data : INodeData
        {
            public int A;
        }

        static void Reset(int index, INodeBlob blob, IBlackboard blackboard)
        {
            Debug.Log($"[A]: reset {index}");
        }

        static NodeState Tick(int index, INodeBlob blob, IBlackboard blackboard)
        {
            var data = blob.GetNodeData<Data>(index);
            var state = data.A == 0 ? NodeState.Failure : NodeState.Success;
            Debug.Log($"[A]: tick {index} {state}");
            return state;
        }
    }
    
    public class BTTestNodeA : BTNode<NodeA.Data>
    {
        public int A;
        public override int NodeId => NodeA.Id;
        public override unsafe void Build(void* dataPtr) => ((NodeA.Data*) dataPtr)->A = A;
    }
}