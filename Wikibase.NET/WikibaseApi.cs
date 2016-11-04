using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using MinimalJson;
using Wikibase.DataValues;
using System.Threading.Tasks;

namespace Wikibase
{
    /// <summary>
    /// Base class for the Wikibase API.
    /// </summary>
    public class WikibaseApi : Api
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiUrl">The base url of the wiki like "https://www.wikidata.org"</param>
        public WikibaseApi(String apiUrl)
            : base(apiUrl)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiUrl">The API URL of the wiki like "https://www.wikidata.org/w/api.php"</param>
        /// <param name="userAgent">The user agent</param>
        public WikibaseApi(String apiUrl, String userAgent)
            : base(apiUrl, userAgent)
        {
        }

        /// <summary>
        /// Get the data for the entities in the given languages from the provided ids.
        /// </summary>
        /// <param name="ids">The ids</param>
        /// <param name="languages">The languages</param>
        /// <returns>The list of entities</returns>
        internal Entity[] GetEntitiesFromIds(String[] ids, String[] languages)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>()
            {
                { "action", "wbgetentities" },
                { "ids", String.Join("|", ids ) }
            };
            if (languages != null)
            {
                parameters["languages"] = String.Join("|", languages);
            }
            JsonObject result = this.Get(parameters);
            return ParseGetEntitiesApiResponse(result);
        }

        internal JsonObject GetEntityJsonFromId(EntityId id)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>()
            {
                { "action", "wbgetentities" },
                { "ids", id.PrefixedId }
            };
            JsonObject result = this.Get(parameters);
            return result.get("entities").asObject();
        }

        /// <summary>
        /// Get the data for the entities in the given languages from the provided sites and titles.
        /// </summary>
        /// <param name="sites">The sites</param>
        /// <param name="titles">The titles</param>
        /// <param name="languages">The languages</param>
        /// <returns>The list of entities</returns>
        internal Entity[] GetEntitesFromSitelinks(String[] sites, String[] titles, String[] languages)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>()
            {
                { "action", "wbgetentities" },
                { "sites", String.Join("|", sites ) },
                { "titles", String.Join("|", titles ) }
            };
            if (languages != null)
            {
                parameters["languages"] = String.Join("|", languages);
            }
            JsonObject result = this.Get(parameters);
            return ParseGetEntitiesApiResponse(result);
        }

        /// <summary>
        /// Create a list of entities from an api response.
        /// </summary>
        /// <param name="result">The result of the api request</param>
        /// <returns>The list of entities</returns>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <c>null</c>.</exception>
        protected Entity[] ParseGetEntitiesApiResponse(JsonObject result)
        {
            if (result == null)
                throw new ArgumentNullException("result");

            List<Entity> entities = new List<Entity>();
            if (result.get("entities") != null)
            {
                foreach (JsonObject.Member member in result.get("entities").asObject())
                {
                    if (member.value.asObject().get("missing") == null)
                    {
                        entities.Add(Entity.NewFromArray(this, member.value.asObject()));
                    }
                }
            }
            return entities.ToArray();
        }

        /// <summary>
        /// Edit an entity.
        /// </summary>
        /// <param name="id">The id of the entity</param>
        /// <param name="data">The serialized data of the entity</param>
        /// <param name="baseRevisionId">The numeric identifier for the revision to base the modification on</param>
        /// <param name="summary">The summary for the change</param>
        /// <returns>The returned json object from the server.</returns>
        internal JsonObject EditEntity(String id, JsonObject data, Int32 baseRevisionId, String summary)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>()
            {
                { "action", "wbeditentity" }
            };
            Dictionary<String, String> postFields = new Dictionary<String, String>()
            {
                { "data", data.ToString() },
                { "id", id }
            };
            return this.EditAction(parameters, postFields, baseRevisionId, summary);
        }

        /// <summary>
        /// Delete an entity.
        /// </summary>
        /// <param name="title">The id of the entity</param>
        /// <param name="summary">The summary for the change</param>
        /// <param name="baseRevisionId">The numeric identifier for the revision to base the modification on</param>
        /// <returns>The returned json object from the server.</returns>
        internal void DeleteEntity(String title, Int32 baseRevisionId, String summary)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>()
            {
                { "action", "delete" }
            };
            Dictionary<String, String> postFields = new Dictionary<String, String>()
            {
                { "title", title }
            };
            this.EditAction(parameters, postFields, baseRevisionId, summary);
        }

        /// <summary>
        /// Create an entity.
        /// </summary>
        /// <param name="type">The type of the entity</param>
        /// <param name="data">The serialized data of the entity</param>
        /// <param name="baseRevisionId">The numeric identifier for the revision to base the modification on</param>
        /// <param name="summary">The summary for the change</param>
        /// <returns>The returned json object from the server.</returns>
        internal JsonObject CreateEntity(String type, JsonObject data, Int32 baseRevisionId, String summary)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>()
            {
                { "action", "wbeditentity" }
            };
            Dictionary<String, String> postFields = new Dictionary<String, String>()
            {
                { "data", data.ToString() },
                { "new", type }
            };
            return this.EditAction(parameters, postFields, baseRevisionId, summary);
        }

        /// <summary>
        /// Create a claim.
        /// </summary>
        /// <param name="entity">The id of the entity you are adding the claim to.</param>
        /// <param name="snakType">The type of the snak.</param>
        /// <param name="property">The id of the snak property.</param>
        /// <param name="value">The value of the snak when creating a claim with a snak that has a value.</param>
        /// <param name="baseRevisionId">The numeric identifier for the revision to base the modification on.</param>
        /// <param name="summary">The summary for the change.</param>
        /// <returns>The returned json object from the server.</returns>
        internal JsonObject CreateClaim(String entity, String snakType, String property, DataValue value, Int32 baseRevisionId, String summary)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>()
            {
                { "action", "wbcreateclaim" },
                { "entity", entity },
                { "snaktype", snakType },
                { "property", property }
            };
            if (value != null)
            {
                parameters["value"] = value.Encode().ToString();
            }
            return this.EditAction(parameters, new Dictionary<String, String>(), baseRevisionId, summary);
        }

        /// <summary>
        /// Create a claim.
        /// </summary>
        /// <param name="claim">Statement or claim serialization</param>
        /// <param name="baseRevisionId">The numeric identifier for the revision to base the modification on.</param>
        /// <param name="summary">The summary for the change.</param>
        /// <returns>The returned json object from the server.</returns>
        internal JsonObject SetClaim(String claim, Int32 baseRevisionId, String summary)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>()
            {
                { "action", "wbsetclaim" }
            };
            Dictionary<String, String> postFields = new Dictionary<String, String>()
            {
                { "claim", claim }
            };
            return this.EditAction(parameters, postFields, baseRevisionId, summary);
        }


        /// <summary>
        /// Set a claim value.
        /// </summary>
        /// <param name="claim">GUID identifying the claim</param>
        /// <param name="snakType">The type of the snak</param>
        /// <param name="value">The value of the snak when creating a claim with a snak that has a value.</param>
        /// <param name="baseRevisionId">The numeric identifier for the revision to base the modification on.</param>
        /// <param name="summary">The summary for the change.</param>
        /// <returns>The returned json object from the server.</returns>
        internal JsonObject SetClaimValue(String claim, String snakType, DataValue value, Int32 baseRevisionId, String summary)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>()
            {
                { "action", "wbsetclaimvalue" },
                { "claim", claim },
                { "snaktype", snakType }
            };
            if (value != null)
            {
                parameters["value"] = value.Encode().ToString();
            }
            return this.EditAction(parameters, new Dictionary<String, String>(), baseRevisionId, summary);
        }

        /// <summary>
        /// Remove the claims.
        /// </summary>
        /// <param name="claims">The claims</param>
        /// <param name="baseRevisionId">The numeric identifier for the revision to base the modification on</param>
        /// <param name="summary">The summary for the change</param>
        /// <returns>The returned json object from the server.</returns>
        internal JsonObject RemoveClaims(String[] claims, Int32 baseRevisionId, String summary)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>()
            {
                { "action", "wbremoveclaims" },
                { "claim", string.Join("|", claims) }
            };
            return this.EditAction(parameters, new Dictionary<String, String>(), baseRevisionId, summary);
        }

        /// <summary>
        /// Set a reference.
        /// </summary>
        /// <param name="statement">GUID identifying the statement</param>
        /// <param name="snaks">The snaks to set the reference to. Array with property ids pointing to arrays containing the snaks for that property</param>
        /// <param name="reference">A hash of the reference that should be updated. When not provided, a new reference is created</param>
        /// <param name="baseRevisionId">The numeric identifier for the revision to base the modification on</param>
        /// <param name="summary">The summary for the change</param>
        /// <returns>The returned json object from the server.</returns>
        internal JsonObject SetReference(String statement, JsonObject snaks, String reference, Int32 baseRevisionId, String summary)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>()
            {
                { "action", "wbsetreference" },
                { "statement", statement },
                { "snaks", snaks.ToString() }
            };
            if (reference != null)
            {
                parameters["reference"] = reference;
            }
            return this.EditAction(parameters, new Dictionary<String, String>(), baseRevisionId, summary);
        }

        /// <summary>
        /// Remove the references.
        /// </summary>
        /// <param name="statement">GUID identifying the statement</param>
        /// <param name="references">The hashes of the references that should be removed</param>
        /// <param name="baseRevisionId">The numeric identifier for the revision to base the modification on</param>
        /// <param name="summary">The summary for the change</param>
        /// <returns>The returned json object from the server.</returns>
        internal JsonObject RemoveReferences(String statement, String[] references, Int32 baseRevisionId, String summary)
        {
            Dictionary<string, string> parameters = new Dictionary<String, String>()
            {
                { "action", "wbremovereferences" },
                { "statement", statement },
                { "references", string.Join("|", references) }
            };
            return this.EditAction(parameters, new Dictionary<String, String>(), baseRevisionId, summary);
        }

        /// <summary>
        /// Set a qualifier.
        /// </summary>
        /// <param name="statement">GUID identifying the statement</param>
        /// <param name="snakType">The type of the snak.</param>
        /// <param name="property">The id of the snak property.</param>
        /// <param name="value">The value of the snak when creating a claim with a snak that has a value.</param>
        /// <param name="baseRevisionId">The numeric identifier for the revision to base the modification on.</param>
        /// <param name="summary">The summary for the change.</param>
        /// <returns>The returned json object from the server.</returns>
        internal JsonObject SetQualifier(String statement, String snakType, String property, DataValue value, Int32 baseRevisionId, String summary)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>()
            {
                { "action", "wbsetqualifier" },
                { "claim", statement },
                { "snaktype", snakType },
                { "property", property }
            };
            if (value != null)
            {
                parameters["value"] = value.Encode().ToString();
            }

            return this.EditAction(parameters, new Dictionary<String, String>(), baseRevisionId, summary);
        }

        /// <summary>
        /// Remove the qualifiers.
        /// </summary>
        /// <param name="statement">GUID identifying the statement.</param>
        /// <param name="qualifiers">The hashes of the qualifiers that should be removed.</param>
        /// <param name="baseRevisionId">The numeric identifier for the revision to base the modification on.</param>
        /// <param name="summary">The summary for the change.</param>
        /// <returns>The returned json object from the server.</returns>
        internal JsonObject RemoveQualifier(String statement, String[] qualifiers, Int32 baseRevisionId, String summary)
        {
            Dictionary<string, string> parameters = new Dictionary<String, String>()
            {
                { "action", "wbremovequalifiers" },
                { "statement", statement },
                { "qualifiers", string.Join("|", qualifiers) }
            };
            return this.EditAction(parameters, new Dictionary<String, String>(), baseRevisionId, summary);
        }

        /// <summary>
        /// Perform an edit action.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="postFields">The post fields.</param>
        /// <param name="baseRevisionId">The numeric identifier for the revision to base the modification on.</param>
        /// <param name="summary">The summary for the change.</param>
        /// <returns>The returned json object from the server.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="parameters"/> or <paramref name="postFields"/> is <c>null</c>.</exception>
        private JsonObject EditAction(Dictionary<String, String> parameters, Dictionary<String, String> postFields, Int32 baseRevisionId, String summary)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (postFields == null)
                throw new ArgumentNullException("postFields");

            postFields["token"] = this.GetEditToken();
            if (baseRevisionId != 0)
            {
                parameters["baserevid"] = baseRevisionId.ToString(CultureInfo.InvariantCulture);
            }
            if (summary != null)
            {
                parameters["summary"] = summary;
            }
            if (this.BotEdits)
            {
                parameters["bot"] = true.ToString();
            }
            // limit number of edits
            Int32 time = Environment.TickCount;
            if (this.LastEditTimestamp > 0 && (time - this.LastEditTimestamp) < this.EditLaps)
            {
                Int32 wait = this.LastEditTimestamp + this.EditLaps - time;
                Console.WriteLine("Wait for {0} seconds...", wait / 1000);
                Task.Delay(wait).Wait();
            }
            this.LastEditTimestamp = Environment.TickCount;
            return this.Post(parameters, postFields);
        }
    }
}