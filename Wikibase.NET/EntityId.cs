﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Wikibase
{
    /// <summary>
    /// Supported entity types.
    /// </summary>
    public enum EntityType
    {
        /// <summary>
        /// Properties, Wikidata URL is https://www.wikidata.org/wiki/Property:P###.
        /// </summary>
        Property,

        /// <summary>
        /// Items, Wikidata URL is https://www.wikidata.org/wiki/Q###.
        /// </summary>
        Item
    }

    /// <summary>
    /// Represents an ID of an Entity.
    /// </summary>
    public class EntityId
    {
        private Dictionary<EntityType, string> _entityTypePrefixes = new Dictionary<EntityType, string>
        {
            {EntityType.Item, "q"},
            {EntityType.Property, "p"},
        };

        /// <summary>
        /// The allowed prefixes for entity ids
        /// </summary>
        private static readonly string[] s_prefixes = new string[] { "q", "p" };

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <value>The entity type.</value>
        public EntityType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the prefix.
        /// </summary>
        /// <value>The prefix.</value>
        public string Prefix
        {
            get
            {
                return _entityTypePrefixes[Type];
            }
            private set
            {
                SetPrefix(value);
            }
        }

        /// <summary>
        /// Gets the numeric id.
        /// </summary>
        /// <value>The numeric id.</value>
        public int NumericId
        {
            get;
            private set;
        }

        private static Regex s_prefixedIdRegex = new Regex(@"^(\w)(\d+)(#.*|)$");

        private void SetPrefix(string prefix)
        {
            string prefixToFind = CultureInfo.InvariantCulture.TextInfo.ToLower(prefix);
            if (!_entityTypePrefixes.Values.Contains(prefixToFind))
            {
                throw new ArgumentException($"\"{prefix}\" is no recognized prefix");
            }
            Type = _entityTypePrefixes.First(x => x.Value == prefixToFind).Key;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="prefix">The prefix</param>
        /// <param name="numericId">The numeric id</param>
        public EntityId(string prefix, int numericId)
        {
            Prefix = prefix;
            NumericId = numericId;
        }

        /// <summary>
        /// Constructs an entity id from a prefixed id.
        /// </summary>
        /// <param name="prefixedId">The prefixed id.</param>
        public EntityId(string prefixedId)
        {
            bool success = false;
            if (!string.IsNullOrWhiteSpace(prefixedId))
            {
                Match match = s_prefixedIdRegex.Match(CultureInfo.InvariantCulture.TextInfo.ToLower(prefixedId));

                if (match.Success)
                {
                    if (Array.Exists(s_prefixes, delegate (string s)  { return s == match.Groups[1].Value; }))
                    {
                        NumericId = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                        Prefix = match.Groups[1].Value;
                        success = true;
                    }
                }
            }
            if (!success)
            {
                throw new ArgumentException($"\"{prefixedId}\" is not a parseable prefixed id");
            }
        }

        /// <summary>
        /// Gets the prefixed id of the entity id.
        /// </summary>
        /// <value>The prefixed id.</value>
        public string PrefixedId
        {
            get
            {
                return Prefix + NumericId;
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same
        /// type.
        ///</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.</returns>
        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (other == null)
            {
                return false;
            }
            if (this.GetType() != other.GetType())
            {
                return false;
            }
            EntityId otherId = (EntityId)other;
            return Type == otherId.Type && NumericId == otherId.NumericId;
        }

        /// <summary>
        /// Gets a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return (Type.GetHashCode() * 3) ^ (NumericId.GetHashCode() * 7);
        }

        /// <summary>
        /// Converts a entity Id to a string.
        /// </summary>
        /// <returns>Entity Id as a string.</returns>
        public override string ToString()
        {
            return PrefixedId;
        }
    }
}