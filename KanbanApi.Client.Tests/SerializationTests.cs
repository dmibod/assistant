namespace KanbanApi.Client.Tests;

using System.Text.Json;
using System.Text.Json.Serialization;
using KanbanApi.Client.Serialization;

[TestClass]
public class SerializationTests
{
    [DataTestMethod]
    [DataRow(LayoutTypes.H, "H")]
    [DataRow(LayoutTypes.V, "V")]
    public void LayoutType_Serialize_ExpectCorrectValue(LayoutTypes layout, string expectedValue)
    {
        // Arrange
        var model = new LayoutAware
        {
            Layout = layout
        };

        var expectedJson = "{\"layout\":\"{placeholder}\"}".Replace("{placeholder}", expectedValue);

        // Act
        var actualJson = JsonSerializer.Serialize(model, SerializationDefaults.Options);

        // Assert
        Assert.AreEqual(expectedJson, actualJson);
    }
    
    [DataTestMethod]
    [DataRow(LaneTypes.C, "C")]
    [DataRow(LaneTypes.L, "L")]
    public void LaneType_Serialize_ExpectCorrectValue(LaneTypes type, string expectedValue)
    {
        // Arrange
        var model = new LaneTypeAware
        {
            Type = type
        };

        var expectedJson = "{\"type\":\"{placeholder}\"}".Replace("{placeholder}", expectedValue);

        // Act
        var actualJson = JsonSerializer.Serialize(model, SerializationDefaults.Options);

        // Assert
        Assert.AreEqual(expectedJson, actualJson);
    }
}

internal class LayoutAware
{
    [JsonPropertyName("layout")] public LayoutTypes Layout { get; set; }
}

internal class LaneTypeAware
{
    [JsonPropertyName("type")] public LaneTypes Type { get; set; }
}