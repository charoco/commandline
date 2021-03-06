#region License
//
// Command Line Library: CommandLine.cs
//
// Author:
//   Giacomo Stelluti Scala (gsscoder@gmail.com)
//
// Copyright (C) 2005 - 2012 Giacomo Stelluti Scala
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
#endregion

namespace CommandLine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    internal abstract class ArgumentParser
    {
        protected ArgumentParser()
        {
            this.PostParsingState = new List<ParsingError>();
        }

        public static readonly IEnumerable<string> ValidSwitches = new[]{"--","-","/"};

        public abstract ParserState Parse(IArgumentEnumerator argumentEnumerator, OptionMap map, object options);

        public virtual List<ParsingError> PostParsingState { get; protected set; }

        protected void DefineOptionThatViolatesFormat(OptionInfo option)
        {
            //this.PostParsingState.BadOptionInfo = option;
            //this.PostParsingState.ViolatesFormat = true;
            this.PostParsingState.Add(new ParsingError(option.ShortName, option.LongName, true));
        }

        public static ArgumentParser Create(string argument)
        {
            if (ValidSwitches.Contains(argument))
            {
                return null;
            }

            foreach(var validSwitch in ValidSwitches)
            {
                if(argument.StartsWith(validSwitch))
                {
                    return new LongOrOptionGroupParser(validSwitch.Length);
                }
            }

            return null;
        }

        public static bool IsInputValue(string argument)
        {
            if (argument.Length > 0)
                return argument.Equals("-", StringComparison.InvariantCulture) || !StartsWithValidSwitch(argument);

            return true;
        }

        private static bool StartsWithValidSwitch(string argument)
        {
            return ValidSwitches.Any(argument.StartsWith);
        }

#if DEBUG
        public static IList<string> PublicWrapperOfGetNextInputValues(IArgumentEnumerator ae)
        {
            return GetNextInputValues(ae);
        }
#endif

        protected static IList<string> GetNextInputValues(IArgumentEnumerator ae)
        {
            IList<string> list = new List<string>();

            while (ae.MoveNext())
            {
                if (IsInputValue(ae.Current))
                    list.Add(ae.Current);
                else
                    break;
            }
            if (!ae.MovePrevious())
                throw new CommandLineParserException();

            return list;
        }

        public static bool CompareShort(string argument, string option, bool caseSensitive)
        {
            return string.Compare(argument, "-" + option, !caseSensitive) == 0;
        }

        public static bool CompareLong(string argument, string option, bool caseSensitive)
        {
            return string.Compare(argument, "--" + option, !caseSensitive) == 0;
        }

        protected static ParserState BooleanToParserState(bool value)
        {
            return BooleanToParserState(value, false);
        }

        protected static ParserState BooleanToParserState(bool value, bool addMoveNextIfTrue)
        {
            if (value && !addMoveNextIfTrue)
                return ParserState.Success;
            else if (value && addMoveNextIfTrue)
                return ParserState.Success | ParserState.MoveOnNextElement;
            else
                return ParserState.Failure;
        }

        protected static void EnsureOptionAttributeIsArrayCompatible(OptionInfo option)
        {
            if (!option.IsAttributeArrayCompatible)
                throw new CommandLineParserException();
        }

        protected static void EnsureOptionArrayAttributeIsNotBoundToScalar(OptionInfo option)
        {
            if (!option.IsArray && option.IsAttributeArrayCompatible)
                throw new CommandLineParserException();
        }
    }

    internal class LongOrOptionGroupParser : ArgumentParser
    {
        private readonly int switchLength;
        private ArgumentParser innerParser;

        public LongOrOptionGroupParser(int switchLength)
        {
            this.switchLength = switchLength;
        }

        public override ParserState Parse(IArgumentEnumerator argumentEnumerator, OptionMap map, object options)
        {
            innerParser = new OptionGroupParser(switchLength);

            var parts = argumentEnumerator.Current.Substring(switchLength).Split(new char[] { '=' }, 2);
            if(parts[0].Length > 1)
            {
                var option = map[parts[0]];
                if (option != null) innerParser = new LongOptionParser(switchLength);
            }
            
            return innerParser.Parse(argumentEnumerator, map, options);
        }

        public override List<ParsingError> PostParsingState
        {
            get
            {
                return innerParser != null ? innerParser.PostParsingState : base.PostParsingState;
            }
            protected set
            {
                base.PostParsingState = value;
            }
        }
    }
}