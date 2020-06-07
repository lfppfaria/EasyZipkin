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
.WithServiceName("sample_api")
.WithCollectorUri("http://localhost:9411")
.Build();
```
Next we just need to tell the method that we wanna trace:

```C# 
[Trace]
[HttpGet]
public async Task<IActionResult> Get()
{
	//Do your stuff...
}
```

The above aproach will use the method name as zipkin operation name.
But if you preffer you may tell us how to store the trace:

```C# 
[Trace("myfancyname")]
[HttpGet]
public async Task<IActionResult> Get()
{
	//Do your stuff...
}
```

After this simples setup if we go to Zipkin to find our trace we will find something like this:

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/FirstTrace.JPG?raw=true)

We will automatically trace any methods with the Trace attribute within the call stack.
So let's spice the things up a bit and call a method from our entry point.

```C# 
[HttpGet]
[Trace]
public async Task<IActionResult> Get()
{
    await new MyClassWithSomeStuff().DoSomethingAsync();

    return Ok();
}
```
And on my DoSomethingAsync method we just decorate with the TraceAttribute...

[Trace]
public Task DoSomethingAsync()
{
	//let's wait a sec..
    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));

    return Task.CompletedTask;
}

Now let's take a look at Zipkin and we should have something like this:

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/NestedTrace.JPG?raw=true)

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
public async Task<IActionResult> Get()
{
    //Do your stuff...            
}
```
Our library automatically will detect any zipkin header and resume tracing, if the request is an untraced request we will just trace from this point onwards.

If everything is wired up properly we will have a trace wich started on our client application an resume tracing on a remote resource:

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/RemoteTraceTimeline.JPG?raw=true)

With the annotations giving us some info about how things happened:

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/RemoteTraceAnnotations.JPG?raw=true)

#### Queues

A lot of applications use message queues such as RabbitMQ, IBMMQ, Azure or any other and sometimes might be interesting to trace the acts of producing and/or consuming:

```C#
using (new ProducerTracer())
{
    //Produce to queue...
}
```
```C#
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