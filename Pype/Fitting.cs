using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pype.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Pype
{
  /// <summary>
  /// Generic API utility for integrating with third-parties
  /// </summary>
  public class Fitting
  {
    private readonly ILogger<Fitting> _logger;

    /// <summary>
    /// If no ContentType is provided this default value will be assumed
    /// </summary>
    private const string DefaultContentType = "application/json";

    public Fitting(ILogger<Fitting> logger = null)
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
    /// Initiate the HTTP request 
    /// </summary>
    /// <returns>JObject</returns>
    public async Task<FittingResponse> SendRequest()
    {
      #region Validate all required parameters are present
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
      #endregion

      FittingResponse fittingResponse = new FittingResponse
      {
        Status = new FittingResponseStatus
        {
          RequestUtcDateTime = DateTime.UtcNow
        }
      };

      try
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

        WebResponse response = await webRequest.GetResponseAsync();
        Stream dataStream = response.GetResponseStream();

        if (dataStream != null)
        {
          StreamReader reader = new StreamReader(dataStream);
          string responseFromServer = reader.ReadToEnd();

          JObject json = JObject.Parse(responseFromServer);

          reader.Close();
          dataStream.Close();
          response.Close();

          fittingResponse.Status.Health = FittingResponseStatusHealth.Good;
          fittingResponse.Result = json;
        }
      }
      catch (Exception e)
      {
        fittingResponse.Status.Health = FittingResponseStatusHealth.Bad;

        _logger?.LogError($"Fitting SendRequest threw exception {e.Message}{Environment.NewLine}{e.StackTrace}");
      }

      fittingResponse.Status.ResponseUtcDateTime = DateTime.UtcNow;
      return fittingResponse;
    }
  }
}
