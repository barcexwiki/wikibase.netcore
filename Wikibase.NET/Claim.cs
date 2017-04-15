using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Wikibase.DataValues;
using System.Linq;
using Newtonsoft.Json.Linq;

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
        public string Id
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
                    throw new ArgumentNullException();

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
        /// <param name="data">JToken data to be parsed.</param>
        internal Claim(Entity entity, JToken data)
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
            RefreshId();
            this.status = ClaimStatus.New;
        }

        internal void RefreshId()
        {
            if (this.Id == null)
            {
                this.Id = (Entity.Id != null ? this.Entity.Id.PrefixedId + "$" + Guid.NewGuid().ToString() : null);
            }
        }

        /// <summary>
        /// Parses the <paramref name="data"/> and adds the results to this instance.
        /// </summary>
        /// <param name="data"><see cref="JToken"/> to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected virtual void FillData(JToken data)
        {

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data["mainsnak"] != null)
            {
                _mainSnak = new Snak(data["mainsnak"]);
            }
            if (data["id"] != null)
            {
                this.Id = (string)data["id"];
            }

            JToken qualifiersData = data["qualifiers"];
            if (qualifiersData != null && qualifiersData.Type == JTokenType.Object)
            {
                _qualifiers.Clear();

                foreach (JProperty entry in qualifiersData)
                {
                    foreach (JToken value in entry.Value)
                    {
                        Qualifier parsedQualifier = new Qualifier(this, value);
                        _qualifiers.Add(parsedQualifier);
                    }
                }
            }

            JToken qualifiersOrderSection = data["qualifiers-order"];
            if (qualifiersOrderSection != null && qualifiersOrderSection.Type == JTokenType.Array)
            {
                _qualifiersOrder.Clear();                

                foreach (JToken property in qualifiersOrderSection)
                {
                    _qualifiersOrder.Add(new EntityId((string)(property)));
                }
            }

            this.status = ClaimStatus.Existing;
        }

        internal static Claim NewFromArray(Entity entity, JToken data)
        {

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (data["type"] != null)
            {
                switch ((string)data["type"])
                {
                    case "statement":
                        return new Statement(entity, data);
                    default:
                        return new Claim(entity, data);
                }
            }
            throw new ArgumentException("Unknown type in data", nameof(data));
        }

        /// <summary>
        /// Saves the claim to the server.
        /// </summary>
        /// <param name="summary">Edit summary.</param>
        internal void Save(string summary)
        {
            Dictionary<SnakType, string> snakTypeIdentifiers = new Dictionary<SnakType, string>()
            {
                {SnakType.None,"novalue"},
                {SnakType.SomeValue,"somevalue"},
                {SnakType.Value,"value"},
            };

            JToken result;
            switch (this.status)
            {
                case ClaimStatus.New:
                case ClaimStatus.Modified:
                    result = this.Entity.Api.SetClaim(this.Encode().ToString(), this.Entity.LastRevisionId, summary);
                    this.UpdateDataFromResult(result);
                    break;
                case ClaimStatus.Deleted:
                    result = this.Entity.Api.RemoveClaims(new string[] { this.Id }, this.Entity.LastRevisionId, summary);
                    this.UpdateDataFromResult(result);
                    break;
            }
        }

        /// <summary>
        /// Updates instance from API call result.
        /// </summary>
        /// <param name="result">Json result.</param>
        protected void UpdateDataFromResult(JToken result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (result["claim"] != null)
            {
                this.FillData(result["claim"]);
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
            this.Entity.Touch();
        }

        /// <summary>
        /// Checks whether the claim is about a given property.
        /// </summary>
        /// <param name="value">Property identifier string.</param>
        /// <returns><c>true</c> if is about the property, <c>false</c> otherwise.</returns>
        public bool IsAboutProperty(string value)
        {
            EntityId property = new EntityId(value);
            return property.Equals(MainSnak.PropertyId);
        }

        /// <summary>
        /// Get the qualifiers for the given property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The qualifiers.</returns>
        public Qualifier[] GetQualifiers(string property)
        {
            var qualifierList = from q in _qualifiers
                                where q.PropertyId.PrefixedId.ToUpper() == property.ToUpper()
                                select q;

            return qualifierList.ToArray();
        }



        /// <summary>
        /// Encodes this claim in a JObject
        /// </summary>
        /// <returns>a JObject with the claim encoded.</returns>
        protected virtual JObject Encode()
        {
            JObject encoded = new JObject
            {
                { "mainsnak", MainSnak.Encode() },
                { "id", this.Id }
            };

            JObject qualifiersSection = new JObject();

            foreach (EntityId property in _qualifiersOrder)
            {
                Qualifier[] qualifiersForTheProperty = GetQualifiers(property.PrefixedId);

                if (qualifiersForTheProperty.Any())
                {
                    JArray arrayOfQualifiers = new JArray();

                    foreach (Qualifier q in qualifiersForTheProperty)
                    {
                        arrayOfQualifiers.Add(q.Encode());
                    }

                    qualifiersSection.Add( new JProperty(property.PrefixedId.ToUpper(), arrayOfQualifiers));
                }
            }

            JArray qualifiersOrderSection = new JArray();
            foreach (EntityId property in _qualifiersOrder)
            {
                qualifiersOrderSection.Add(property.PrefixedId.ToUpper());
            }

            encoded.Add("qualifiers", qualifiersSection);
            encoded.Add("qualifiers-order", qualifiersOrderSection);

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