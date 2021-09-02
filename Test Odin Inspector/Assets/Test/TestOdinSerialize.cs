using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Serialization.Utilities;

public class TestOdinSerialize : SerializedMonoBehaviour
{
    [NonSerialized]
    [OdinSerialize]
    public int iValue;

    [Button]
    public void Test()
    {
        List<SerializationNode> nodes = new List<SerializationNode>();
        nodes.Add(new SerializationNode() {Name = "iValue", Entry = EntryType.Integer, Data = "3"});

        using (var context = Cache<DeserializationContext>.Claim())
        using (var reader = new SerializationNodeDataReader(context))
        using (var resolver = Cache<UnityReferenceResolver>.Claim())
        {
            reader.Nodes = nodes;
            context.Value.IndexReferenceResolver = resolver.Value;
            UnitySerializationUtility.DeserializeUnityObject(this, reader);
        }
    }
}
