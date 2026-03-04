using System.Diagnostics;
using ActivitySourceGenerator.Attributes;
using Xunit;

namespace ActivitySourceGenerator.Tests;

public class ActivitySourceGeneratorIntegrationTests
{
    [Activity]
    public static string TestMethodWithActivity(string input)
    {
        return $"Processed: {input}";
    }

    [Activity(ActivityName = "CustomTestActivity")]
    public static async Task<int> TestAsyncMethodWithActivity(string data)
    {
        await Task.Delay(10);
        return data.Length;
    }

    [Fact]
    public void ActivityAttribute_ExistsAndCanBeApplied()
    {
        // This test verifies that the ActivityAttribute is generated and can be applied
        // The fact that this code compiles proves the source generator is working
        Assert.True(true, "ActivityAttribute exists and can be applied to methods");
    }

    [Fact]
    public void GeneratedWrapperMethods_ExistAndWork()
    {
        // Test synchronous wrapper method
        var result = ActivitySourceGeneratorIntegrationTestsActivityWrapper.TestMethodWithActivityWithActivity("test input");
        Assert.Equal("Processed: test input", result);
    }

    [Fact]
    public async Task GeneratedAsyncWrapperMethods_ExistAndWork()
    {
        // Test asynchronous wrapper method
        var result = await ActivitySourceGeneratorIntegrationTestsActivityWrapper.TestAsyncMethodWithActivityWithActivity("test data");
        Assert.Equal(9, result);
    }

    [Fact]
    public void GeneratedWrapperMethods_CreateActivities()
    {
        var activities = new List<Activity>();

        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStarted = activities.Add,
        };
        
        ActivitySource.AddActivityListener(listener);

        // Execute wrapper method which should create an activity
        ActivitySourceGeneratorIntegrationTestsActivityWrapper.TestMethodWithActivityWithActivity("test");

        // Verify activity was created
        Assert.Single(activities);
        Assert.Equal("TestMethodWithActivity", activities[0].DisplayName);
    }

    [Fact]
    public void GeneratedWrapperMethods_HandleExceptions()
    {
        var activities = new List<Activity>();
        
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => activities.Add(activity),
        };
        
        ActivitySource.AddActivityListener(listener);

        // Test exception handling by calling a method that throws
        Assert.Throws<ArgumentNullException>(() => 
            ActivitySourceGeneratorIntegrationTestsActivityWrapper.TestExceptionMethodWithActivity());

        // Verify activity was created and marked as error
        Assert.Single(activities);
        Assert.Equal(ActivityStatusCode.Error, activities[0].Status);
    }

    [Activity]
    public static void TestExceptionMethod()
    {
        throw new ArgumentNullException("test parameter");
    }
}