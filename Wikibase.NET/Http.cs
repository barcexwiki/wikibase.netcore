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
        public String UserAgent
        {
            get;
            set;
        }

        private CookieContainer cookies = new CookieContainer();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userAgent">The user agent</param>
        public Http(String userAgent)
        {
            this.UserAgent = userAgent;
        }

        /// <summary>
        /// Performs a http get request.
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns>The response</returns>
        public String get(String url)
        {
            return this.post(url, null);
        }

        /// <summary>
        /// Performs a http post request.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="postFields">The post fields.</param>
        /// <returns>The response.</returns>
        public String post(String url, Dictionary<String, String> postFields)
        {

            using (var _handler = new HttpClientHandler() { CookieContainer = cookies })
            using (HttpClient _client = new HttpClient(_handler))
            {
                _client.DefaultRequestHeaders.UserAgent.ParseAdd(this.UserAgent);

                HttpResponseMessage response;
                if (postFields != null)
                {
                    HttpContent _body = new StringContent(this.buildQuery(postFields));
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
        public String buildQuery(Dictionary<String, String> fields)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");

            String query = String.Empty;
            foreach (KeyValuePair<String, String> field in fields)
            {
                query += System.Uri.EscapeDataString(field.Key) + "=" + System.Uri.EscapeDataString(field.Value) + "&";
            }
            if (!String.IsNullOrEmpty(query))
            {
                query = query.Remove(query.Length - 1);
            }
            return query;
        }
    }
}