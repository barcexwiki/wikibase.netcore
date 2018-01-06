using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wikibase.DataValues;
using Newtonsoft.Json.Linq;

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
        public string Hash
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
        /// Creates a <see cref="Qualifier"/> from a <see cref="JToken"/>.
        /// </summary>
        /// <param name="statement">Statement to which the new qualifier belongs.</param>
        /// <param name="data">JToken to be parsed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        internal Qualifier(Claim statement, JToken data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Claim = statement;
            FillFromArray(data);
        }

        /// <summary>
        /// Fills the snak with data parsed from a JSon array.
        /// </summary>
        /// <param name="data">JSon array to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected override void FillFromArray(JToken data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            base.FillFromArray(data);
            if (data["hash"] != null)
            {
                this.Hash = (string)data["hash"];
            }
        }

    }
}