using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Wikibase
{
    /// <summary>
    /// A reference.
    /// </summary>
    public class Reference
    {
        /// <summary>
        /// Gets the statement this reference belongs to.
        /// </summary>
        /// <value>The statement this reference belongs to.</value>
        public Statement Statement { get; private set; }

        /// <summary>
        /// Gets the hash.
        /// </summary>
        /// <value>The hash.</value>
        public string Hash { get; private set; }

        /// <summary>
        /// Gets the internal id.
        /// </summary>
        /// <value>The internal id.</value>
        public string InternalId { get; private set; }

        /// <summary>
        /// Gets the collection of snaks assigned to the reference.
        /// </summary>
        /// <value>Collection of snaks.</value>
        public IEnumerable<Snak> Snaks
        {
            get { return _snaks; }
        }

        private List<Snak> _snaks = new List<Snak>();

        private List<EntityId> _snaksOrder = new List<EntityId>();

        /// <summary>
        /// Creates a new reference by parsing the JSon result.
        /// </summary>
        /// <param name="statement">Statement to which the new reference belongs.</param>
        /// <param name="data">JToken to parse.</param>
        internal Reference(Statement statement, JToken data)
        {
            Statement = statement;
            FillData(data);
        }

        /// <summary>
        /// Parses the <paramref name="data"/> and adds the results to this instance.
        /// </summary>
        /// <param name="data"><see cref="JToken"/> to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected void FillData(JToken data)
        {
            
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data["snaks"] != null)
            {
                foreach (JProperty member in data["snaks"])
                {
                    foreach (JToken s in member.Value)
                    {
                        Snak snak = new Snak(s);
                        _snaks.Add(snak);
                    }
                }
            }

            JToken snaksOrderSection2 = data["snaks-order"];
            if (snaksOrderSection2 != null && snaksOrderSection2.Type == JTokenType.Array)
            {
                _snaksOrder.Clear();

                foreach (JToken property in snaksOrderSection2)
                {
                    _snaksOrder.Add(new EntityId((string)property));
                }
            }

            if (data["hash"] != null)
            {
                Hash = (string)data["hash"];
            }
            if (InternalId == null)
            {
                if (Hash != null)
                {
                    InternalId = Hash;
                }
                else
                {
                    InternalId = "" + Environment.TickCount + Statement.Id;
                }
            }
        }

        /// <summary>
        /// Create a new references with the given snaks.
        /// </summary>
        /// <param name="statement">Statement to which the reference should be added.</param>
        /// <param name="snaks">Snaks to be part of the reference.</param>
        /// <returns>New reference instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="statement"/> or <paramref name="snaks"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="snaks"/> is empty.</exception>
        internal Reference(Statement statement, IEnumerable<Snak> snaks)
        {
            if (snaks == null)
                throw new ArgumentNullException(nameof(snaks));

            if (!snaks.Any())
                throw new ArgumentException("no snaks",nameof(snaks));

            Statement = statement ?? throw new ArgumentNullException(nameof(statement));

            foreach (Snak snak in snaks)
            {
                AddSnak(snak);
            }
            InternalId = Environment.TickCount + Statement.Id;
        }

        /// <summary>
        /// Get the snaks for the given property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The snaks.</returns>
        public Snak[] GetSnaks(string property)
        {
            var snakList = from s in _snaks
                           where s.PropertyId.PrefixedId.ToUpper() == property.ToUpper()
                           select s;

            return snakList.ToArray();
        }


        /// <summary>
        /// Add a snak.
        /// </summary>
        /// <param name="snak">The snak.</param>
        /// <exception cref="ArgumentNullException"><paramref name="snak"/> is <c>null</c>.</exception>
        public void AddSnak(Snak snak)
        {
            if (snak == null)
                throw new ArgumentNullException(nameof(snak));

            _snaks.Add(snak);

            if (!_snaksOrder.Contains(snak.PropertyId))
                _snaksOrder.Add(snak.PropertyId);
            Touch();
        }


        private void Touch()
        {
            Statement.Touch();
        }



        /// <summary>
        /// Remove the snak.
        /// </summary>
        /// <param name="snak">The snak.</param>
        /// <exception cref="ArgumentNullException"><paramref name="snak"/> is <c>null</c>.</exception>
        public void RemoveSnak(Snak snak)
        {
            if (snak == null)
                throw new ArgumentNullException(nameof(snak));

            _snaks.Remove(snak);
            if (!_snaks.Where(x => x.PropertyId == snak.PropertyId).Any())
            {
                _snaksOrder.Remove(snak.PropertyId);
            }
            Touch();
        }

        /// <summary>
        /// Updates instance from API call result.
        /// </summary>
        /// <param name="result">Json result.</param>
        protected void UpdateDataFromResult(JToken result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (result["reference"] != null)
            {
                FillData(result["reference"]);
            }
            Statement.Entity.UpdateLastRevisionIdFromResult(result);
        }


        /// <summary>
        /// Encodes this referecne in a JToken
        /// </summary>
        /// <returns>a JToken with the reference encoded.</returns>
        internal virtual JToken Encode()
        {
            JObject encoded = new JObject();

            JObject snaksSection = new JObject();

            foreach (EntityId property in _snaksOrder)
            {
                Snak[] snaksForTheProperty = GetSnaks(property.PrefixedId);

                if (snaksForTheProperty.Any())
                {
                    var arrayOfSnaks = new JArray();

                    foreach (Snak s in snaksForTheProperty)
                    {
                        arrayOfSnaks.Add(s.Encode());
                    }

                    snaksSection.Add(property.PrefixedId.ToUpper(), arrayOfSnaks);
                }
            }

            JArray snaksOrderSection = new JArray();
            foreach (EntityId property in _snaksOrder)
            {
                snaksOrderSection.Add(property.PrefixedId.ToUpper());
            }

            encoded.Add("snaks", snaksSection);
            encoded.Add("snaks-order", snaksOrderSection);

            if (Hash != null)
                encoded.Add("hash", Hash);

            return encoded;
        }

        /// <summary>
        /// Encodes this reference in a JSON representation
        /// </summary>
        /// <returns>string with the JSON representation of the reference</returns>
        public string ToJson()
        {
            return Encode().ToString();
        }
    }
}
