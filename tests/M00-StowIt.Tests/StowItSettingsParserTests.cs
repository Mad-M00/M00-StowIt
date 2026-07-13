using M00StowIt.Core;
using M00StowIt.Tests.Fakes;
using Xunit;

namespace M00StowIt.Tests;

// Config parsing must never brick the mod: a broken or missing config falls
// back to complete, playable defaults (Alt+X / Alt+Z, 15x15x15 box).
// These tests pin both the happy path and that resilience promise.
public class StowItSettingsParserTests
{
	private const string ValidXml = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<xml>
	<SortButtons>308 120</SortButtons>
	<RestockButtons>308 122</RestockButtons>
	<SortingMode>Category</SortingMode>
	<FallbackCrateName>Misc</FallbackCrateName>
	<SortDistance>7 7 7</SortDistance>
</xml>";

	private static StowItSettings Parse(string xml)
	{
		return StowItSettingsParser.Parse(xml, new RecordingLog());
	}

	[Fact]
	public void A_valid_config_is_fully_parsed()
	{
		StowItSettings settings = Parse(ValidXml);
		Assert.Equal(new[] { 308, 120 }, settings.SortKeyCodes);
		Assert.Equal(new[] { 308, 122 }, settings.RestockKeyCodes);
		Assert.Equal(RoutingMode.Category, settings.Routing);
		Assert.Equal("Misc", settings.FallbackCrateName);
		Assert.Equal(7, settings.SortRadius.X);
	}

	[Fact]
	public void Sort_distance_is_clamped_to_the_supported_range()
	{
		StowItSettings settings = Parse(ValidXml.Replace("7 7 7", "500 -3 7"));
		Assert.Equal(127, settings.SortRadius.X);
		Assert.Equal(0, settings.SortRadius.Y);
		Assert.Equal(7, settings.SortRadius.Z);
	}

	[Fact]
	public void A_missing_required_tag_falls_back_to_complete_defaults()
	{
		StowItSettings settings = Parse(ValidXml.Replace(
			"<SortButtons>308 120</SortButtons>", ""));
		AssertDefaults(settings);
	}

	[Fact]
	public void Malformed_xml_falls_back_to_complete_defaults()
	{
		AssertDefaults(Parse("not xml at all"));
	}

	// Old configs may still carry tags from removed features (slot locking
	// colors and hotkeys); they are simply ignored, never an error.
	[Fact]
	public void Unknown_leftover_tags_are_ignored()
	{
		string xml = ValidXml.Replace("</xml>",
			"<LockSlotButtons>308</LockSlotButtons><LockedSlotsIconColor>255 0 0 255</LockedSlotsIconColor></xml>");
		StowItSettings settings = Parse(xml);
		Assert.Equal(new[] { 308, 120 }, settings.SortKeyCodes);
	}

	[Fact]
	public void Routing_mode_and_fallback_name_are_optional_with_sensible_defaults()
	{
		string xml = ValidXml
			.Replace("<SortingMode>Category</SortingMode>", "")
			.Replace("<FallbackCrateName>Misc</FallbackCrateName>", "");
		StowItSettings settings = Parse(xml);
		Assert.Equal(RoutingMode.Category, settings.Routing);
		Assert.Equal("Misc", settings.FallbackCrateName);
		// ...while the rest of the file still applies:
		Assert.Equal(new[] { 308, 120 }, settings.SortKeyCodes);
	}

	[Fact]
	public void Vanilla_routing_mode_can_be_selected()
	{
		StowItSettings settings = Parse(ValidXml.Replace(">Category<", ">vanilla<"));
		Assert.Equal(RoutingMode.Vanilla, settings.Routing);
	}

	private static void AssertDefaults(StowItSettings settings)
	{
		StowItSettings defaults = StowItSettings.Defaults();
		Assert.Equal(defaults.SortKeyCodes, settings.SortKeyCodes);
		Assert.Equal(defaults.RestockKeyCodes, settings.RestockKeyCodes);
		Assert.Equal(defaults.Routing, settings.Routing);
		Assert.Equal(defaults.FallbackCrateName, settings.FallbackCrateName);
		Assert.Equal(defaults.SortRadius.X, settings.SortRadius.X);
	}
}
