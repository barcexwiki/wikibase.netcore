using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

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
        private const string ReferencesJsonName = "references";

        /// <summary>
        /// The name of the <see cref="Rank"/> property in the serialized json object.
        /// </summary>
        private const string RankJsonName = "rank";

        private static Dictionary<Rank, string> s_rankJsonNames = new Dictionary<Rank, string>()
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
            get { return _references; }
        }
        private List<Reference> _references = new List<Reference>();

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="entity">Entity to which the statement belongs.</param>
        /// <param name="data">JSon data to be parsed.</param>
        internal Statement(Entity entity, JToken data)
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
        /// <param name="data"><see cref="JToken"/> to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected override void FillData(JToken data)
        {

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            base.FillData(data);
            if (data[RankJsonName] != null)
            {
                string rank = (string)data[RankJsonName];
                if (s_rankJsonNames.Any(x => x.Value == rank))
                {
                    this.Rank = s_rankJsonNames.First(x => x.Value == rank).Key;
                }
                else
                {
                    this.Rank = Rank.Unknown;
                }
            }
            if (data[ReferencesJsonName] != null)
            {
                _references.Clear();
                foreach (JToken value in data[ReferencesJsonName])
                {
                    Reference reference = new Reference(this, JToken.Parse(value.ToString()) );
                    _references.Add(reference);
                }
            }
        }


        /// <summary>
        /// Adds a new reference in this statement with the provided snaks.
        /// </summary>
        /// <param name="snaks">The snak which makes up the reference.</param>
        /// <returns>The newly created reference.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="snaks"/> is <c>null</c>.</exception>
        public Reference AddReference(IEnumerable<Snak> snaks)
        {
            if (snaks == null)
                throw new ArgumentNullException(nameof(snaks));

            Reference reference = new Reference(this, snaks);
            AddReference(reference);
            return reference;
        }

        /// <summary>
        /// Adds a new reference in this statement with the provided snak.
        /// </summary>
        /// <param name="snak">The snak which makes up the reference.</param>
        /// <returns>The newly created reference.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="snak"/> is <c>null</c>.</exception>
        public Reference AddReference(Snak snak)
        {
            if (snak == null)
                throw new ArgumentNullException(nameof(snak));

            return AddReference(new Snak[] { snak });
        }


        private Reference AddReference(Reference reference)
        {
            _references.Add(reference);
            Touch();
            return reference;
        }

        /// <summary>
        /// Removes a qualifier from the claim.
        /// </summary>
        public void RemoveReference(Reference reference)
        {
            _references.Remove(reference);
            Touch();
        }

        /// <summary>
        /// Encodes this statement in a JObject
        /// </summary>
        /// <returns>a JObject with the statement encoded.</returns>
        protected override JObject Encode()
        {
            JObject encodedClaim = base.Encode();

            encodedClaim.Add("type", "statement");
            encodedClaim.Add("rank", s_rankJsonNames[Rank]);

            JArray referencesSection = new JArray();

            foreach (Reference reference in _references)
            {
                referencesSection.Add(reference.Encode());
            }
            encodedClaim.Add("references", referencesSection);


            return encodedClaim;
        }
    }
}