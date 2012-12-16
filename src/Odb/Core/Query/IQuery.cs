using System;
using NDatabase2.Odb.Core.Query.Criteria;

namespace NDatabase2.Odb.Core.Query
{
    public interface IQuery
    {
        /// <summary>
        ///   To order by the result of a query in descendent order
        /// </summary>
        /// <param name="fields"> A comma separated field list </param>
        /// <returns> this </returns>
        IQuery OrderByDesc(string fields);

        /// <summary>
        ///   To order by the result of a query in ascendent order
        /// </summary>
        /// <param name="fields"> A comma separated field list </param>
        /// <returns> this </returns>
        IQuery OrderByAsc(string fields);

        /// <summary>
        ///   Returns true if the query has an order by clause
        /// </summary>
        /// <returns> true if has an order by flag </returns>
        bool HasOrderBy();

        /// <summary>
        ///   Returns the field names of the order by
        /// </summary>
        /// <returns> The array of fields of the order by </returns>
        string[] GetOrderByFieldNames();

        /// <returns> the type of the order by - ORDER_BY_NONE,ORDER_BY_DESC,ORDER_BY_ASC </returns>
        OrderByConstants GetOrderByType();

        /// <summary>
        ///   To indicate if a query must be executed on a single object with the specific OID.
        /// </summary>
        /// <remarks>
        ///   To indicate if a query must be executed on a single object with the specific OID. Used for ValuesQeuries
        /// </remarks>
        /// <returns> </returns>
        bool IsForSingleOid();

        /// <summary>
        ///   used with isForSingleOid == true, to indicate we are working on a single object with a specific oid
        /// </summary>
        /// <returns> </returns>
        OID GetOidOfObjectToQuery();

        bool Match(object @object);

        Type UnderlyingType { get; }

        IConstraint Equal<TItem>(string attributeName, TItem value);
        void Constrain(IConstraint criterion);
        IConstraint LessOrEqual<TItem>(string attributeName, TItem value) where TItem : IComparable;
        IConstraint InvariantEqual(string attributeName, string value);
        IConstraint Like(string attributeName, string value);
        IConstraint InvariantLike(string attributeName, string value);
        IConstraint GreaterThan<TItem>(string attributeName, TItem value) where TItem : IComparable;
        IConstraint GreaterOrEqual<TItem>(string attributeName, TItem value) where TItem : IComparable;
        IConstraint LessThan<TItem>(string attributeName, TItem value) where TItem : IComparable;
        IConstraint Contain<TItem>(string attributeName, TItem value);
        IConstraint IsNull(string attributeName);
        IConstraint IsNotNull(string attributeName);
        IConstraint SizeEq(string attributeName, int size);
        IConstraint SizeNe(string attributeName, int size);
        IConstraint SizeGt(string attributeName, int size);
        IConstraint SizeGe(string attributeName, int size);
        IConstraint SizeLt(string attributeName, int size);
        IConstraint SizeLe(string attributeName, int size);
    }
}
