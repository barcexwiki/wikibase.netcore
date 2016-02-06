using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MinimalJson;

namespace Wikibase
{
    /// <summary>
    /// Ranks of statements.
    /// </summary>
    public enum Rank
    {
        /// <summary>
        /// Rank not defined.
        /// </summary>
        Unknown,

        /// <summary>
        /// Preferred statement.
        /// </summary>
        Preferred,

        /// <summary>
        /// Normal statement.
        /// </summary>
        Normal,

        /// <summary>
        /// Deprecated statement.
        /// </summary>
        Deprecated,
    }

    /// <summary>
    /// A statement.
    /// </summary>
    public class Statement : Claim
    {
        #region Json names

        /// <summary>
        /// The name of the <see cref="References"/> property in the serialized json object.
        /// </summary>
        private const String ReferencesJsonName = "references";

        /// <summary>
        /// The name of the <see cref="Rank"/> property in the serialized json object.
        /// </summary>
        private const String RankJsonName = "rank";

        private static Dictionary<Rank, String> _rankJsonNames = new Dictionary<Rank, String>()
        {
             {Rank.Preferred, "preferred" },
             {Rank.Normal, "normal" },
             {Rank.Deprecated, "deprecated" },
             {Rank.Unknown, "normal" } //TODO: fix this
        };

        #endregion Jscon names

        /// <summary>
        /// Gets the rank of the statement.
        /// </summary>
        /// <value>The rank of the statement.</value>
        public Rank Rank
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the collection of references assigned to the statement.
        /// </summary>
        /// <value>Collection of qualifiers.</value>
        public IEnumerable<Reference> References
        {
            get { return references; }
        }
        private List<Reference> references = new List<Reference>();

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="entity">Entity to which the statement belongs.</param>
        /// <param name="data">JSon data to be parsed.</param>
        internal Statement(Entity entity, JsonObject data)
            : base(entity, data)
        {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="entity">Entity to which the statement belongs.</param>
        /// <param name="snak">Snak for the statement.</param>
        /// <param name="rank">Rank for the statement.</param>
        internal Statement(Entity entity, Snak snak, Rank rank)
            : base(entity, snak)
        {
            this.Rank = rank;
        }

        /// <summary>
        /// Parses the <paramref name="data"/> and adds the results to this instance.
        /// </summary>
        /// <param name="data"><see cref="JsonObject"/> to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected override void FillData(JsonObject data)
        {
            if ( data == null )
                throw new ArgumentNullException("data");

            base.FillData(data);
            if ( data.get(RankJsonName) != null )
            {
                var rank = data.get(RankJsonName).asString();
                if ( _rankJsonNames.Any(x => x.Value == rank) )
                {
                    this.Rank = _rankJsonNames.First(x => x.Value == rank).Key;
                }
                else
                {
                    this.Rank = Rank.Unknown;
                }
            }
            if ( data.get(ReferencesJsonName) != null )
            {
                foreach ( JsonValue value in data.get(ReferencesJsonName).asArray() )
                {
                    Reference reference = new Reference(this, value.asObject());
                    this.references.Add(reference);
                }
            }
        }


        /// <summary>
        /// Adds a qualifier to the claim.
        /// </summary>
        public Reference AddReference(IEnumerable<Snak> snaks)
        {
            Reference reference = new Reference(this, snaks);
            AddReference(reference);
            return reference;
        }

        private Reference AddReference(Reference reference)
        {
            references.Add(reference);
            Touch();
            return reference;
        }

        /// <summary>
        /// Removes a qualifier from the claim.
        /// </summary>
        public void RemoveReference(Reference reference)
        {
            references.Remove(reference);
            Touch();
        }

        /// <summary>
        /// Create a new reference in this statement with the provided snak.
        /// </summary>
        /// <param name="snak">The snak which makes up the reference.</param>
        /// <returns>The newly created reference.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="snak"/> is <c>null</c>.</exception>
        public Reference CreateReferenceForSnak(Snak snak)
        {
            if ( snak == null )
                throw new ArgumentNullException("snak");

            return new Reference(this, new Snak[] { snak });
        }

        /// <summary>
        /// Encodes this statement in a JsonObject
        /// </summary>
        /// <returns>a JsonObject with the statement encoded.</returns>
        protected override JsonObject Encode()
        {
            JsonObject encodedClaim =  base.Encode();

            encodedClaim.add("type", "statement")
                .add("rank", _rankJsonNames[Rank]);


            JsonArray referencesSection = new JsonArray();

            foreach (Reference reference in references)
            {
                referencesSection.add(reference.Encode());
            }
            encodedClaim.add("references", referencesSection);


            return encodedClaim;
        }

    }
}