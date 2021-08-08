using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Serialization.Utilities;
using UnityEngine;

public class TestOdinSerialize : SerializedMonoBehaviour
{
    [OdinSerialize]
    [NonSerialized]
    public int iValue;
    public Action @delegate;

    [Button]
    public void Test()
    {
        List<SerializationNode> nodes = new List<SerializationNode>();
        nodes.Add(new SerializationNode() {Name = "iValue", Data = "3"});

        using (var context = Cache<SerializationContext>.Claim())
        using (var writer = new SerializationNodeDataWriter(context))
        using (var resolver = Cache<UnityReferenceResolver>.Claim())
        {
            writer.Nodes = nodes;

            //resolver.Value.SetReferencedUnityObjects(data.ReferencedUnityObjects);
            context.Value.IndexReferenceResolver = resolver.Value;

            UnitySerializationUtility.SerializeUnityObject(this, writer);
        }
    }
}
