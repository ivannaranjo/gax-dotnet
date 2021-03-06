[![Build Status](https://travis-ci.org/googleapis/gax-dotnet.svg?branch=master)](https://travis-ci.org/googleapis/gax-dotnet)

This repository's primary library is `Google.Api.Gax`, but
additional code is present for the support of Google's
REST/JSON/HTTP1.1 APIs. This is present in the `Google.Api.Gax.Rest`
library, which is [described separately](rest.md).

Google API Extensions for .NET
===

Google API Extensions for .NET (`Google.Api.Gax`) is a set of libraries which
aids the development of APIs, client and server, based on
[gRPC](http://grpc.io) and Google API conventions.

Application code will rarely need to use most of the classes within this
library directly, but code generated automatically from the [API definition
files](https://github.com/googleapis/googleapis/)
can use services such as page streaming to provide
a more convenient and idiomatic API surface to callers.

Quickstart
---

Currently there is no public package for this library. When we come
closer to release, it will be published on
[NuGet](https://nuget.org) with an expected package name of
`Google.Api.Gax`. However, it will also be a dependency of specific
API packages, so it is unlikely that you will need to add an
explicit dependency on this library.

Supported platforms
---

We are initially targeting support for .NET Core and .NET 4.5. As
.NET Core is not yet released, support is only provisional.

Contributing
------------

Contributions to this library are always welcome and highly encouraged.

See the
[CONTRIBUTING](https://github.com/googleapis/gax-dotnet/blob/master/CONTRIBUTING)
documentation for more information on how to get started.


Versioning
----------

This library follows [semantic versioning](http://semver.org).

It is currently in major version zero (`0.y.z`), which means that anything
may change at any time and the public API should not be considered
stable.

Details
---

An API package (such as `Google.Pubsub.V1`) will typically contain the following
code:

- Code corresponding to the Protocol Buffer messages within the API, generated
  by vanilla `protoc` (the protocol compiler). These are data-only
  classes such as `Topic` and `Subscription`.
- Low-level code corresponding to the Protocol Buffer services within the API,
  generated by `protoc` using `grpc_csharp_plugin` provided by the gRPC project.
  These are service classes and interfaces such as `IPublisherClient`.
- High-level code generated by the Google API Code Generator
  which understands the Google API conventions such as page streaming, and can be
  configured manually when additional features are provided.
- Hand-written code to provide additional platform-idiomatic and API-specific
  code, such as a C# event raised when a pubsub topic receives a message. This
  may be minimal, or it may provide a signficant amount of logic over the top
  of the API itself.

Such an API package depends on other libraries:

- [`Google.Protobuf`](https://github.com/google/protobuf) for message serialization
- [`Grpc.Core`](https://github.com/grpc/grpc) for RPC services
- [`Google.Apis.Auth`](https://github.com/google/google-api-dotnet-client)
  (and `Grpc.Auth`) for authentication
- `Google.Api.Gax` (this library) for support services required by the
  high-level code, and common API Protocol Buffer messages.

Code in this repository
---

Namespaces generated from common protocol buffer types:

- `Google.Api` - Contains types that comprise the configuration model for 
  API services.
- `Google.Longrunning` - Contains the standardized types for defining
  long running operations.
- `Google.Rpc` - Contains the types for general RPC systems. While gRPC uses
  these types, they are not designed specifically to be gRPC-specific.
- `Google.Type` - Common types for Google APIs. All types defined in this
  package are suitable for different APIs to exchange data, and will never break
  binary compatibility.
  
Namespaces containing non-generated code:

- `Google.Api.Gax` - Contains support classes for features such as
  page streaming, resource name composition and decomposition, and
  request bundling.

License
-------

BSD - See
[LICENSE](https://github.com/googleapis/gax-dotnet/blob/master/LICENSE)
for more information.
