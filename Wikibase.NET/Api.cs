using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Wikibase
{
    /// <summary>
    /// Base api class
    /// </summary>
    public class Api
    {
        private const string ApiName = "Wikibase.NET";
        private const string ApiVersion = "0.1";

        private Http _http;
        private string _apiUrl;
        private string _editToken;

        /// <summary>
        /// Gets the sets the time stamp of the last API action.
        /// </summary>
        /// <value>The time stamp of the last API action.</value>
        protected int LastEditTimestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the sets the time lap between two consecutive API actions in milliseconds.
        /// </summary>
        /// <value>The time lap in milliseconds.</value>
        public int EditLaps
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets if bot edits should be used.
        /// </summary>
        /// <value><c>true</c> if bot edits are used, <c>false</c> otherwise.</value>
        public bool BotEdits
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets if the edits should be limited.
        /// </summary>
        /// <value><c>true</c> if the edits are limited, <c>false</c> otherwise.</value>
        public bool EditLimit
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="apiUrl">The URL of the wiki API like "http://www.wikidata.org/w/api.php".</param>
        public Api(string apiUrl)
            : this(apiUrl, ApiName)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="apiUrl">The URL of the API access point "https://www.wikidata.org/w/api.php".</param>
        /// <param name="userAgent">The user agent.</param>
        /// <exception cref="ArgumentNullException"><paramref name="userAgent"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="apiUrl"/> is empty or <c>null</c>.</exception>
        public Api(string apiUrl, string userAgent)
        {
            if (string.IsNullOrWhiteSpace(apiUrl))
                throw new ArgumentException("Invalid API URL", nameof(apiUrl));
            if (userAgent == null)
                throw new ArgumentNullException(nameof(userAgent));

            _apiUrl = apiUrl;
            _http = new Http(string.Format(CultureInfo.InvariantCulture, "{0} {1}/{2}", userAgent.Trim(), ApiName, ApiVersion));
        }

        /// <summary>
        /// Perform a http get request to the api.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The result.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="parameters"/> is <c>null</c>.</exception>
        public JToken Get(Dictionary<string, string> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            parameters["format"] = "json";
            string url = _apiUrl + "?" + _http.BuildQuery(parameters);
            string response = _http.Get(url);
            JToken result = JToken.Parse(response);
            if (result.Type != JTokenType.Object)
            {
                return null;
            }
            JToken obj = result;
            if (obj["error"] != null)
            {
                throw new ApiException((string)obj["error"]["info"]);
            }
            return obj;
        }

        /// <summary>
        /// Perform a http post request to the api.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="postFields">The post fields.</param>
        /// <returns>The result.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="parameters"/> or <paramref name="postFields"/> is <c>null</c>.</exception>
        public JToken Post(Dictionary<string, string> parameters, Dictionary<string, string> postFields)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            if (postFields == null)
                throw new ArgumentNullException(nameof(postFields));

            parameters["format"] = "json";
            string url = _apiUrl + "?" + _http.BuildQuery(parameters);
            string response = _http.Post(url, postFields);
            JToken result = JToken.Parse(response);
            if (result["error"] != null)
            {
                throw new ApiException((string)result["error"]["info"]);
            }
            return result;
        }

        /// <summary>
        /// Get the continuation parameter of a query.
        /// </summary>
        /// <param name="result">The result of the query.</param>
        /// <returns>An array containing the continuation parameter key at 0 and the continuation parameter value at 1.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <c>null</c>.</exception>
        //public string[] GetContinueParam(JsonObject result)
        //{
        //    if (result == null)
        //        throw new ArgumentNullException(nameof(result));

        //    if (result.get("query-continue") != null)
        //    {
        //        List<string> keys = (List<string>)result.get("query-continue").asObject().names();
        //        List<string> keys2 = (List<string>)result.get("query-continue").asObject().get(keys[0]).asObject().names();
        //        return new string[] { keys2[0], result.get("query-continue").asObject().get(keys[0]).asObject().get(keys2[0]).asString() };
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        /// <summary>
        /// Do login.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns><c>true</c> if the user is logged in successfully, <c>false</c> otherwise.</returns>
        public bool Login(string userName, string password)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "action", "login" }
            };
            Dictionary<string, string> postFields = new Dictionary<string, string>()
            {
                { "lgname", userName },
                { "lgpassword", password }
            };
            JToken login = Post(parameters, postFields)["login"];
            if ((string)login["result"] == "NeedToken")
            {
                postFields["lgtoken"] = (string)login["token"];
                login = Post(parameters, postFields)["login"];
            }
            if ((string)login["result"] == "Success")
            {
                _editToken = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Do logout.
        /// </summary>
        public void Logout()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "action", "logout" }
            };
            Get(parameters);
            _editToken = null;
        }

        /// <summary>
        /// Return the edit token for the current user.
        /// </summary>
        /// <returns>The edit token.</returns>
        public string GetEditToken()
        {
            if (_editToken == null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>()
                {
                    { "action", "query" },
                    { "prop", "info" },
                    { "intoken", "edit" },
                    { "titles", "Main Page" }
                };
                JToken query = Get(parameters)["query"];
                foreach (JProperty member in query["pages"])
                {
                    return (string)member.Value["edittoken"];
                }
            }
            return _editToken;
        }
    }
}