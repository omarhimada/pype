using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
    public class Fitting : IAsyncDisposable, IDisposable
    {
        private bool _disposed;

        private readonly ILogger _logger;

        /// <summary>
        /// If no ContentType is provided this default value will be assumed
        /// </summary>
        private const string DefaultContentType = "application/json";

        public Fitting(ILogger logger = null)
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
        private FittingResponse<T> PrepareNewResponse<T>() where T : class =>
          new FittingResponse<T>
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
                _logger?.LogError(
                  $"Fitting threw an exception because it does not have an ApiBasePath");

                throw new ArgumentNullException(nameof(ApiBasePath));
            }
            if (string.IsNullOrEmpty(Method))
            {
                _logger?.LogError(
                  $"Fitting threw an exception because it does not have a Method specified");

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

            WebRequest webRequest = await PrepareWebRequest();

            return await webRequest.GetRequestStreamAsync();
        }

        /// <summary>
        /// Initiate the HTTP request asynchronously
        /// </summary>
        public async Task<FittingResponse<T>> SendRequest<T>() where T : class
        {
            ValidateParameters();

            FittingResponse<T> fittingResponse = PrepareNewResponse<T>();

            try
            {
                WebRequest webRequest = await PrepareWebRequest();

                WebResponse response = await webRequest.GetResponseAsync();
                Stream dataStream = response.GetResponseStream();

                if (dataStream != null)
                {
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();

                    //JToken json = JToken.Parse(responseFromServer);
                    T result = JsonConvert.DeserializeObject<T>(responseFromServer);

                    reader.Close();
                    dataStream.Close();
                    response.Close();

                    fittingResponse.Status.Health = FittingResponseStatusHealth.Good;
                    fittingResponse.Result = result;

                    _logger?.LogInformation(
                      $"Fitting successfully received a response from {webRequest.RequestUri.ToString()}");
                }
            }
            catch (WebException webException)
            {
                fittingResponse.ExceptionThrown = webException;

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

                errorLogBuilder.AppendLine(
                  $"{Environment.NewLine}{webException.Message}{Environment.NewLine}{webException.StackTrace}");

                fittingResponse.Status.Health = FittingResponseStatusHealth.Bad;

                _logger?.LogError(
                  $"{errorLogBuilder}");
            }
            catch (Exception exception)
            {
                fittingResponse.ExceptionThrown = exception;

                fittingResponse.Status.Health = FittingResponseStatusHealth.Bad;

                _logger?.LogError(
                  $"Fitting SendRequest threw an Exception {Environment.NewLine}{exception.Message}{Environment.NewLine}{exception.StackTrace}");
            }

            fittingResponse.Status.ResponseUtcDateTime = DateTime.UtcNow;
            return fittingResponse;
        }

        #region Disposable implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
        }

        public virtual ValueTask DisposeAsync()
        {
            try
            {
                Dispose();
                return default;
            }
            catch (Exception exception)
            {
                return new ValueTask(Task.FromException(exception));
            }
        }
        #endregion
    }
}
