﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nerdbank.Streams;
using OmniSharp.Extensions.JsonRpc.Server;

namespace OmniSharp.Extensions.JsonRpc
{
    public abstract class JsonRpcServerOptionsBase<T> : JsonRpcOptionsRegistryBase<T>, IJsonRpcServerOptions where T : IJsonRpcHandlerRegistry<T>
    {
        public PipeReader Input { get; set; }
        public PipeWriter Output { get; set; }
        public ILoggerFactory LoggerFactory { get; set; } = new LoggerFactory();
        public IEnumerable<Assembly> Assemblies { get; set; } = Enumerable.Empty<Assembly>();
        public abstract IRequestProcessIdentifier RequestProcessIdentifier { get; set; }
        public int? Concurrency { get; set; }
        public Func<ServerError, string, Exception> CreateResponseException { get; set; }
        public Action<Exception> OnUnhandledException { get; set; }
        public bool SupportsContentModified { get; set; } = true;
        public TimeSpan MaximumRequestTimeout { get; set; } = TimeSpan.FromMinutes(5);
        internal CompositeDisposable CompositeDisposable { get; } = new CompositeDisposable();
        IDisposable IJsonRpcServerOptions.RegisteredDisposables => CompositeDisposable;

        public void RegisterForDisposal(IDisposable disposable)
        {
            CompositeDisposable.Add(disposable);
        }

        public T WithAssemblies(IEnumerable<Assembly> assemblies)
        {
            Assemblies = Assemblies.Concat(assemblies);
            return (T)(object) this;
        }

        public T WithAssemblies(params Assembly[]  assemblies)
        {
            Assemblies = Assemblies.Concat(assemblies);
            return (T)(object) this;
        }

        public T WithInput(Stream input)
        {
            Input = input.UsePipeReader();
            RegisterForDisposal(input);
            return (T)(object) this;
        }
        public T WithInput(PipeReader input)
        {
            Input = input;
            return (T)(object) this;
        }

        public T WithOutput(Stream output)
        {
            Output = output.UsePipeWriter();
            RegisterForDisposal(output);
            return (T)(object) this;
        }

        public T WithOutput(PipeWriter output)
        {
            Output = output;
            return (T)(object) this;
        }

        public T WithPipe(Pipe pipe)
        {
            Input = pipe.Reader;
            Output = pipe.Writer;
            return (T)(object) this;
        }

        public T WithLoggerFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
            return (T)(object) this;
        }

        public T WithRequestProcessIdentifier(IRequestProcessIdentifier requestProcessIdentifier)
        {
            RequestProcessIdentifier = requestProcessIdentifier;
            return (T)(object) this;
        }

        public T WithHandler<THandler>(JsonRpcHandlerOptions options = null)
            where THandler : class, IJsonRpcHandler
        {
            return AddHandler<THandler>(options);
        }

        public T WithHandler<THandler>(THandler handler, JsonRpcHandlerOptions options = null)
            where THandler : class, IJsonRpcHandler
        {
            return AddHandler(handler, options);
        }

        public T WithHandlersFrom(Type type, JsonRpcHandlerOptions options = null)
        {
            return AddHandler(type, options);
        }

        public T WithHandlersFrom(TypeInfo typeInfo, JsonRpcHandlerOptions options = null)
        {
            return AddHandler(typeInfo.AsType(), options);
        }

        public T WithServices(Action<IServiceCollection> servicesAction)
        {
            servicesAction(Services);
            return (T)(object) this;
        }

        public T WithResponseExceptionFactory(Func<ServerError, string, Exception> handler)
        {
            CreateResponseException = handler;
            return (T)(object) this;
        }

        public T WithUnhandledExceptionHandler(Action<Exception> handler)
        {
            OnUnhandledException = handler;
            return (T)(object) this;
        }

        public T WithContentModifiedSupport(bool supportsContentModified)
        {
            SupportsContentModified = supportsContentModified;
            return (T)(object) this;
        }

        public T WithMaximumRequestTimeout(TimeSpan maximumRequestTimeout)
        {
            MaximumRequestTimeout = maximumRequestTimeout;
            return (T)(object) this;
        }
    }
}