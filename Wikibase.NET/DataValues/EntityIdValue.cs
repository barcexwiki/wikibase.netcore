using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Wikibase.DataValues
{
    /// <summary>
    /// Data value for entity ids
    /// </summary>
    public class EntityIdValue : DataValue
    {
        #region Json names

        /// <summary>
        /// The identifier of this data type in the serialized json object.
        /// </summary>
        public const string TypeJsonName = "wikibase-entityid";

        /// <summary>
        /// The name of the <see cref="NumericId"/> property in the serialized json object.
        /// </summary>
        private const string NumericIdJsonName = "numeric-id";

        /// <summary>
        /// The name of the <see cref="EntityType"/> property in the serialized json object.
        /// </summary>
        private const string EntityTypeJsonName = "entity-type";

        #endregion Json names

        private Dictionary<EntityType, string> _entityTypeJsonNames = new Dictionary<EntityType, string>()
        {
             {EntityType.Property, "property"},
             {EntityType.Item, "item"}
        };

        /// <summary>
        /// Gets or sets the entity type
        /// </summary>
        /// <value>The entity type.</value>
        /// <remarks>Should be "item" in most cases.</remarks>
        public EntityType EntityType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the numeric id.
        /// </summary>
        /// <value>The numeric id.</value>
        public int NumericId
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="entityType">The entity type ("item").</param>
        /// <param name="numericId">The numeric id.</param>
        public EntityIdValue(EntityType entityType, int numericId)
        {
            this.EntityType = entityType;
            this.NumericId = numericId;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="entityId">The entity id.</param>
        public EntityIdValue(EntityId entityId)
        {
            this.EntityType = entityId.Type;
            this.NumericId = entityId.NumericId;
        }

        /// <summary>
        /// Creates a new instance by parsing a JToken.
        /// </summary>
        /// <param name="value">JToken to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> contains data which cannot be parsed.</exception>
        internal EntityIdValue(JToken value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Type != JTokenType.Object)
                throw new ArgumentException("not a JSON object", nameof(value));

            JObject obj = (JObject)value;
            string entityTypeJson = (string)obj[EntityTypeJsonName];
            if ( !_entityTypeJsonNames.Any(x => x.Value == entityTypeJson) )
            {
                throw new ArgumentException($"Json contained unknown entity type {entityTypeJson}");
            }
            EntityType = _entityTypeJsonNames.First(x => x.Value == entityTypeJson).Key;
            NumericId = (int)obj[NumericIdJsonName];
        }

        /// <summary>
        /// Get the type of the data value.
        /// </summary>
        /// <returns>Data type identifier.</returns>
        protected override string JsonName
        {
            get { 
                return TypeJsonName;
            }
        }

        /// <summary>
        /// Encode the value part of the data value to json.
        /// </summary>
        /// <returns>The json value</returns>
        internal override JToken Encode()
        {
            JToken j = new JObject
            {
                { EntityTypeJsonName, _entityTypeJsonNames[EntityType] },
                { NumericIdJsonName, NumericId }
            };
            return j;
        }

        /// <summary>
        /// Converts the instance to a string.
        /// </summary>
        /// <returns>String representation of the instance.</returns>
        public override string ToString()
        {
            return EntityType + " " + NumericId;
        }

        /// <summary>
        /// Compares two objects of this class and determines if they are equal in value
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        private bool IsEqual(EntityIdValue other)
        {
            EntityIdValue entityId = other as EntityIdValue;

            return (entityId != null)
                && (this.EntityType == entityId.EntityType)
                && (this.NumericId == entityId.NumericId);
        }

        /// <summary>
        /// Tests for value equality.
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        public override bool Equals(object other)
        {
            // Is null?
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }

            // Is the same object?
            if (object.ReferenceEquals(this, other))
            {
                return false;
            }

            // Is the same type?
            if (other.GetType() != this.GetType())
            {
                return false;
            }

            return IsEqual((EntityIdValue)other);
        }
        
        /// <summary>
        /// Tests for value equality.
        /// </summary>
        /// <returns>True if both objects are equal in value</returns>
        public bool Equals(EntityIdValue other)
        {
            // Is null?
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }

            // Is the same object?
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            return IsEqual(other);
        }

        /// <summary>
        /// Gets the hash code of this object.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {


                // Prime numbers
                const int Base = (int)2166136261;
                const int Multiplier = 16777619;

                int hashCode = Base;
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.EntityType) ? this.EntityType.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!object.ReferenceEquals(null, this.NumericId) ? this.EntityType.GetHashCode() : 0);
                return hashCode;

            }
        }
    }
}