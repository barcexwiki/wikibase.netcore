using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using MinimalJson;
using Wikibase.DataValues;

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

        // TODO: Changes of qualifiers

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
        /// Gets the id used internally.
        /// </summary>
        /// <value>The internally used id.</value>
        /// <remarks>Consists of the property id plus an internal identifier. It is equal to <see cref="Id"/> if the claim was parsed from server results.</remarks>
        public String InternalId
        {
            get;
            private set;
        }

        // Change to a dictionary with property ID as key?
        /// <summary>
        /// Gets the collection of qualifiers assigned to the statement.
        /// </summary>
        /// <value>Collection of qualifiers.</value>
        public ObservableCollection<Qualifier> Qualifiers
        {
            get;
            private set;
        }

        internal ClaimStatus status;

        private Snak mainSnak;

        /// <summary>
        /// The main snak
        /// </summary>
        public Snak MainSnak
        {
            get
            {
                return mainSnak;
            }
            set
            {
                if ( value == null )
                    throw new ArgumentNullException("value");

                if ( !this.mainSnak.PropertyId.Equals(value.PropertyId) )
                {
                    throw new ArgumentException("Different property id");
                }
                this.mainSnak = value;
                if (this.status == ClaimStatus.Existing)
                    this.status = ClaimStatus.Modified;
            }
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="entity">Entity to which the claim belongs.</param>
        /// <param name="data">JSon data to be parsed.</param>
        internal Claim(Entity entity, JsonObject data)
        {
            Qualifiers = new ObservableCollection<Qualifier>();
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
            this.mainSnak = snak;
            this.Id = null;
            Qualifiers = new ObservableCollection<Qualifier>();
            this.InternalId = this.Entity.Id.PrefixedId + "$" + Guid.NewGuid().ToString();
            this.status = ClaimStatus.New;
        }

        /// <summary>
        /// Parses the <paramref name="data"/> and adds the results to this instance.
        /// </summary>
        /// <param name="data"><see cref="JsonObject"/> to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected virtual void FillData(JsonObject data)
        {
            if ( data == null )
                throw new ArgumentNullException("data");

            if ( data.get("mainsnak") != null )
            {
                this.mainSnak = new Snak(data.get("mainsnak").asObject());
            }
            if ( data.get("id") != null )
            {
                this.Id = data.get("id").asString();
            }
            var qualifiersData = data.get("qualifiers");
            if ( qualifiersData != null && qualifiersData.isObject() )
            {
                var qualifiers = qualifiersData.asObject();

                foreach ( var entry in qualifiers.names() )
                {
                    var json = qualifiers.get(entry).asArray();
                    foreach ( var value in json )
                    {
                        var parsedQualifier = new Qualifier(this, value as JsonObject);
                        Qualifiers.Add(parsedQualifier);
                    }
                }
            }
            if ( this.InternalId == null )
            {
                if ( this.Id != null )
                {
                    this.InternalId = this.Id;
                }
                else
                {
                    this.InternalId = this.Entity.Id.PrefixedId+"$"+Guid.NewGuid().ToString();
                    //this.internalId = "" + Environment.TickCount + this.mMainSnak.PropertyId + this.mMainSnak.DataValue;
                }
            }

            this.status = ClaimStatus.Existing;
        }

        internal static Claim NewFromArray(Entity entity, JsonObject data)
        {
            if ( entity == null )
                throw new ArgumentNullException("entity");

            if ( data.get("type") != null )
            {
                switch ( data.get("type").asString() )
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
                    result = this.Entity.Api.setClaim(this.Encode().ToString(), this.Entity.LastRevisionId, "");
                    this.UpdateDataFromResult(result);
                    break;
                case ClaimStatus.Deleted:
                    result = this.Entity.Api.removeClaims(new string[] { this.Id }, this.Entity.LastRevisionId, "");
                    this.UpdateDataFromResult(result);
                    break;
                case ClaimStatus.Modified:
                     result = this.Entity.Api.setClaimValue(
                            this.Id,
                            snakTypeIdentifiers[this.MainSnak.Type],
                            this.MainSnak.DataValue,
                            this.Entity.LastRevisionId,
                            summary);
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
            if ( result == null )
                throw new ArgumentNullException("result");

            if ( result.get("claim") != null )
            {
                this.FillData(result.get("claim").asObject());
            }
            this.Entity.UpdateLastRevisionIdFromResult(result);
        }

        /// <summary>
        /// Marks the claim as deleted
        /// </summary>
        internal void Delete()
        {
            this.status = ClaimStatus.Deleted;
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
        /// Encodes this claim in a JsonObject
        /// </summary>
        /// <returns>a JsonObject with the claim encoded.</returns>
        protected virtual JsonObject Encode()
        {
            JsonObject encoded = new JsonObject()
                .add("mainsnak", MainSnak.Encode())
                .add("id", this.Id != null ? this.Id : this.InternalId);

            return encoded;
        }
    }
}