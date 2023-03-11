﻿namespace Xunit.DependencyInjection;

public class DependencyInjectionTestCaseRunner : XunitTestCaseRunner
{
    private readonly IServiceProvider _provider;

    public DependencyInjectionTestCaseRunner(IServiceProvider provider, IXunitTestCase testCase,
        string displayName, string skipReason, object?[] constructorArguments, object[] testMethodArguments,
        IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        : base(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus,
            aggregator, cancellationTokenSource) => _provider = provider;

    /// <inheritdoc />
    protected override Task<RunSummary> RunTestAsync() =>
        new DependencyInjectionTestRunner(_provider, new XunitTest(TestCase, DisplayName), MessageBus,
                FromServicesAttribute.CreateFromServices(TestMethod),
                TestClass, ConstructorArguments, TestMethod, TestMethodArguments, SkipReason,
                BeforeAfterAttributes, new(Aggregator), CancellationTokenSource)
            .RunAsync();
}
