using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;

namespace Wikibase
{
    /// <summary>
    /// Http related code.
    /// </summary>
    internal class Http
    {
        /// <summary>
        /// Gets or sets the user agent.
        /// </summary>
        /// <value>The user agent.</value>
        public string UserAgent
        {
            get;
            set;
        }

        private CookieContainer _cookies = new CookieContainer();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userAgent">The user agent</param>
        public Http(string userAgent)
        {
            this.UserAgent = userAgent;
        }

        /// <summary>
        /// Performs a http get request.
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns>The response</returns>
        public string Get(string url)
        {
            return this.Post(url, null);
        }

        /// <summary>
        /// Performs a http post request.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="postFields">The post fields.</param>
        /// <returns>The response.</returns>
        public string Post(string url, Dictionary<string, string> postFields)
        {
            using (var _handler = new HttpClientHandler() { CookieContainer = _cookies })
            using (HttpClient _client = new HttpClient(_handler))
            {
                _client.DefaultRequestHeaders.UserAgent.ParseAdd(this.UserAgent);

                HttpResponseMessage response;
                if (postFields != null)
                {
                    HttpContent _body = new StringContent(this.BuildQuery(postFields));
                    _body.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    response = _client.PostAsync(url, _body).Result;
                }
                else
                {
                    response = _client.GetAsync(url).Result;
                }

                return response.Content.ReadAsStringAsync().Result;
            }
        }

        /// <summary>
        /// Builds a http query string.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns>The query string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fields"/> is <c>null</c>.</exception>
        public string BuildQuery(Dictionary<string, string> fields)
        {
            if (fields == null)
                throw new ArgumentNullException(nameof(fields));

            string query = string.Empty;
            foreach (KeyValuePair<string, string> field in fields)
            {
                query += System.Uri.EscapeDataString(field.Key) + "=" + System.Uri.EscapeDataString(field.Value) + "&";
            }
            if (!string.IsNullOrEmpty(query))
            {
                query = query.Remove(query.Length - 1);
            }
            return query;
        }
    }
}