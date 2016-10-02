// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using MinimalJson;
using Wikibase.DataValues;
using System.Linq;

namespace Wikibase
{
    /// <summary>
    /// A claim
    /// </summary>
    public class Claim
    {
        internal enum ClaimStatus
        {
            Existing,
            New,
            Deleted,
            Modified
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <value>The entitiy.</value>
        public Entity Entity
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        /// <remarks>Consists of the property id plus an internal identifier. Is <c>null</c> if not saved to server yet.</remarks>
        public String Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the collection of qualifiers assigned to the claim.
        /// </summary>
        /// <value>Collection of qualifiers.</value>
        public IEnumerable<Qualifier> Qualifiers
        {
            get { return _qualifiers; }
        }
        private List<Qualifier> _qualifiers = new List<Qualifier>();

        internal ClaimStatus status;

        private Snak _mainSnak;

        private List<EntityId> _qualifiersOrder = new List<EntityId>();

        /// <summary>
        /// The main snak
        /// </summary>
        public Snak MainSnak
        {
            get
            {
                return _mainSnak;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (!_mainSnak.PropertyId.Equals(value.PropertyId))
                {
                    throw new ArgumentException("Different property id");
                }
                _mainSnak = value;
                Touch();
            }
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="entity">Entity to which the claim belongs.</param>
        /// <param name="data">JSon data to be parsed.</param>
        internal Claim(Entity entity, JsonObject data)
        {
            _qualifiers = new List<Qualifier>();
            this.Entity = entity;
            this.FillData(data);
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="entity">Entity to which the statement belongs.</param>
        /// <param name="snak">Snak for the statement.</param>
        protected Claim(Entity entity, Snak snak)
        {
            this.Entity = entity;
            _mainSnak = snak;
            _qualifiers = new List<Qualifier>();
            this.Id = this.Entity.Id.PrefixedId + "$" + Guid.NewGuid().ToString();
            this.status = ClaimStatus.New;
        }

        /// <summary>
        /// Parses the <paramref name="data"/> and adds the results to this instance.
        /// </summary>
        /// <param name="data"><see cref="JsonObject"/> to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected virtual void FillData(JsonObject data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (data.get("mainsnak") != null)
            {
                _mainSnak = new Snak(data.get("mainsnak").asObject());
            }
            if (data.get("id") != null)
            {
                this.Id = data.get("id").asString();
            }

            var qualifiersData = data.get("qualifiers");
            if (qualifiersData != null && qualifiersData.isObject())
            {
                _qualifiers.Clear();
                var qualifiersSection = qualifiersData.asObject();

                foreach (var entry in qualifiersSection.names())
                {
                    var json = qualifiersSection.get(entry).asArray();
                    foreach (var value in json)
                    {
                        var parsedQualifier = new Qualifier(this, value as JsonObject);
                        _qualifiers.Add(parsedQualifier);
                    }
                }
            }

            var qualifiersOrderSection = data.get("qualifiers-order");
            if (qualifiersOrderSection != null && qualifiersOrderSection.isArray())
            {
                _qualifiersOrder.Clear();
                var qualifiersOrderArray = qualifiersOrderSection.asArray();

                foreach (var property in qualifiersOrderArray.getValues())
                {
                    _qualifiersOrder.Add(new EntityId(property.asString()));
                }
            }

            this.status = ClaimStatus.Existing;
        }

        internal static Claim NewFromArray(Entity entity, JsonObject data)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (data.get("type") != null)
            {
                switch (data.get("type").asString())
                {
                    case "statement":
                        return new Statement(entity, data);
                    default:
                        return new Claim(entity, data);
                }
            }
            throw new ArgumentException("Unknown type in data", "data");
        }

        /// <summary>
        /// Saves the claim to the server.
        /// </summary>
        /// <param name="summary">Edit summary.</param>
        internal void Save(String summary)
        {
            Dictionary<SnakType, String> snakTypeIdentifiers = new Dictionary<SnakType, String>()
            {
                {SnakType.None,"novalue"},
                {SnakType.SomeValue,"somevalue"},
                {SnakType.Value,"value"},
            };

            JsonObject result;
            switch (this.status)
            {
                case ClaimStatus.New:
                case ClaimStatus.Modified:
                    result = this.Entity.Api.setClaim(this.Encode().ToString(), this.Entity.LastRevisionId, summary);
                    this.UpdateDataFromResult(result);
                    break;
                case ClaimStatus.Deleted:
                    result = this.Entity.Api.removeClaims(new string[] { this.Id }, this.Entity.LastRevisionId, summary);
                    this.UpdateDataFromResult(result);
                    break;
            }
        }

        /// <summary>
        /// Updates instance from API call result.
        /// </summary>
        /// <param name="result">Json result.</param>
        protected void UpdateDataFromResult(JsonObject result)
        {
            if (result == null)
                throw new ArgumentNullException("result");

            if (result.get("claim") != null)
            {
                this.FillData(result.get("claim").asObject());
            }
            this.Entity.UpdateLastRevisionIdFromResult(result);
        }

        /// <summary>
        /// Marks the claim as deleted.
        /// </summary>
        internal void Delete()
        {
            this.status = ClaimStatus.Deleted;
        }

        /// <summary>
        /// Adds a qualifier to the claim.
        /// </summary>
        public Qualifier AddQualifier(SnakType type, EntityId propertyId, DataValue dataValue)
        {
            Qualifier q = new Qualifier(this, type, propertyId, dataValue);
            AddQualifier(q);
            return q;
        }

        private Qualifier AddQualifier(Qualifier q)
        {
            _qualifiers.Add(q);
            if (!_qualifiersOrder.Contains(q.PropertyId))
                _qualifiersOrder.Add(q.PropertyId);
            Touch();
            return q;
        }

        /// <summary>
        /// Removes a qualifier from the claim.
        /// </summary>
        public void RemoveQualifier(Qualifier q)
        {
            _qualifiers.Remove(q);
            if (!_qualifiers.Where(x => x.PropertyId == q.PropertyId).Any())
            {
                _qualifiersOrder.Remove(q.PropertyId);
            }
            Touch();
        }


        /// <summary>
        /// After a change inside the class it changes the status of the claim accordingly.
        /// </summary>
        internal void Touch()
        {
            if (status == ClaimStatus.Existing)
                status = ClaimStatus.Modified;
        }

        /// <summary>
        /// Checks whether the claim is about a given property.
        /// </summary>
        /// <param name="value">Property identifier string.</param>
        /// <returns><c>true</c> if is about the property, <c>false</c> otherwise.</returns>
        public Boolean IsAboutProperty(String value)
        {
            var property = new EntityId(value);
            return property.Equals(MainSnak.PropertyId);
        }

        /// <summary>
        /// Get the qualifiers for the given property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The qualifiers.</returns>
        public Qualifier[] GetQualifiers(String property)
        {
            var qualifierList = from q in _qualifiers
                                where q.PropertyId.PrefixedId.ToUpper() == property.ToUpper()
                                select q;

            return qualifierList.ToArray();
        }



        /// <summary>
        /// Encodes this claim in a JsonObject
        /// </summary>
        /// <returns>a JsonObject with the claim encoded.</returns>
        protected virtual JsonObject Encode()
        {
            JsonObject encoded = new JsonObject()
                .add("mainsnak", MainSnak.Encode())
                .add("id", this.Id);

            JsonObject qualifiersSection = new JsonObject();

            foreach (EntityId property in _qualifiersOrder)
            {
                var qualifiersForTheProperty = GetQualifiers(property.PrefixedId);

                if (qualifiersForTheProperty.Any())
                {
                    var arrayOfQualifiers = new JsonArray();

                    foreach (Qualifier q in qualifiersForTheProperty)
                    {
                        arrayOfQualifiers.add(q.Encode());
                    }

                    qualifiersSection.add(property.PrefixedId.ToUpper(), arrayOfQualifiers);
                }
            }

            JsonArray qualifiersOrderSection = new JsonArray();
            foreach (EntityId property in _qualifiersOrder)
            {
                qualifiersOrderSection.add(property.PrefixedId.ToUpper());
            }

            encoded.add("qualifiers", qualifiersSection);
            encoded.add("qualifiers-order", qualifiersOrderSection);

            return encoded;
        }

        /// <summary>
        /// Encodes this claim in a JSON representation
        /// </summary>
        /// <returns>string with the JSON representation of the claim</returns>
        public string ToJson()
        {
            return Encode().ToString();
        }
    }
}