# EasyZipkin

Another Zipkin library aiming to ease usage of zipkin capabilities.

## Installation

Just grab it from NuGet.

Since we use MethodBoudaryAspect.Fody to add some AOP capabilities to our library you need to install it from NuGet as well on every project you desire to trace.

## Usage

### Basics

From your Startup.cs (or Program.cs) just create a new trace context with service name and the collector url:

```C#
new TracerContextBuilder()
.WithServiceName("WatherForecast")
.WithCollectorUri("http://localhost:9411")
.Build();
```
Next we just need to tell the method that we wanna trace:

```C# 
[Trace]
public IEnumerable<WeatherForecast> Get()
{
    //Do your stuff
}
```

The above aproach will use the method name as zipkin operation name.
But if you preffer you may tell us how to store the trace:

```C# 
[Trace("myfancyname")]
public IEnumerable<WeatherForecast> Get()
{
    //Do your stuff...
}
```

### Other tracing types

So far we covered the basics of tracing but we have some more stuff to show:

#### Trace over http

Zipkin is capable of send trace headers over http and resume tracing on the resource (since the resource also uses zipkin and it is porperly configured, of course).
To help on this situation we implemented a RequestTracer, witch automatically adds the traces headers for you and the service will continue tracing.
From the client:

```C#
var httpClient = _httpClientFactory.CreateClient();

var request = new HttpRequestMessage(HttpMethod.Get, "https://myremotecall");

using (new HttpRequestTracer(request))
{
    var response = await httpClient.SendAsync(request);
}
```
On the remote server you just need to use our TraceHeaderMiddleware:

```C#
app.UseMiddleware<TraceHeaderMiddleware>();
```
And trace the controller method as usual:

```C#
[HttpGet]
[Trace]
public async Task<IActionResult> AnotherGet()
{
    //Do your stuff...            
}
```
Our library automatically will detect any zipkin header and resume tracing, if the request is an untraced request we will just trace from this point onwards.

#### Queues

A lot of applications use message queues such as RabbitMQ, IBMMQ, Azure or any other and sometimes might be interesting to trace the acts of producing and/or consuming:

```C#
using (new ProducerTracer())
{
    //Produce to queue...
}

using (new ConsumerTracer())
{
    //Consume from queue...
}
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)