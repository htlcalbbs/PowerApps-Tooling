// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.PowerPlatform.PowerApps.Persistence.MsApp;
using Persistence.Tests.Extensions;

namespace Persistence.Tests.MsApp;

[TestClass]
public class MsappArchiveSaveTests : TestBase
{
    [TestMethod]
    [DataRow(@"  Hello   ", $"src/{MsappArchive.Directories.Controls}/Hello.fx.yaml", @"_TestData/ValidYaml/Screen-Hello1.fx.yaml")]
    [DataRow(@"..\..\Hello", $"src/{MsappArchive.Directories.Controls}/Hello.fx.yaml", @"_TestData/ValidYaml/Screen-Hello2.fx.yaml")]
    [DataRow(@"c:\win\..\..\Hello", $"src/{MsappArchive.Directories.Controls}/cWinHello.fx.yaml", @"_TestData/ValidYaml/Screen-Hello3.fx.yaml")]
    [DataRow(@"//..?HelloScreen", $"src/{MsappArchive.Directories.Controls}/HelloScreen.fx.yaml", @"_TestData/ValidYaml/Screen-Hello4.fx.yaml")]
    [DataRow(@"Hello Space", $"src/{MsappArchive.Directories.Controls}/Hello Space.fx.yaml", @"_TestData/ValidYaml/Screen-Hello5.fx.yaml")]
    public void Msapp_ShouldSave_Screen(string screenName, string screenEntryName, string expectedYamlPath)
    {
        // Arrange
        var tempFile = Path.Combine(TestContext.DeploymentDirectory!, Path.GetRandomFileName());
        using var msappArchive = MsappArchiveFactory.Create(tempFile);

        msappArchive.App.Should().BeNull();

        // Act
        var screen = ControlFactory.CreateScreen(screenName);
        msappArchive.Save(screen);
        msappArchive.Dispose();

        // Assert
        using var msappValidation = MsappArchiveFactory.Open(tempFile);
        msappValidation.App.Should().BeNull();
        msappValidation.CanonicalEntries.Count.Should().Be(2);
        var screenEntry = msappValidation.CanonicalEntries[MsappArchive.NormalizePath(screenEntryName)];
        screenEntry.Should().NotBeNull();
        using var streamReader = new StreamReader(msappValidation.GetRequiredEntry(screenEntryName).Open());
        var yaml = streamReader.ReadToEnd().NormalizeNewlines();
        var expectedYaml = File.ReadAllText(expectedYamlPath).NormalizeNewlines();
        yaml.Should().Be(expectedYaml);
    }

    [TestMethod]
    [DataRow(@"  Hello   ", "My control",
        $"src/{MsappArchive.Directories.Controls}/Hello.fx.yaml",
        $"{MsappArchive.Directories.Controls}/Hello.json",
        @"_TestData/ValidYaml/Screen-with-control1.fx.yaml",
        @"_TestData/ValidYaml/Screen-with-control1.json")]
    public void Msapp_ShouldSave_Screen_With_Control(string screenName, string controlName, string screenEntryName, string editorStateName,
        string expectedYamlPath, string expectedJsonPath)
    {
        // Arrange
        var tempFile = Path.Combine(TestContext.DeploymentDirectory!, Path.GetRandomFileName());
        using var msappArchive = MsappArchiveFactory.Create(tempFile);

        msappArchive.App.Should().BeNull();

        // Act
        var screen = ControlFactory.CreateScreen(screenName,
            controls: new[] {
                ControlFactory.Create(controlName, "ButtonCanvas")
            });
        msappArchive.Save(screen);
        msappArchive.Dispose();

        // Assert
        using var msappValidation = MsappArchiveFactory.Open(tempFile);
        msappValidation.App.Should().BeNull();
        msappValidation.CanonicalEntries.Count.Should().Be(2);

        // Validate screen
        var screenEntry = msappValidation.CanonicalEntries[MsappArchive.NormalizePath(screenEntryName)];
        screenEntry.Should().NotBeNull();
        using var streamReader = new StreamReader(msappValidation.GetRequiredEntry(screenEntryName).Open());
        var yaml = streamReader.ReadToEnd().NormalizeNewlines();
        var expectedYaml = File.ReadAllText(expectedYamlPath).NormalizeNewlines();
        yaml.Should().Be(expectedYaml);

        // Validate editor state
        var editorStateEntry = msappValidation.CanonicalEntries[MsappArchive.NormalizePath(editorStateName)];
        editorStateEntry.Should().NotBeNull();
        using var editorStateReader = new StreamReader(msappValidation.GetRequiredEntry(editorStateName).Open());
        var json = editorStateReader.ReadToEnd().ReplaceLineEndings();
        var expectedJson = File.ReadAllText(expectedJsonPath).ReplaceLineEndings().TrimEnd();
        json.Should().Be(expectedJson);
    }


    [TestMethod]
    [DataRow("HelloWorld", "HelloScreen")]
    public void Msapp_ShouldSave_App(string appName, string screenName)
    {
        // Arrange
        var tempFile = Path.Combine(TestContext.DeploymentDirectory!, Path.GetRandomFileName());
        using (var msappArchive = MsappArchiveFactory.Create(tempFile))
        {
            msappArchive.App.Should().BeNull();

            // Act
            var app = ControlFactory.CreateApp(appName);
            app.Screens.Add(ControlFactory.CreateScreen(screenName));
            msappArchive.App = app;

            msappArchive.Save();
        }

        // Assert
        using var msappValidation = MsappArchiveFactory.Open(tempFile);
        msappValidation.App.Should().NotBeNull();
        msappValidation.App!.Screens.Count.Should().Be(1);
        msappValidation.App.Screens.Single().Name.Should().Be(screenName);
        msappValidation.App.Name.Should().Be(appName);
        msappValidation.CanonicalEntries.Keys.Should().Contain(MsappArchive.NormalizePath(MsappArchive.HeaderFileName));
    }
}