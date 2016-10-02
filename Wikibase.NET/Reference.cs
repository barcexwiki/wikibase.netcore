using System;
using System.Collections.Generic;
using System.Text;
using MinimalJson;
using System.Linq;

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
        public String Hash { get; private set; }

        /// <summary>
        /// Gets the internal id.
        /// </summary>
        /// <value>The internal id.</value>
        public String InternalId { get; private set; }

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
        /// <param name="data">JsonObject to parse.</param>
        internal Reference(Statement statement, JsonObject data)
        {
            this.Statement = statement;
            this.FillData(data);
        }

        /// <summary>
        /// Parses the <paramref name="data"/> and adds the results to this instance.
        /// </summary>
        /// <param name="data"><see cref="JsonObject"/> to parse.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        protected void FillData(JsonObject data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (data.get("snaks") != null)
            {
                foreach (JsonObject.Member member in data.get("snaks").asObject())
                {
                    foreach (JsonValue value in member.value.asArray())
                    {
                        Snak snak = new Snak(value.asObject());
                        _snaks.Add(snak);
                    }
                }
            }

            var snaksOrderSection = data.get("snaks-order");
            if (snaksOrderSection != null && snaksOrderSection.isArray())
            {
                _snaksOrder.Clear();
                var snaksOrderArray = snaksOrderSection.asArray();

                foreach (var property in snaksOrderArray.getValues())
                {
                    _snaksOrder.Add(new EntityId(property.asString()));
                }
            }

            if (data.get("hash") != null)
            {
                this.Hash = data.get("hash").asString();
            }
            if (this.InternalId == null)
            {
                if (this.Hash != null)
                {
                    this.InternalId = this.Hash;
                }
                else
                {
                    this.InternalId = "" + Environment.TickCount + this.Statement.Id;
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
                throw new ArgumentNullException("snaks");
            if (statement == null)
                throw new ArgumentNullException("statement");
            if (!snaks.Any())
                throw new ArgumentException("snaks");

            this.Statement = statement;
            foreach (Snak snak in snaks)
            {
                AddSnak(snak);
            }
            this.InternalId = Environment.TickCount + this.Statement.Id;
        }

        /// <summary>
        /// Get the snaks for the given property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The snaks.</returns>
        public Snak[] GetSnaks(String property)
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
                throw new ArgumentNullException("snak");

            _snaks.Add(snak);

            if (!_snaksOrder.Contains(snak.PropertyId))
                _snaksOrder.Add(snak.PropertyId);
            Touch();
        }


        private void Touch()
        {
            this.Statement.Touch();
        }



        /// <summary>
        /// Remove the snak.
        /// </summary>
        /// <param name="snak">The snak.</param>
        /// <exception cref="ArgumentNullException"><paramref name="snak"/> is <c>null</c>.</exception>
        public void RemoveSnak(Snak snak)
        {
            if (snak == null)
                throw new ArgumentNullException("snak");

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
        protected void UpdateDataFromResult(JsonObject result)
        {
            if (result == null)
                throw new ArgumentNullException("result");

            if (result.get("reference") != null)
            {
                this.FillData(result.get("reference").asObject());
            }
            this.Statement.Entity.UpdateLastRevisionIdFromResult(result);
        }


        /// <summary>
        /// Encodes this referecne in a JsonObject
        /// </summary>
        /// <returns>a JsonObject with the reference encoded.</returns>
        internal virtual JsonObject Encode()
        {
            JsonObject encoded = new JsonObject();

            var snaksSection = new JsonObject();

            foreach (EntityId property in _snaksOrder)
            {
                var snaksForTheProperty = GetSnaks(property.PrefixedId);

                if (snaksForTheProperty.Any())
                {
                    var arrayOfSnaks = new JsonArray();

                    foreach (Snak s in snaksForTheProperty)
                    {
                        arrayOfSnaks.add(s.Encode());
                    }

                    snaksSection.add(property.PrefixedId.ToUpper(), arrayOfSnaks);
                }
            }

            JsonArray snaksOrderSection = new JsonArray();
            foreach (EntityId property in _snaksOrder)
            {
                snaksOrderSection.add(property.PrefixedId.ToUpper());
            }

            encoded.add("snaks", snaksSection);
            encoded.add("snaks-order", snaksOrderSection);

            if (Hash != null)
                encoded.add("hash", Hash);

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
