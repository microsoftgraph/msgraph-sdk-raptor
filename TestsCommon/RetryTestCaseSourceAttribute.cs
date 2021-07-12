using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace TestsCommon
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class RetryTestCaseSourceAttribute : TestCaseSourceAttribute, IRepeatTest
    {
        #region constructors
        public RetryTestCaseSourceAttribute(string sourceName) : base(sourceName){}
        public RetryTestCaseSourceAttribute(Type sourceType) : base(sourceType){}
        public RetryTestCaseSourceAttribute(Type sourceType, string sourceName) : base(sourceType, sourceName){}
        public RetryTestCaseSourceAttribute(string sourceName, object[] methodParams) : base(sourceName, methodParams){}
        public RetryTestCaseSourceAttribute(Type sourceType, string sourceName, object[] methodParams) : base(sourceType, sourceName, methodParams){}
        #endregion

        #region repeat components
        public int MaxTries { get; set; }
        TestCommand ICommandWrapper.Wrap(TestCommand command) => new RetryAttribute.RetryCommand(command, MaxTries);
        #endregion
    }
}
