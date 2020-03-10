using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pype.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pype
{
    /// <summary>
    /// Generic API utility for integrating with third-parties
    /// </summary>
    public class Fitting<T>
    {
        private readonly ILogger<T> _logger;

        /// <summary>
        /// If no ContentType is provided this default value will be assumed
        /// </summary>
        private const string DefaultContentType = "application/json";

        public Fitting(ILogger<T> logger = null)
        {
            _logger = logger;
        }

        #region Properties
        /// <summary>
        /// API base path
        /// </summary>
        public string ApiBasePath { get; set; }

        /// <summary>
        /// Request suffix 
        /// </summary>
        public string RequestSuffix { get; set; }

        /// <summary>
        /// HTTP method (e.g.: GET, POST, PUT, PATCH, DELETE, etc.)
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Content-Type header (e.g.: application/json, text/plain, etc.)
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Dictionary of additional headers to add to the request
        /// </summary>
        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        /// <summary>
        /// Parameters to include with either PUT or POST requests
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }
        #endregion

        /// <summary>
        /// Prepares a new FittingResponse object and sets its RequestUtcDateTime to now
        /// </summary>
        private FittingResponse PrepareNewResponse() =>
          new FittingResponse
          {
              Status = new FittingResponseStatus
              {
                  RequestUtcDateTime = DateTime.UtcNow
              }
          };

        /// <summary>
        /// Prepares a new WebRequest object using the Fitting's properties
        /// </summary>
        private async Task<WebRequest> PrepareWebRequest()
        {
            string urlToRequestTo = ApiBasePath;

            // Append RequestSuffix to the base path URL if one was provided to this method
            if (!string.IsNullOrEmpty(RequestSuffix))
            {
                urlToRequestTo += RequestSuffix;
            }

            WebRequest webRequest = WebRequest.Create(urlToRequestTo);
            webRequest.Method = Method;
            webRequest.ContentType = string.IsNullOrEmpty(ContentType) ? DefaultContentType : ContentType;

            foreach (string key in Headers.Keys)
            {
                webRequest.Headers.Add(key, Headers[key]);
            }

            // If PUT or POST serialize the parameters and include them in the request payload
            switch (Method)
            {
                case Models.Method.Post:
                case Models.Method.Put:
                {
                    // Write JSON to the WebRequest
                    await using StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream());
                    string jsonData = JsonConvert.SerializeObject(Parameters, Formatting.Indented);

                    streamWriter.Write(jsonData);
                    streamWriter.Flush();
                    streamWriter.Close();
                    break;
                }
            }

            return webRequest;
        }

        /// <summary>
        /// Validates all required parameters are present
        /// </summary>
        private void ValidateParameters()
        {
            if (string.IsNullOrEmpty(ApiBasePath))
            {
                throw new ArgumentNullException(nameof(ApiBasePath));
            }
            if (string.IsNullOrEmpty(RequestSuffix))
            {
                throw new ArgumentNullException(nameof(RequestSuffix));
            }
            if (string.IsNullOrEmpty(Method))
            {
                throw new ArgumentNullException(nameof(Method));
            }
        }

        /// <summary>
        /// Returns the request stream - used for scenarios in which you don't want to hold the response in memory
        /// (e.g.: deserializing large amounts of JSON)
        /// </summary>
        public async Task<Stream> OpenFaucet()
        {
            ValidateParameters();

            FittingResponse fittingResponse = PrepareNewResponse();

            WebRequest webRequest = await PrepareWebRequest();

            return await webRequest.GetRequestStreamAsync();
        }

        /// <summary>
        /// Initiate the HTTP request asynchronously
        /// </summary>
        public async Task<FittingResponse> SendRequest()
        {
            ValidateParameters();

            FittingResponse fittingResponse = PrepareNewResponse();

            try
            {
                WebRequest webRequest = await PrepareWebRequest();

                WebResponse response = await webRequest.GetResponseAsync();
                Stream dataStream = response.GetResponseStream();

                if (dataStream != null)
                {
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();

                    JToken json = JToken.Parse(responseFromServer);

                    reader.Close();
                    dataStream.Close();
                    response.Close();

                    fittingResponse.Status.Health = FittingResponseStatusHealth.Good;
                    fittingResponse.Result = json;
                }
            }
            catch (WebException webException)
            {
                StringBuilder errorLogBuilder = new StringBuilder();

                switch (webException.Status)
                {
                    case WebExceptionStatus.Timeout:
                        errorLogBuilder.Append($"Fitting SendRequest threw a WebException due to a timeout");
                        break;
                    case WebExceptionStatus.SecureChannelFailure:
                        errorLogBuilder.Append($"Fitting SendRequest threw a WebException due to an SSL/TLS error");
                        break;
                    case WebExceptionStatus.ProtocolError:
                        int statusCode = (int)((HttpWebResponse)webException.Response).StatusCode;
                        errorLogBuilder.Append($"Fitting SendRequest threw a WebException with status code {statusCode}");
                        break;
                    default:
                        errorLogBuilder.Append($"Fitting SendRequest threw a WebException due to an unknown error");
                        break;
                }

                fittingResponse.Status.Health = FittingResponseStatusHealth.Bad;

                _logger?.LogError(
                  $"{errorLogBuilder}{Environment.NewLine}{webException.Message}{Environment.NewLine}{webException.StackTrace}");
            }
            catch (Exception exception)
            {
                fittingResponse.Status.Health = FittingResponseStatusHealth.Bad;

                _logger?.LogError(
                  $"Fitting SendRequest threw an Exception {Environment.NewLine}{exception.Message}{Environment.NewLine}{exception.StackTrace}");
            }

            fittingResponse.Status.ResponseUtcDateTime = DateTime.UtcNow;
            return fittingResponse;
        }
    }
}
