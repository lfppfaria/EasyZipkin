# EasyZipkin

Another Zipkin library aiming to ease usage of zipkin capabilities.

## About Zipkin

[Zipkin](https://zipkin.io) is a distributed tracing system wich helps gather information to troubleshoot latency problems in service oriented architectures.

More info will be found on their web site: [https://zipkin.io](https://zipkin.io)

## Installation

Just grab it from NuGet.

### MethodBoundaryAspect.Fody

Since we use MethodBoudaryAspect.Fody to add some AOP capabilities to our library you need to install it from NuGet as well on every project you desire to trace.

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/MethodBoundaryAspect.Fody.JPG?raw=true)

Or using command line:

install-package MethodBoundaryAspect.Fody

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

After this simple setup if we go to Zipkin to ckeck onour trace we will find something like this:

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/FirstTrace.JPG?raw=true)

We will automatically trace any methods with the TraceAttribute within the call stack. 
So let's spice the things up a bit and call a method from our entry point.

```C# 
[HttpGet]
[Trace]
public async Task<IActionResult> Get()
{
    _myClassWithSomeStuff.DoSomethingAsync();

    return Ok();
}
```

And on our DoSomethingAsync method we just decorate with the TraceAttribute...

```C#
[Trace]
public Task DoSomethingAsync()
{
	//let's wait a sec..
	System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));

	return Task.CompletedTask;
}
```

Now let's take a look at Zipkin and we should have something like this:

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/NestedTrace.JPG?raw=true)

#### Annotations

We are also able to add some custom annotations to our trace.

```C#
TracerContext.RegisterEvent("Hi there!");
```

It wil be registred within the current Trace just like this:

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/CustomAnnotation.JPG?raw=true)

#### Exceptions

If something goes wrong wile tracing we automatically add a Exception Annotation on the span with the exception and the stack trace.
Note that our goal is not to turn Zipkin into a logging tool and you should not use on this way as well.
But we think it could be useful have some register if something goes wrong.

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/Exception.JPG?raw=true)

### Other tracing types

So far we covered the basics of tracing but we have some more stuff to show:

#### Trace over http

Zipkin is capable of send trace headers over http and resume tracing on the resource (if the resource also uses zipkin and it is properly configured, of course).
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

Our library automatically will detect any zipkin header and resume tracing, if the request is an untraced request we will just trace from the entry point onwards.

If everything is wired up properly we will have a trace wich started on our client application an resumed tracing on a remote resource:

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/RemoteTraceTimeline.JPG?raw=true)

With the annotations giving us some info about how things happened:

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/RemoteTraceAnnotations.JPG?raw=true)

#### Queues

A lot of applications use message queues such as RabbitMQ, IBMMQ, Azure or any other and sometimes might be interesting to trace the acts of producing and/or consuming:

```C#
using (new ProducerTracer("producing to test mq")
{
    //Produce to queue...
}
```

Zipkin will record the submission to the queue like this:

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/ProduceTrace.JPG?raw=true)

```C#
using (new ConsumerTracer("consuming from mq"))
{
    //Consume from queue...
}
```

And the consumption will be registred like this:

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/ConsumeTrace.JPG?raw=true)

### Parallel and Async code

Zipkin deals very well with parallel and async code but our goal is make things even easier.
Thanks to [AsyncLocal](https://docs.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1?view=netcore-3.1) we are able to trace over an async stack.
The usage is exactly the same.
Let's see some code and a sample output:

First let's suppose that we have a method wich will call our DoSomethingAsync:

```C#
[Trace]
public async Task<IActionResult> Get()
{
    await _myClassWithSomeStuff.DoSomethingAsync();

    return Ok();
}
```

And the DoSomethingAsync will have a parallel for calling an async DoAnotherSomethingAsync wich will finally call JustAnotherMethod.

Something like: DoSomethingAsync > DoAnotherSomethingAsync > JustAnotherMethod

```C#
[Trace]
public Task DoSomethingAsync()
{
    Parallel.For(0, 3, async (item) =>
    {
        await DoAnotherSomethingAsync();
    });

    return Task.CompletedTask;
}

[Trace]
public Task DoAnotherSomethingAsync()
{
    Parallel.For(0, 3, async (item) =>
    {
        await JustAnotherMethod();
    });

    return Task.CompletedTask;
}

[Trace]
public Task JustAnotherMethod()
{
    return Task.CompletedTask;
}
```

Now let's take another look on Zipkin:

![Alt text](https://github.com/lfppfaria/EasyZipkin/blob/master/Images/ParallelAndAsyncTrace.JPG?raw=true)

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)