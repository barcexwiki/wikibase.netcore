using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MinimalJson;
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
        public const String TypeJsonName = "wikibase-entityid";

        /// <summary>
        /// The name of the <see cref="NumericId"/> property in the serialized json object.
        /// </summary>
        private const String NumericIdJsonName = "numeric-id";

        /// <summary>
        /// The name of the <see cref="EntityType"/> property in the serialized json object.
        /// </summary>
        private const String EntityTypeJsonName = "entity-type";

        #endregion Json names

        private Dictionary<EntityType, String> _entityTypeJsonNames = new Dictionary<EntityType, String>()
        {
             {EntityType.Property, "property" },
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
        public Int32 NumericId
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="entityType">The entity type ("item").</param>
        /// <param name="numericId">The numeric id.</param>
        public EntityIdValue(EntityType entityType, Int32 numericId)
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
        /// Creates a new instance by parsing a JsonValue.
        /// </summary>
        /// <param name="value">JSonValue to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> contains data which cannot be parsed.</exception>
        internal EntityIdValue(JsonValue value)
        {
            if ( value == null )
                throw new ArgumentNullException("value");

            JsonObject obj = value.asObject();
            var entityTypeJson = obj.get(EntityTypeJsonName).asString();
            if ( !_entityTypeJsonNames.Any(x => x.Value == entityTypeJson) )
            {
                throw new ArgumentException(String.Format("Json contained unknown entity type {0}", entityTypeJson));
            }
            this.EntityType = _entityTypeJsonNames.First(x => x.Value == entityTypeJson).Key;
            this.NumericId = obj.get(NumericIdJsonName).asInt();
        }

        /// <summary>
        /// Get the type of the data value.
        /// </summary>
        /// <returns>Data type identifier.</returns>
        protected override String JsonName()
        {
            return TypeJsonName;
        }

        /// <summary>
        /// Encode the value part of the data value to json.
        /// </summary>
        /// <returns>The json value</returns>
        internal override JsonValue Encode()
        {
            JToken j = new JObject(
                new JProperty(EntityTypeJsonName, _entityTypeJsonNames[EntityType]), 
                new JProperty(NumericIdJsonName, NumericId)
                );
            string output = j.ToString();
            return JsonValue.readFrom(output);
        }

        /// <summary>
        /// Converts the instance to a string.
        /// </summary>
        /// <returns>String representation of the instance.</returns>
        public override String ToString()
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
            if (Object.ReferenceEquals(null, other))
            {
                return false;
            }

            // Is the same object?
            if (Object.ReferenceEquals(this, other))
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
            if (Object.ReferenceEquals(null, other))
            {
                return false;
            }

            // Is the same object?
            if (Object.ReferenceEquals(this, other))
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
                hashCode = (hashCode * Multiplier) ^ (!Object.ReferenceEquals(null, this.EntityType) ? this.EntityType.GetHashCode() : 0);
                hashCode = (hashCode * Multiplier) ^ (!Object.ReferenceEquals(null, this.NumericId) ? this.EntityType.GetHashCode() : 0);
                return hashCode;

            }
        }
    }
}