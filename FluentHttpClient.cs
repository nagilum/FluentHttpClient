using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FluentWrappers;

/// <summary>
/// Fluent wrapper for HttpClient.
/// </summary>
public class FluentHttpClient
{
    #region Static properties

    /// <summary>
    /// Pool of HttpClients.
    /// </summary>
    private static Dictionary<Guid, HttpClientPoolEntry> HttpClientPool { get; } = new();

    #endregion

    #region Instance properties

    /// <summary>
    /// Which HttpClient to use from the pool.
    /// </summary>
    private Guid PoolId { get; set; }

    /// <summary>
    /// Request content.
    /// </summary>
    private object? Content { get; set; }

    /// <summary>
    /// Content encoding.
    /// </summary>
    private Encoding? ContentEncoding { get; set; }

    /// <summary>
    /// Content media type.
    /// </summary>
    private string? ContentType { get; set; }

    /// <summary>
    /// Request cookies.
    /// </summary>
    private List<Cookie>? Cookies { get; set; }

    /// <summary>
    /// Request credentials.
    /// </summary>
    private NetworkCredential? Credentials { get; set; }

    /// <summary>
    /// Request headers.
    /// </summary>
    private Dictionary<string, object?>? Headers { get; set; }

    /// <summary>
    /// JSON Serialization options.
    /// </summary>
    private JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Request timeout.
    /// </summary>
    private TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Request uri.
    /// </summary>
    private Uri Uri { get; set; }

    /// <summary>
    /// User agent.
    /// </summary>
    private string? UserAgent { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Create a new instance of the fluent HTTP client.
    /// </summary>
    /// <param name="uri">Uri to request.</param>
    /// <param name="createNewHttpClient">Whether to create a new HttpClient, or reuse the last one.</param>
    public FluentHttpClient(Uri uri, bool createNewHttpClient = false)
    {
        Guid poolId;

        if (HttpClientPool.Count == 0 ||
            createNewHttpClient)
        {
            poolId = Guid.NewGuid();

            HttpClientPool.Add(poolId, new());
        }
        else
        {
            poolId = HttpClientPool
                .Last()
                .Key;
        }
        
        this.PoolId = poolId;
        this.Uri = uri;
    }

    /// <summary>
    /// Create a new instance of the fluent HTTP client.
    /// </summary>
    /// <param name="url">URL to request.</param>
    /// <param name="createNewHttpClient">Whether to create a new HttpClient, or reuse the last one.</param>
    /// <returns>Fluent HTTP client instance.</returns>
    public static FluentHttpClient Create(string url, bool createNewHttpClient = false)
    {
        return new(new(url), createNewHttpClient);
    }

    /// <summary>
    /// Create a new instance of the fluent HTTP client.
    /// </summary>
    /// <param name="uri">Uri to request.</param>
    /// <param name="createNewHttpClient">Whether to create a new HttpClient, or reuse the last one.</param>
    /// <returns>Fluent HTTP client instance.</returns>
    public static FluentHttpClient Create(Uri uri, bool createNewHttpClient = false)
    {
        return new(uri, createNewHttpClient);
    }

    #endregion

    #region Fluent functions

    /// <summary>
    /// Add content (payload) to request.
    /// </summary>
    /// <param name="content">Content to add.</param>
    /// <param name="contentType">Content media type.</param>
    /// <param name="encoding">Content encoding.</param>
    /// <returns>Fluent HTTP client instance.</returns>
    public FluentHttpClient AddContent(
        object content,
        string? contentType = null,
        Encoding? encoding = null)
    {
        this.Content = content;
        this.ContentType = contentType;
        this.ContentEncoding = encoding;

        return this;
    }

    /// <summary>
    /// Add a cookie to request.
    /// </summary>
    /// <param name="cookie">Cookie to add.</param>
    /// <returns>Fluent HTTP client instance.</returns>
    public FluentHttpClient AddCookie(Cookie cookie)
    {
        this.Cookies ??= new();
        this.Cookies.Add(cookie);

        return this;
    }

    /// <summary>
    /// Add several cookies to request.
    /// </summary>
    /// <param name="cookies">Cookies to add.</param>
    /// <returns>Fluent HTTP client instance.</returns>
    public FluentHttpClient AddCookies(params Cookie[] cookies)
    {
        this.Cookies ??= new();
        this.Cookies.AddRange(cookies);

        return this;
    }

    /// <summary>
    /// Add header to request.
    /// </summary>
    /// <param name="name">Name.</param>
    /// <param name="value">Value, if any.</param>
    /// <returns>Fluent HTTP client instance.</returns>
    public FluentHttpClient AddHeader(string name, object? value = null)
    {
        this.Headers ??= new();
        this.Headers.TryAdd(name, value);

        return this;
    }

    /// <summary>
    /// Add headers to request.
    /// </summary>
    /// <param name="headers">Headers to add.</param>
    /// <returns>Fluent HTTP client instance.</returns>
    public FluentHttpClient AddHeaders(Dictionary<string, object?> headers)
    {
        this.Headers ??= new();

        foreach (var (key, value) in headers)
        {
            this.Headers.TryAdd(key, value);
        }

        return this;
    }

    /// <summary>
    /// Set JSON serialization and deserialization options.
    /// </summary>
    /// <param name="options">Options to set.</param>
    /// <returns>Fluent HTTP client instance.</returns>
    public FluentHttpClient SetJsonSerializerOptions(JsonSerializerOptions options)
    {
        this.JsonSerializerOptions = options;
        return this;
    }

    /// <summary>
    /// Set request timeout.
    /// </summary>
    /// <param name="timeout">Timeout.</param>
    /// <returns>Fluent HTTP client instance.</returns>
    public FluentHttpClient SetTimeout(TimeSpan timeout)
    {
        this.Timeout = timeout;
        return this;
    }

    /// <summary>
    /// Set request timeout.
    /// </summary>
    /// <param name="milliseconds">Timeout.</param>
    /// <returns>Fluent HTTP client instance.</returns>
    public FluentHttpClient SetTimeout(int milliseconds)
    {
        this.Timeout = TimeSpan.FromMilliseconds(milliseconds);
        return this;
    }

    /// <summary>
    /// Set user-agent for request.
    /// </summary>
    /// <param name="userAgent">User-agent.</param>
    /// <returns>Fluent HTTP client instance.</returns>
    public FluentHttpClient SetUserAgent(string userAgent)
    {
        this.UserAgent = userAgent;
        return this;
    }

    #endregion

    #region Request function

    /// <summary>
    /// Perform request.
    /// </summary>
    /// <param name="httpMethod">HTTP method.</param>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>Response message.</returns>
    public async Task<dynamic?> SendAsync<T>(
        HttpMethod httpMethod,
        CancellationToken ctoken = default)
    {
        var client = HttpClientPool[this.PoolId].HttpClient;
        var handler = HttpClientPool[this.PoolId].HttpClientHandler;

        var req = new HttpRequestMessage(httpMethod, this.Uri);

        // Add content.
        if (this.Content is not null)
        {
            if (this.Content is string str)
            {
                if (this.ContentType is null)
                {
                    req.Content = new StringContent(
                        str,
                        this.ContentEncoding);
                }
                else
                {
                    req.Content = new StringContent(
                        str,
                        this.ContentEncoding,
                        this.ContentType);
                }
            }
            else if (this.Content is byte[] bytes)
            {
                req.Content = new ByteArrayContent(bytes);

                if (this.ContentType is not null)
                {
                    req.Headers.TryAddWithoutValidation("Content-Type", this.ContentType);
                }
            }
            else
            {
                using var stream = new MemoryStream();

                await JsonSerializer.SerializeAsync(
                    stream,
                    this.Content,
                    options: this.JsonSerializerOptions,
                    cancellationToken: ctoken);

                req.Content = new ByteArrayContent(stream.ToArray());

                if (this.ContentType is not null)
                {
                    req.Headers.TryAddWithoutValidation("Content-Type", this.ContentType);
                }
            }
        }

        // Add cookies.
        if (this.Cookies?.Count > 0)
        {
            handler.UseCookies = true;
            
            foreach (var cookie in this.Cookies)
            {
                handler.CookieContainer.Add(cookie);
            }
        }

        // Add credentials.
        if (this.Credentials is not null)
        {
            handler.Credentials = this.Credentials;
        }

        // Add headers.
        if (this.Headers?.Count > 0)
        {
            foreach (var (key, value) in this.Headers)
            {
                req.Headers.TryAddWithoutValidation(key, value?.ToString());
            }
        }

        // Set timeout.
        if (this.Timeout.HasValue)
        {
            client.Timeout = this.Timeout.Value;
        }

        // Set user-agent.
        if (this.UserAgent is not null)
        {
            client
                .DefaultRequestHeaders
                .UserAgent
                .TryParseAdd(this.UserAgent);
        }

        // Perform request and output.
        var res = await client.SendAsync(req, ctoken);

        if (res is null)
        {
            return default;
        }

        var typeOfT = typeof(T);

        if (typeOfT == typeof(HttpResponseMessage))
        {
            return res;
        }
        else if (typeOfT == typeof(byte[]))
        {
            return await res.Content.ReadAsByteArrayAsync(ctoken);
        }
        else if (typeOfT == typeof(string))
        {
            return await res.Content.ReadAsStringAsync(ctoken);
        }
        else if (typeof(Stream).IsAssignableFrom(typeOfT))
        {
            return await res.Content.ReadAsStreamAsync(ctoken);
        }
        else
        {
            return await res.Content.ReadFromJsonAsync<T>(this.JsonSerializerOptions, ctoken);
        }
    }

    #endregion

    #region Shorthand functions

    /// <summary>
    /// Perform a DELETE request.
    /// </summary>
    /// <typeparam name="T">Type to cast response content to.</typeparam>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>Type to cast response content to.</returns>
    public async Task<T?> DeleteAsync<T>(CancellationToken ctoken = default)
    {
        return await this.SendAsync<T>(HttpMethod.Delete, ctoken);
    }

    /// <summary>
    /// Perform a GET request.
    /// </summary>
    /// <typeparam name="T">Type to cast response content to.</typeparam>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>Type to cast response content to.</returns>
    public async Task<T?> GetAsync<T>(CancellationToken ctoken = default)
    {
        return await this.SendAsync<T>(HttpMethod.Get, ctoken);
    }

    /// <summary>
    /// Perform a PATCH request.
    /// </summary>
    /// <typeparam name="T">Type to cast response content to.</typeparam>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>Type to cast response content to.</returns>
    public async Task<T?> PatchAsync<T>(CancellationToken ctoken = default)
    {
        return await this.SendAsync<T>(HttpMethod.Patch, ctoken);
    }

    /// <summary>
    /// Perform a POST request.
    /// </summary>
    /// <typeparam name="T">Type to cast response content to.</typeparam>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>Type to cast response content to.</returns>
    public async Task<T?> PostAsync<T>(CancellationToken ctoken = default)
    {
        return await this.SendAsync<T>(HttpMethod.Post, ctoken);
    }

    /// <summary>
    /// Perform a PUT request.
    /// </summary>
    /// <typeparam name="T">Type to cast response content to.</typeparam>
    /// <param name="ctoken">Cancellation token.</param>
    /// <returns>Type to cast response content to.</returns>
    public async Task<T?> PutAsync<T>(CancellationToken ctoken = default)
    {
        return await this.SendAsync<T>(HttpMethod.Put, ctoken);
    }

    #endregion

    #region Helper class

    /// <summary>
    /// HttpClient pool entry.
    /// </summary>
    internal class HttpClientPoolEntry
    {
        /// <summary>
        /// HTTP client.
        /// </summary>
        public HttpClient HttpClient { get; private set; }

        /// <summary>
        /// HTTP client handler.
        /// </summary>
        public HttpClientHandler HttpClientHandler { get; private set; }

        /// <summary>
        /// Create new pool entry instance.
        /// </summary>
        public HttpClientPoolEntry()
        {
            this.HttpClientHandler = new();
            this.HttpClient = new(this.HttpClientHandler);
        }
    }

    #endregion
}