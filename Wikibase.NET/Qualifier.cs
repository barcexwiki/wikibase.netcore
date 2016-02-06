using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MinimalJson;
using Wikibase.DataValues;

namespace Wikibase
{
    /// <summary>
    /// Qualifier, almost identical to a <see cref="Snak"/>.
    /// </summary>
    public class Qualifier : Snak
    {
        /// <summary>
        /// Gets the claim this qualifier belongs to.
        /// </summary>
        /// <value>The claim this qualifier belongs to.</value>
        public Claim Claim
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the hash.
        /// </summary>
        /// <value>The hash.</value>
        public String Hash
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="propertyId">The property id</param>
        /// <param name="dataValue">The data value</param>
        /// <param name="claim">The claim this qualifier belongs to.</param>
        internal Qualifier(Claim claim, SnakType type, EntityId propertyId, DataValue dataValue)
            : base(type, propertyId, dataValue)
        {
            this.Claim = claim;
        }

        /// <summary>
        /// Empty constructor.
        /// </summary>
        protected Qualifier()
            : base()
        {
        }

        /// <summary>
        /// Creates a <see cref="Qualifier"/> from a <see cref="JsonObject"/>.
        /// </summary>
        /// <param name="statement">Statement to which the new qualifier belongs.</param>
        /// <param name="data">JSonObject to be parsed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        internal Qualifier(Claim statement, JsonObject data)
        {
            if ( data == null )
                throw new ArgumentNullException("data");

            this.Claim = statement;
            this.FillFromArray(data);
        }

        /// <summary>
        /// Fills the snak with data parsed from a JSon array.
        /// </summary>
        /// <param name="data">JSon array to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected override void FillFromArray(JsonObject data)
        {
            if ( data == null )
                throw new ArgumentNullException("data");

            base.FillFromArray(data);
            if ( data.get("hash") != null )
            {
                this.Hash = data.get("hash").asString();
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

            // result is a complete claim
            if ( result.get("claim") != null )
            {
                var claim = result.get("claim").asObject();
                if ( claim.get("qualifiers") != null )
                {
                    var qualifiers = claim.get("qualifiers").asObject();
                    foreach ( var entry in qualifiers.names() )
                    {
                        if ( new EntityId(entry).Equals(PropertyId) )
                        {
                            var json = qualifiers.get(entry).asArray();
                            foreach ( var value in json )
                            {
                                FillFromArray(value as JsonObject);
                            }
                        }
                    }
                }
            }

            this.Claim.Entity.UpdateLastRevisionIdFromResult(result);
        }
    }
}