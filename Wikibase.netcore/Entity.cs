using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Wikibase
{
    /// <summary>
    /// An entity
    /// </summary>
    public abstract class Entity
    {
        private enum AliasStatus
        {
            Existing,
            New,
            Removed
        }
        public enum EntityStatus
        {
            New,
            Loaded,
            Changed,
            ToBeDeleted,
            Deleted
        }

        private class EntityAlias
        {
            public string Language { get; set; }
            public string Label { get; set; }
            public AliasStatus Status { get; set; }

            public EntityAlias(string language, string label, AliasStatus status)
            {
                Language = language;
                Label = label;
                Status = status;
            }
        }


        /// <summary>
        /// The entity object Status (New, Loaded, Changed, Deleted)
        /// </summary>
        public EntityStatus Status
        {
            get;
            protected set;
        }

        /// <summary>
        /// The entity id
        /// </summary>
        public EntityId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// The last revision id
        /// </summary>
        public int LastRevisionId
        {
            get;
            set;
        }

        /// <summary>
        /// The api
        /// </summary>
        public WikibaseApi Api
        {
            get;
            private set;
        }

        /// <summary>
        /// Labels, the actual name. Key is the language editifier, value the label.
        /// </summary>
        private Dictionary<string, string> _labels = new Dictionary<string, string>();

        /// <summary>
        /// Descriptions, to explain the item. Key is the language editifier, value the description.
        /// </summary>
        private Dictionary<string, string> _descriptions = new Dictionary<string, string>();

        /// <summary>
        /// Aliases.
        /// </summary>
        private List<EntityAlias> _aliases = new List<EntityAlias>();

        /// <summary>
        /// Claims. 
        /// </summary>
        private List<Claim> _claims = new List<Claim>();

        /// <summary>
        /// Changes cache.
        /// </summary>
        protected JObject changes = new JObject();

        /// <summary>
        /// List of languages codes whose labels have changed
        /// </summary>
        protected HashSet<string> dirtyLabels = new HashSet<string>();

        /// <summary>
        /// List of languages codes whose descriptions have changed
        /// </summary>
        protected HashSet<string> dirtyDescriptions = new HashSet<string>();


        /// <summary>
        /// Constructor creating a blank entity instance.
        /// </summary>
        /// <param name="api">The api.</param>
        public Entity(WikibaseApi api)
        {
            Api = api;
            FillData(new JObject());
            Status = EntityStatus.New;
        }

        /// <summary>
        /// Constructor creating an entitiy from a Json object.
        /// </summary>
        /// <param name="api">The api</param>
        /// <param name="data">The json object to be parsed.</param>
        internal Entity(WikibaseApi api, JToken data)
        {
            Api = api;
            FillData(data);
            Status = EntityStatus.Loaded;
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

            // Clears the dirty sets
            dirtyLabels.Clear();
            dirtyDescriptions.Clear();

            if (data["id"] != null)
            {
                Id = new EntityId((string)data["id"]);
            }
            if (data["lastrevid"] != null)
            {
                LastRevisionId = (int)data["lastrevid"];
            }
            JToken returnedLabels = data["labels"];
            if ((returnedLabels != null) && (returnedLabels.Type == JTokenType.Object))
            {
                _labels.Clear();
                foreach (JProperty member in returnedLabels)
                {
                    _labels.Add((string)member.Value["language"], (string)member.Value["value"]);
                }
            }
            JToken returnedDescriptions = data["descriptions"];
            if ((returnedDescriptions != null) && (returnedDescriptions.Type == JTokenType.Object))
            {
                _descriptions.Clear();
                foreach (JProperty member in returnedDescriptions)
                {
                    _descriptions.Add((string)member.Value["language"], (string)member.Value["value"]);
                }
            }
            JToken returnedAliases = data["aliases"];
            if ((returnedAliases != null) && (returnedAliases.Type == JTokenType.Object))
            {
                // strange - after save an empty array is returned, whereas by a normal get the fully alias list is returned
                _aliases.Clear();
                foreach (JProperty member in returnedAliases)
                {
                    List<string> list = new List<string>();
                    foreach (JToken alias in member.Value)
                    {
                        _aliases.Add(new EntityAlias((string)alias["language"], (string)alias["value"], AliasStatus.Existing));
                    }
                }
            }
            JToken returnedClaims = data["claims"];
            if ((returnedClaims != null) && (returnedClaims.Type == JTokenType.Object)) 
            {
                _claims.Clear();
                foreach (JProperty member in returnedClaims)
                {
                    foreach (JToken value in member.Value)
                    {
                        Claim claim = Claim.NewFromArray(this, value);
                        _claims.Add(claim);
                    }
                }
            }
        }

        internal static Entity NewFromArray(WikibaseApi api, JToken data)
        {
            if (data["type"] != null)
            {
                switch ((string)data["type"])
                {
                    case "item":
                        return new Item(api, data);
                    case "property":
                        return new Property(api, data);
                }
            }
            throw new Exception("Unknown type");
        }

        /// <summary>
        /// Get all labels.
        /// </summary>
        /// <returns>The labels</returns>
        /// <remarks>Key is the language, value the label.</remarks>
        public Dictionary<string, string> GetLabels()
        {
            return new Dictionary<string, string>(_labels);
        }

        /// <summary>
        /// Get the label for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <returns>The label.</returns>
        /// <exception cref="ArgumentException"><paramref name="lang"/> is empty string or <c>null</c>.</exception>
        public string GetLabel(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language",nameof(lang));
            return _labels.ContainsKey(lang) ? _labels[lang] : null;
        }


        /// <summary>
        /// Set the label for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <param name="value">The label.</param>
        /// <exception cref="ArgumentException"><paramref name="lang"/> or <paramref name="value"/> is empty string or <c>null</c>.</exception>
        public void SetLabel(string lang, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("empty description",nameof(value));
            if (string.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language", nameof(lang));

            if (!IsTouchable())
            {
                throw new InvalidOperationException($"Cannot set a label for an entity with status {Status}");
            }

            if (GetLabel(lang) != value)
            {
                _labels[lang] = value;
                dirtyLabels.Add(lang);
                Touch();
            }
        }

        /// <summary>
        /// Remove the label for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <returns><c>true</c> if the label was removed successfully, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="lang"/> is empty string or <c>null</c>.</exception>
        public bool RemoveLabel(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language",nameof(lang));

            if (!IsTouchable())
            {
                throw new InvalidOperationException($"Cannot remove a label from an entity with status {Status}");
            }

            if (_labels.Remove(lang))
            {
                dirtyLabels.Add(lang);
                Touch();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get all descriptions.
        /// </summary>
        /// <returns>The descriptions.</returns>
        /// <remarks>Keys is the language, value the description.</remarks>
        public Dictionary<string, string> GetDescriptions()
        {
            return new Dictionary<string, string>(_descriptions);
        }

        /// <summary>
        /// Get the description for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <returns>The description.</returns>
        /// <exception cref="ArgumentException"><paramref name="lang"/> is empty string or <c>null</c>.</exception>
        public string GetDescription(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language",nameof(lang));
            return _descriptions.ContainsKey(lang) ? _descriptions[lang] : null;
        }

        /// <summary>
        /// Set the description for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <param name="value">The description.</param>
        /// <exception cref="ArgumentException"><paramref name="lang"/> or <paramref name="value"/> is empty string or <c>null</c>.</exception>
        public void SetDescription(string lang, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("empty description",nameof(value));
            if (string.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language",nameof(lang));

            if (!IsTouchable())
            {
                throw new InvalidOperationException($"Cannot set a description for an entity with status {Status}");
            }

            if (GetDescription(lang) != value)
            {
                _descriptions[lang] = value;
                dirtyDescriptions.Add(lang);
                Touch();
            }
        }

        /// <summary>
        /// Remove the description for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <returns><c>true</c> if the description was removed successfully, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="lang"/> is empty string or <c>null</c>.</exception>
        public bool RemoveDescription(string lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language",nameof(lang));

            if (!IsTouchable())
            {
                throw new InvalidOperationException($"Cannot remove a description from an entity with status {Status}");
            }

            if (_descriptions.Remove(lang))
            {
                dirtyDescriptions.Add(lang);
                Touch();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get all aliases.
        /// </summary>
        /// <returns>The aliases</returns>
        /// <value>Key is the language, value a list of aliases.</value>
        public Dictionary<string, List<string>> GetAliases()
        {
            IEnumerable<EntityAlias> filtered = from a in _aliases
                                                where a.Status == AliasStatus.Existing || a.Status == AliasStatus.New
                                                select a;

            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

            foreach (EntityAlias a in filtered)
            {
                if (!result.ContainsKey(a.Language))
                    result[a.Language] = new List<string>();

                result[a.Language].Add(a.Label);
            }

            return result;
        }


        /// <summary>
        /// Get the aliases for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <returns>The aliases, or <c>null</c> if no aliases are defined for the language.</returns>
        public string[] GetAliases(string lang)
        {
            IEnumerable<string> filtered = from a in _aliases
                                           where a.Language == lang && (a.Status == AliasStatus.Existing || a.Status == AliasStatus.New)
                                           select a.Label;

            //return filtered.Any() ? filtered.ToArray() : null;
            return filtered.ToArray();
        }


        /// <summary>
        /// Add an alias for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <param name="value">The alias.</param>
        public void AddAlias(string lang, string value)
        {
            IEnumerable<EntityAlias> filtered = from a in _aliases
                                                where a.Language == lang && a.Label == value
                                                select a;

            EntityAlias alias = filtered.FirstOrDefault();

            if (alias != null)
            {
                if (alias.Status == AliasStatus.Removed)
                    alias.Status = AliasStatus.Existing;
            }
            else
            {
                _aliases.Add(new EntityAlias(lang, value, AliasStatus.New));
                Touch();
            }
        }


        /// <summary>
        /// Remove the alias for the given language.
        /// </summary>
        /// <param name="lang">The language.</param>
        /// <param name="value">The alias.</param>
        /// <returns><c>true</c> if the alias was removed successfully, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException"><paramref name="lang"/> or <paramref name="value"/> is empty string or <c>null</c>.</exception>
        public void RemoveAlias(string lang, string value)
        {
            if (string.IsNullOrWhiteSpace(lang))
                throw new ArgumentException("empty language", nameof(lang));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("empty value", nameof(value));

            if (!IsTouchable())
            {
                throw new InvalidOperationException($"Cannot remove an alias from an entity with status {Status}");
            }

            IEnumerable<EntityAlias> filtered = from a in _aliases
                                                where a.Language == lang && a.Label == value
                                                select a;

            EntityAlias alias = filtered.FirstOrDefault();

            if (alias != null)
            {
                switch (alias.Status)
                {
                    case AliasStatus.Existing:
                        alias.Status = AliasStatus.Removed;
                        break;
                    case AliasStatus.New:
                        _aliases.Remove(alias);
                        break;
                }
                Touch();
            }
        }


        /// <summary>
        /// Gets all claims.
        /// </summary>
        /// <value>All claims.</value>
        public IEnumerable<Claim> Claims
        {
            get
            {
                return _claims.Where(c => c.status == Claim.ClaimStatus.Existing || c.status == Claim.ClaimStatus.New);
            }
        }

        /// <summary>
        /// Get the claims for the given property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The claims.</returns>
        public Claim[] GetClaims(string property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var claimList = from c in Claims
                            where c.MainSnak.PropertyId.PrefixedId.ToUpper() == property.ToUpper()
                            select c;

            return claimList.ToArray();
        }

        /// <summary>
        /// Remove the claim.
        /// </summary>
        /// <param name="claim">The claim.</param>
        /// <returns><c>true</c> if the claim was removed successfully, <c>false</c> otherwise.</returns>
        public bool RemoveClaim(Claim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            if (!IsTouchable())
            {
                throw new InvalidOperationException($"Cannot remove a claim from an entity with status {Status}");
            }

            if (_claims.Contains(claim))
            {
                if (claim.status == Claim.ClaimStatus.New)
                {
                    _claims.Remove(claim);
                }
                claim.Delete();
                Touch();
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void Delete()
        {            
            if (!IsTouchable())
            {
                throw new InvalidOperationException($"Cannot delete an entity with status {Status}");
            }

            if (Status == EntityStatus.New)
            {
                Status = EntityStatus.Deleted;
            } else
            {
                Status = EntityStatus.ToBeDeleted;
            }

            Clear();

        }

        private bool SaveClaims()
        {
            Claim[] claimsToProcess = _claims.ToArray();

            bool touched = false;

            foreach (Claim c in claimsToProcess)
            {
                switch (c.status)
                {
                    case Claim.ClaimStatus.Deleted:
                        c.Save("");
                        _claims.Remove(c);
                        touched = true;
                        break;
                    case Claim.ClaimStatus.Modified:
                    case Claim.ClaimStatus.New:
                        c.RefreshId();                     
                        c.Save("");
                        touched = true;
                        break;
                }
            }

            return touched;
        }

        /// <summary>
        /// Save all changes.
        /// </summary>
        /// <param name="summary">The edit summary.</param>
        public virtual void Save(string summary)
        {
            if (summary == null)
            {
                summary = "";
            }

            EntityStatus currentStatus = Status;
            switch (currentStatus)
            {
                case EntityStatus.ToBeDeleted:
                    Api.DeleteEntity(GetEntityType() + ":" + Id.PrefixedId, LastRevisionId, summary);
                    Status = EntityStatus.Deleted;
                    break;
                case EntityStatus.Changed:
                case EntityStatus.New:

                    if (dirtyLabels.Count > 0)
                    {
                        if (changes["labels"] == null)
                        {
                            changes["labels"] = new JObject();
                        }

                        foreach (string lang in dirtyLabels)
                        {
                            string labelValue = "";

                            // If there is label the text is the label itself, if not is a removed label (i.e. empty)
                            if (_labels.ContainsKey(lang))
                            {
                                labelValue = _labels[lang];
                            }

                            changes["labels"][lang] = new JObject
                            {
                                { "language", lang},
                                { "value", labelValue }
                            };

                        }
                    }

                    if (dirtyDescriptions.Count > 0)
                    {
                        if (changes["descriptions"] == null)
                        {
                            changes["descriptions"] = new JObject();
                        }

                        foreach (string lang in dirtyDescriptions)
                        {
                            string descriptionValue = "";

                            // If there is description the text is the label itself, if not is a removed label (i.e. empty)
                            if (_descriptions.ContainsKey(lang))
                            {
                                descriptionValue = _descriptions[lang];
                            }

                            changes["descriptions"][lang] = new JObject
                            {
                                { "language", lang},
                                { "value", descriptionValue }
                            };                              
                        }
                    }


                    // Process aliases changes
                    IEnumerable<EntityAlias> aliasesToSave = from a in _aliases
                                                                where a.Status == AliasStatus.New || a.Status == AliasStatus.Removed
                                                                select a;

                    if (changes["aliases"] == null && aliasesToSave.Any())
                    {
                        changes["aliases"] = new JArray();
                    }
                    foreach (EntityAlias a in aliasesToSave)
                    {
                        JObject jsonAlias = new JObject
                        {
                            { "language", a.Language },
                            { "value", a.Label},
                            { a.Status == AliasStatus.New ? "add" : "remove", true}
                        };                           

                        ((JArray)(changes["aliases"])).Add(jsonAlias);
                    }

                    JToken result;

                    if (Id == null)
                    {
                        result = Api.CreateEntity(GetEntityType(), changes, LastRevisionId, summary);

                        if (result["entity"] != null)
                        {
                            JObject data = (JObject)result["entity"];
                            if (data["id"] != null)
                            {
                                Id = new EntityId((string)data["id"]);
                            }
                        }
                    }
                    else
                    {
                        result = Api.EditEntity(Id.PrefixedId, changes, LastRevisionId, summary);
                    }


                    if (SaveClaims())
                    {
                        JToken entity = Api.GetEntityJsonFromId(Id);
                        FillData(entity);
                        Status = EntityStatus.Loaded;
                    }
                    else
                    {
                        if (result["entity"] != null)
                        {
                            FillData(result["entity"]);
                            Status = EntityStatus.Loaded;
                        }
                    }


                    UpdateLastRevisionIdFromResult(result);
                    changes = new JObject();

                    break;
            }
        }

        internal void UpdateLastRevisionIdFromResult(JToken result)
        {
            if (result["pageinfo"] != null && result["pageinfo"]["lastrevid"] != null)
            {
                LastRevisionId = (int)(result["pageinfo"]["lastrevid"]);
            }
        }

        public Statement AddStatement(Snak snak, Rank rank)
        {

            if (!IsTouchable())
            {
                throw new InvalidOperationException($"Cannot add a statement to an entity with status {Status}");
            }

            Statement s = new Statement(this, snak, rank);
            _claims.Add(s);
            Touch();
            return s;
        }

        /// <summary>
        /// Gets the type identifier of the type at server side.
        /// </summary>
        /// <returns>The type identifier.</returns>
        protected abstract string GetEntityType();


        protected bool IsTouchable()
        {
            return (Status != EntityStatus.Deleted && Status != EntityStatus.ToBeDeleted);
        }


        internal void Touch()
        {
            EntityStatus currentStatus = Status;
            switch (currentStatus)
            {
                case EntityStatus.Changed:
                    break;
                case EntityStatus.New:
                    break;
                case EntityStatus.Loaded:
                    Status = EntityStatus.Changed;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        protected virtual void Clear()
        {
            _aliases.Clear();
            _claims.Clear();
            _descriptions.Clear();
            _labels.Clear();
            _aliases.Clear();
            dirtyDescriptions.Clear();
            dirtyLabels.Clear();
        }

    }
}