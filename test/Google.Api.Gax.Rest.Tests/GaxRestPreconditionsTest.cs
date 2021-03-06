/*
 * Copyright 2016 Google Inc. All Rights Reserved.
 * Use of this source code is governed by a BSD-style
 * license that can be found in the LICENSE file or at
 * https://developers.google.com/open-source/licenses/bsd
 */

using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Google.Api.Gax.Rest.Tests
{
    public class GaxRestPreconditionsTest
    {
        const int RangeMin = -4;
        const int RangeMax = 5;

        [Fact]
        public void CheckNotNull_Valid()
        {
            object x = new object();
            Assert.Same(x, GaxRestPreconditions.CheckNotNull(x, nameof(x)));
        }

        [Fact]
        public void CheckNotNull_Invalid()
        {
            object x = null;
            var exception = Assert.Throws<ArgumentNullException>(() => GaxRestPreconditions.CheckNotNull(x, nameof(x)));
            Assert.Equal(nameof(x), exception.ParamName);
        }

        [Fact]
        public void CheckNotNullOrEmpty_Valid()
        {
            string x = "foo";
            Assert.Same(x, GaxRestPreconditions.CheckNotNullOrEmpty(x, nameof(x)));
        }

        [Fact]
        public void CheckNotNull_Null()
        {
            string x = null;
            var exception = Assert.Throws<ArgumentNullException>(() => GaxRestPreconditions.CheckNotNullOrEmpty(x, nameof(x)));
            Assert.Equal(nameof(x), exception.ParamName);
        }

        [Fact]
        public void CheckNotNull_Empty()
        {
            string x = "";
            var exception = Assert.Throws<ArgumentException>(() => GaxRestPreconditions.CheckNotNullOrEmpty(x, nameof(x)));
            Assert.Equal(nameof(x), exception.ParamName);
        }

        [Theory]
        [InlineData(RangeMin)]
        [InlineData((RangeMin + RangeMax) / 2)]
        [InlineData(RangeMax)]
        public void CheckRange_Valid(int value)
        {
            Assert.Equal(value, GaxRestPreconditions.CheckArgumentRange(value, nameof(value), -4, 5));
        }

        [Theory]
        [InlineData(RangeMin - 1)]
        [InlineData(RangeMax + 1)]
        public void CheckRange_Invalid(int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GaxRestPreconditions.CheckArgumentRange(value, nameof(value), -4, 5));
        }

        [Fact]
        public void CheckState_Valid()
        {
            GaxRestPreconditions.CheckState(true, "Not used");
        }

        [Fact]
        public void CheckState_Invalid()
        {
            string message = "Exception message";
            var exception = Assert.Throws<InvalidOperationException>(() => GaxRestPreconditions.CheckState(false, message));
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void CheckArgument_Valid()
        {
            GaxRestPreconditions.CheckArgument(true, "irrelevantParameterName", "Irrelevant message");
            GaxRestPreconditions.CheckArgument(true, "irrelevantParameterName", "Irrelevant message", "arg0");
            GaxRestPreconditions.CheckArgument(true, "irrelevantParameterName", "Irrelevant message", "arg0", "arg1");
        }

        [Fact]
        public void CheckArgument_Invalid()
        {
            var parameterName = "parameterName";
            var message = "Message";
            var exception = Assert.Throws<ArgumentException>(() => GaxRestPreconditions.CheckArgument(false, parameterName, message));
            // Note: Not Assert.Equal here, as the ArgumentException constructor magically appends "Parameter name: ..."
            // into the Message property :(
            Assert.StartsWith(message, exception.Message);
            Assert.Equal(parameterName, exception.ParamName);
        }

        [Fact]
        public void CheckArgument_Invalid1FormatArgument()
        {
            var parameterName = "parameterName";
            var exception = Assert.Throws<ArgumentException>(() => GaxRestPreconditions.CheckArgument(false, parameterName, "Foo {0}", 1));
            Assert.StartsWith("Foo 1", exception.Message);
            Assert.Equal(parameterName, exception.ParamName);
        }

        [Fact]
        public void CheckArgument_Invalid2FormatArguments()
        {
            var parameterName = "parameterName";
            var exception = Assert.Throws<ArgumentException>(() => GaxRestPreconditions.CheckArgument(false, parameterName, "Foo {0} {1}", 1, 2));
            Assert.StartsWith("Foo 1 2", exception.Message);
            Assert.Equal(parameterName, exception.ParamName);
        }

        [Fact]
        public void CheckEnumValue_NotDefined()
        {
            var parameterName = "parameterName";
            var exception = Assert.Throws<ArgumentException>(() => GaxRestPreconditions.CheckEnumValue((SampleEnum) 5, nameof(parameterName)));
            Assert.Equal(parameterName, exception.ParamName);
        }

        [Fact]
        public void CheckEnumValue_Defined()
        {
            var parameterName = "parameterName";
            var value = SampleEnum.DefinedValue;
            Assert.Equal(value, GaxRestPreconditions.CheckEnumValue(value, nameof(parameterName)));
        }

        enum SampleEnum
        {
            DefinedValue = 1
        }
    }
}