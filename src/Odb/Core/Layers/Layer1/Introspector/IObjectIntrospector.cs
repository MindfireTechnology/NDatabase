using System.Collections.Generic;
using NDatabase.Odb.Core.Layers.Layer2.Meta;

namespace NDatabase.Odb.Core.Layers.Layer1.Introspector
{
    /// <summary>
    ///   Interface for ObjectInstropector.
    /// </summary>
    /// <remarks>
    ///   Interface for ObjectInstropector.
    /// </remarks>
    /// <author>osmadja</author>
    public interface IObjectIntrospector
    {
        /// <summary>
        ///   retrieve object data
        /// </summary>
        /// <param name="object"> The object to get meta representation </param>
        /// <param name="ci"> The ClassInfo </param>
        /// <param name="recursive"> To indicate that introspection must be recursive </param>
        /// <param name="alreadyReadObjects"> A map with already read object, to avoid cyclic reference problem </param>
        /// <param name="callback"> </param>
        /// <returns> The object info </returns>
        AbstractObjectInfo GetMetaRepresentation(object @object, ClassInfo ci, bool recursive,
                                                 IDictionary<object, NonNativeObjectInfo> alreadyReadObjects,
                                                 IIntrospectionCallback callback);

        NonNativeObjectInfo BuildNnoi(object @object, ClassInfo classInfo, AbstractObjectInfo[] values,
                                      long[] attributesIdentification, int[] attributeIds,
                                      IDictionary<object, NonNativeObjectInfo> alreadyReadObjects);

        void Clear();
    }
}
