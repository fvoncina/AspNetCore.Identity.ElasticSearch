# AspNetCore.Identity.ElasticSearch
AspNetCore ElasticSearch Identity Provider

![VSO Build](https://img.shields.io/vso/build/fvoncina/abd2112d-436d-4dd9-900f-3bf78a4a8791/11.svg)
[![NuGet package version](https://img.shields.io/nuget/v/AspNetCore.Identity.ElasticSearch.svg)](https://www.nuget.org/packages/AspNetCore.Identity.ElasticSearch)

[ElasticSearch](https://www.elastic.co/products/elasticsearch) data store adaptor for [ASP.NET Core Identity](https://github.com/aspnet/Identity),
which allows you to build ASP.NET Core web applications, including membership, login, and user data.
With this library, you can store your user's membership related data on ElasticSearch.

## Using the Library

[The library is available at NuGet.org](https://www.nuget.org/packages/AspNetCore.Identity.ElasticSearch).
You can start using the library by integrating it into your `.csproj` file.
This library supports [`netstandard1.6`](https://docs.microsoft.com/en-us/dotnet/articles/standard/library).

### Samples

You can find some samples under [./sample](./sample) folder and each of the sample contain a README file
on its own with the instructions showing how to run them.

### Tests

In order to be able to run the tests, you need to have DynamoDB up and running on `localhost:8000`.
You can easily do this by running the below Docker command:

```bash
docker run -d -p 9200:9200 -p 9300:9300 elasticsearch
```

After that, you can run the tests through your preferred test runner (e.g. JetBrains Rider test runner)
or by invoking the `dotnet test` command under the test project directory.
If your are running docker on Windows you will need to change the appsettings.json url for the ElasticSearch
using the container ip/port.


## Notes

The project is based on awesome work of [@miltador](https://github.com/miltador) who created [DynamoDB](https://github.com/miltador/AspNetCore.Identity.DynamoDB) 
adaptor to AspNetCore, rewritten and adapted to the specifics of ElasticSearch.

## License

The MIT License (MIT)

Copyright (c) 2016 Tugberk Ugurlu

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
