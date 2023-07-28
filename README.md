# FluentHttpClient

FluentHttpClient is a wrapper class for the native HttpClient in C# which provides fluent-like functions.

## Examples

If you want to get the HTML from a site:

```csharp
var html = await FluentHttpClient.Create("https://microsoft.com")
    .GetAsync<string>();
```

If you want to get an image as a stream with some headers:

```csharp
var stream = await FluentHttpClient.Create("https://url.yo/some/image")
    .AddHeader("accept", "image/jpeg")
    .GetAsync<MemoryStream>();
```

If you want to post some data that times out after 10 seconds:

```csharp
var obj = new
{
    name = "Some Dude",
    age = 43,
    alive = true
};

var res = await FluentHttpClient.Create("https://some.url/that/takes/post")
    .AddContent(obj, "application/json")
    .SetTimeout(TimeSpan.FromSeconds(10))
    .PostAsync<HttpResponseMessage>();
```

## Constructors

There are two variations of the `Create` function.

```csharp
/// <summary>
/// Create a new instance of the fluent HTTP client.
/// </summary>
/// <param name="url">URL to request.</param>
/// <param name="createNewHttpClient">Whether to create a new HttpClient, or reuse the last one.</param>
/// <returns>Fluent HTTP client instance.</returns>
FluentHttpClient Create(string url, bool createNewHttpClient = false)
```

```csharp
/// <summary>
/// Create a new instance of the fluent HTTP client.
/// </summary>
/// <param name="uri">Uri to request.</param>
/// <param name="createNewHttpClient">Whether to create a new HttpClient, or reuse the last one.</param>
/// <returns>Fluent HTTP client instance.</returns>
FluentHttpClient Create(Uri uri, bool createNewHttpClient = false)
```

The `url`/`uri` params is where you provide the URL to contact.

The `createNewHttpClient` param is for when you absolutely need to create a new instance of `HttpClient` under the hood.

As per Microsofts documentation: <https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-7.0>

> HttpClient is intended to be instantiated once and reused throughout the life of an application. In .NET Core and .NET 5+, HttpClient pools connections inside the handler instance and reuses a connection across multiple requests. If you instantiate an HttpClient class for every request, the number of sockets available under heavy loads will be exhausted. This exhaustion will result in SocketException errors.

However, the `AddCookie`, `AddCookies`, `SetTimeout`, and `SetUserAgent` fluent functions set data directly on the `HttpClient`, so if you make one request with a cookie added, and another after, the second request will contain the same cookie. If you need this to differ, that's where the `createNewHttpClient` param comes in. To force a new `HttpClient` under the hood, simply use set it to `true`.

## Fluent Functions

### AddContent

```csharp
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
```

### AddCookie

```csharp
/// <summary>
/// Add a cookie to request.
/// </summary>
/// <param name="cookie">Cookie to add.</param>
/// <returns>Fluent HTTP client instance.</returns>
public FluentHttpClient AddCookie(Cookie cookie)
```

### AddCookies

```csharp
/// <summary>
/// Add several cookies to request.
/// </summary>
/// <param name="cookies">Cookies to add.</param>
/// <returns>Fluent HTTP client instance.</returns>
public FluentHttpClient AddCookies(params Cookie[] cookies)
```

### AddHeader

```csharp
/// <summary>
/// Add header to request.
/// </summary>
/// <param name="name">Name.</param>
/// <param name="value">Value, if any.</param>
/// <returns>Fluent HTTP client instance.</returns>
public FluentHttpClient AddHeader(string name, object? value = null)
```

### AddHeaders

```csharp
/// <summary>
/// Add headers to request.
/// </summary>
/// <param name="headers">Headers to add.</param>
/// <returns>Fluent HTTP client instance.</returns>
public FluentHttpClient AddHeaders(Dictionary<string, object?> headers)
```

### SetJsonSerializerOptions

```csharp
/// <summary>
/// Set JSON serialization and deserialization options.
/// </summary>
/// <param name="options">Options to set.</param>
/// <returns>Fluent HTTP client instance.</returns>
public FluentHttpClient SetJsonSerializerOptions(JsonSerializerOptions options)
```

### SetTimeout

```csharp
/// <summary>
/// Set request timeout.
/// </summary>
/// <param name="timeout">Timeout.</param>
/// <returns>Fluent HTTP client instance.</returns>
public FluentHttpClient SetTimeout(TimeSpan timeout)
```

### SetTimeout

```csharp
/// <summary>
/// Set request timeout.
/// </summary>
/// <param name="milliseconds">Timeout.</param>
/// <returns>Fluent HTTP client instance.</returns>
public FluentHttpClient SetTimeout(int milliseconds)
```

### SetUserAgent

```csharp
/// <summary>
/// Set user-agent for request.
/// </summary>
/// <param name="userAgent">User-agent.</param>
/// <returns>Fluent HTTP client instance.</returns>
public FluentHttpClient SetUserAgent(string userAgent)
```

## Request Function

The request function, or one of the shorthand functions below, must be at the end of the fluent chain. The shorthand functions just use this main request function behind the scenes.

```csharp
/// <summary>
/// Perform request.
/// </summary>
/// <param name="httpMethod">HTTP method.</param>
/// <param name="ctoken">Cancellation token.</param>
/// <returns>Response message.</returns>
public async Task<dynamic?> SendAsync<T>(
    HttpMethod httpMethod,
    CancellationToken ctoken = default)
```

## Shorthand Functions

If you don't want to use the `SendAsync` request function, you can use one of 5 shorthand functions that specify the HTTP method on their own. The only thing they do is use the `SendAsync` function behind the scenes with the HTTP method set.

They are:
* `DeleteAsync`
* `GetAsync`
* `PatchAsync`
* `PostAsync`
* `PutAsync`

## Different Return Types

By default `HttpClient` supports 4 different reads as well as the underlying `HttpResponseMessage` itself, so all these are implemented in the `FluentHttpClient` wrapper as well.

* `SendAsync<HttpResponseMessage>()` will give you the raw `HttpResponseMessage`.
* `SendAsync<byte[]>()` will give you a byte-array of the response body.
* `SendAsync<string>()` will give you a string of the response body.
* `SendAsync<Stream>()` will give you a stream of the response body. Any stream type that is assignable from the base `Stream` type will work.
* Any other type will be treated as a JSON deserialization.