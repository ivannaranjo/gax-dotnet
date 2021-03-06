﻿/*
 * Copyright 2016 Google Inc. All Rights Reserved.
 * Use of this source code is governed by a BSD-style
 * license that can be found in the LICENSE file or at
 * https://developers.google.com/open-source/licenses/bsd
 */

using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Interfaces and implementations of synchronous page streaming. These are gathered into a single file
// for convenience.

namespace Google.Api.Gax
{
    /// <summary>
    /// A sequence of resources obtained via API responses, each of which contributes a page of resources.
    /// Application code can treat this as a simple sequence (with API calls automatically being made
    /// lazily as more results are required), or call <see cref="AsPages"/> to retrieve
    /// a page at a time, potentially with additional information.
    /// </summary>
    /// <typeparam name="TResponse">The API response type. Each response contains a page of resources.</typeparam>
    /// <typeparam name="TResource">The resource type contained within the response.</typeparam>
    public interface IPagedEnumerable<TResponse, TResource> : IEnumerable<TResource>
    {
        /// <summary>
        /// Returns the sequence of API responses, each of which contributes a page of
        /// resources to this sequence.
        /// </summary>
        /// <returns>A sequence of API responses, each containing a page of resources.</returns>
        IResponseEnumerable<TResponse, TResource> AsPages();
    }

    /// <summary>
    /// A sequence of API responses, each of which contains a page of resources and
    /// potentially additional information (depending on the API).
    /// </summary>
    /// <typeparam name="TResponse">The API response type. Each response contains a page of resources.</typeparam>
    /// <typeparam name="TResource">The resource type contained within the response.</typeparam>
    public interface IResponseEnumerable<TResponse, TResource> : IEnumerable<TResponse>
    {
        /// <summary>
        /// Begins lazily iterating over the sequence of pages, with an iterator allowing the page size to
        /// be specified on each step.
        /// </summary>
        /// <returns>An iterator over the pages in the sequence.</returns>
        new IResponseEnumerator<TResponse> GetEnumerator();

        /// <summary>
        /// Creates a lazily-evaluated sequence of pages of resources, where each page other
        /// than the final one is guaranteed to contain exactly <paramref name="pageSize"/> resources.
        /// </summary>
        /// <remarks>
        /// "Natural" pages returned by the API may contain a smaller number of resources than requested.
        /// For example, a request for a page with 100 resources may return a page with 80 resources but
        /// a next page token for more to be retrieved. This is suitable for batch-processing, but not
        /// for user-visible paging such as in a web application, where fixed-size pages are expected.
        /// </remarks>
        /// <param name="pageSize">The page size. Must be greater than 0.</param>
        /// <returns>A lazily-evaluated sequence of fixed-size pages.</returns>
        IEnumerable<FixedSizePage<TResource>> WithFixedSize(int pageSize);
    }

    /// <summary>
    /// An iterator over a sequence of pages of resources. This is similar to a regular
    /// <see cref="IEnumerator{T}"/>, except it allows the size of the requested page to
    /// be modified along the way.
    /// </summary>
    /// <typeparam name="T">The page type.</typeparam>
    public interface IResponseEnumerator<T> : IEnumerator<T>
    {
        /// <summary>
        /// Advance to the next page in the sequence, requesting the specified page size.
        /// </summary>
        /// <param name="pageSize">The number of resources to include in the next page.</param>
        /// <returns><c>true</c> if the enumerator was successfully advanced to the next element; <c>false</c> if the enumerator has passed the end of the collection.</returns>
        bool MoveNext(int pageSize);
    }

    /// <summary>
    /// A sequence of resources, obtained lazily via API operations which retrieve a page at a time.
    /// </summary>
    /// <typeparam name="TRequest">The API request type.</typeparam>
    /// <typeparam name="TResponse">The API response type. Each response contains a page of resources.</typeparam>
    /// <typeparam name="TResource">The resource type contained within the response.</typeparam>
    public sealed class PagedEnumerable<TRequest, TResponse, TResource> : IPagedEnumerable<TResponse, TResource>
        where TRequest : class, IPageRequest, IMessage<TRequest>
        where TResponse : class, IPageResponse<TResource>, IMessage<TResponse>
    {
        private readonly IResponseEnumerable<TResponse, TResource> _pages;

        /// <summary>
        /// Creates a new lazily-evaluated sequence from the given API call, initial request, and call settings.
        /// </summary>
        /// <remarks>The request is cloned each time the sequence is evaluated.</remarks>
        /// <param name="apiCall">The API call made each time a page is required.</param>
        /// <param name="request">The initial request.</param>
        /// <param name="callSettings">The settings to apply to each API call.</param>
        public PagedEnumerable(ApiCall<TRequest, TResponse> apiCall,
            TRequest request, CallSettings callSettings)
        {
            _pages = new ResponseEnumerable<TRequest, TResponse, TResource>(apiCall, request, callSettings);
        }

        /// <inheritdoc/>
        public IResponseEnumerable<TResponse, TResource> AsPages() => _pages;

        /// <inheritdoc/>
        public IEnumerator<TResource> GetEnumerator() => _pages.SelectMany(page => page).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// A sequence of API responses, each containing a page of resources.
    /// </summary>
    /// <typeparam name="TRequest">The API request type.</typeparam>
    /// <typeparam name="TResponse">The API response type.</typeparam>
    /// <typeparam name="TResource">The resource type contained within the response.</typeparam>
    internal sealed class ResponseEnumerable<TRequest, TResponse, TResource> : IResponseEnumerable<TResponse, TResource>
        where TRequest : class, IPageRequest, IMessage<TRequest>
        where TResponse : class, IPageResponse<TResource>, IMessage<TResponse>
    {
        private readonly CallSettings _callSettings;
        private readonly TRequest _request;
        private readonly ApiCall<TRequest, TResponse> _apiCall;

        internal ResponseEnumerable(ApiCall<TRequest, TResponse> apiCall, TRequest request, CallSettings callSettings)
        {
            _callSettings = callSettings;
            _request = request;
            _apiCall = apiCall;
        }

        /// <inheritdoc />
        public IResponseEnumerator<TResponse> GetEnumerator() =>
            new ResponseEnumerator(_callSettings, _request.Clone(), _apiCall);

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        /// <inheritdoc />
        IEnumerator<TResponse> IEnumerable<TResponse>.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerable<FixedSizePage<TResource>> WithFixedSize(int pageSize)
        {
            GaxPreconditions.CheckArgument(pageSize > 0, nameof(pageSize), "Must be greater than 0");
            using (var enumerator = GetEnumerator())
            {
                bool done = false;
                while (!done)
                {
                    var items = new List<TResource>(pageSize);
                    while (items.Count < pageSize)
                    {
                        int requestCount = pageSize - items.Count;
                        done = !enumerator.MoveNext(requestCount);
                        if (done)
                        {
                            break;
                        }
                        items.AddRange(enumerator.Current);
                        if (items.Count > pageSize)
                        {
                            throw new NotSupportedException("Invalid server response: " +
                                $"requested {requestCount} items, received {enumerator.Current.Count()} items");
                        }
                    }
                    if (items.Count != 0)
                    {
                        yield return new FixedSizePage<TResource>(items, enumerator.Current.NextPageToken);
                    }
                }
            }
        }

        private class ResponseEnumerator : IResponseEnumerator<TResponse>
        {
            private readonly CallSettings _callSettings;
            private readonly ApiCall<TRequest, TResponse> _apiCall;
            private readonly TRequest _request; // This is mutated during iteration
            private bool _finished;

            public ResponseEnumerator(CallSettings callSettings,
                TRequest request, ApiCall<TRequest, TResponse> apiCall)
            {
                _callSettings = callSettings;
                _request = request;
                _apiCall = apiCall;
            }

            public TResponse Current { get; private set; }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_finished)
                {
                    return false;
                }
                Current = _apiCall.Sync(_request, _callSettings);
                var nextPageToken = Current.NextPageToken;
                if (nextPageToken == "")
                {
                    _finished = true;
                }
                // Prepare the next request...
                _request.PageToken = nextPageToken;
                return true;
            }

            public bool MoveNext(int pageSize)
            {
                GaxPreconditions.CheckArgument(pageSize > 0, nameof(pageSize), "Must be greater than 0");
                _request.PageSize = pageSize;
                return MoveNext();
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public void Dispose() { }
        }
    }

}
