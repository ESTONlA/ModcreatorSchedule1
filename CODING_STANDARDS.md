# Coding Standards

This document outlines the coding standards and conventions for the Schedule1 Modding Tool (ModcreatorSchedule1).
These standards ensure consistency, maintainability, and predictability across the codebase.

## General Best Practice

* Review the existing codebase thoroughly before making changes or additions.
* Follow the MVVM (Model-View-ViewModel) pattern consistently throughout the application.
* Keep UI logic in code-behind minimal - prefer ViewModel properties and commands where possible.
* Write clean, self-documenting code with meaningful names.

## File and Namespace Structure

* All classes must exist in a logical namespace matching the folder structure.
* Root namespace: `Schedule1ModdingTool`
* Subnamespaces follow the directory structure:
  * `Schedule1ModdingTool.Models` - Data models and blueprints
  * `Schedule1ModdingTool.ViewModels` - MVVM ViewModels
  * `Schedule1ModdingTool.Views` - WPF Views (UserControls, Windows)
  * `Schedule1ModdingTool.Views.Controls` - Reusable custom controls
  * `Schedule1ModdingTool.Services` - Business logic services
  * `Schedule1ModdingTool.Services.CodeGeneration` - Code generation pipeline
  * `Schedule1ModdingTool.Utils` - Utility classes and helpers
  * `Schedule1ModdingTool.Data` - Data access and presets

```csharp
namespace Schedule1ModdingTool.Services.CodeGeneration.Npc
{
    public class NpcCodeGenerator : ICodeGenerator<NpcBlueprint>
    {
        // ...
    }
}
```

## Naming Conventions

* **PascalCase** for class names, methods, properties, events, and public/protected fields.
* **camelCase** for local variables, method parameters, and private fields.
* Prefix private fields with `_`.

```csharp
public class NpcBlueprint : ObservableObject
{
    private string _className = string.Empty;
    private bool _isPhysical = true;

    public string ClassName
    {
        get => _className;
        set => SetProperty(ref _className, value);
    }
}
```

* Event handlers follow the pattern: `{ElementName}_{EventName}` (e.g., `AddScheduleAction_Click`, `EnableCustomer_Checked`).
* XAML element names use PascalCase without underscores (e.g., `ScheduleActionsListBox`, `ConnectionIdTextBox`).
* Dependency properties follow WPF conventions with `Property` suffix (e.g., `XProperty`, `TimeProperty`).

```csharp
public static readonly DependencyProperty TimeProperty =
    DependencyProperty.Register(nameof(Time), typeof(int), typeof(TimeInput),
        new FrameworkPropertyMetadata(900, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnTimeChanged));
```

* Enums use PascalCase and do not need prefixes (e.g., `ScheduleActionType`, not `EScheduleActionType`).
* Use enums over string constants where possible for type safety.

```csharp
public enum ScheduleActionType
{
    WalkTo,
    StayInBuilding,
    LocationDialogue,
    UseVendingMachine
}
```

* Interface names start with `I` (e.g., `ICodeGenerator<T>`, `ICodeBuilder`).
* Abstract base classes may use `Base` suffix if helpful for clarity (e.g., `ObservableObject`).

## Access Modifiers

* Always explicitly specify access modifiers - do not rely on defaults.
* Use `public` for types and members that form the public API.
* Use `internal` for implementation details not needed outside the assembly.
* Use `private` for internal class implementation details.
* Use `protected` for members intended for inheritance (e.g., `ObservableObject.SetProperty`).

```csharp
public class NpcBlueprint : ObservableObject
{
    private string _npcId = string.Empty;

    [JsonProperty("npcId")]
    public string NpcId
    {
        get => _npcId;
        set => SetProperty(ref _npcId, value);
    }

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
    }
}
```

* Use `sealed` on classes when inheritance is not intended or needed.

```csharp
public sealed class CodeFormatter
{
    // Static utility class - seal to prevent inheritance
}
```

* Arrow-bodied members (`=>`) for simple properties and methods:

```csharp
// Simple property
public string FullName => $"{FirstName} {LastName}";

// Simple method
public string GetSafeClassName() =>
    IdentifierSanitizer.MakeSafeIdentifier(ClassName, "GeneratedNpc");
```

* Multi-line arrow-bodied members should have the arrow on the same line and body indented:

```csharp
public bool HasValidSchedule =>
    ScheduleActions != null &&
    ScheduleActions.Count > 0 &&
    ScheduleActions.All(a => a.StartTime >= 0);
```

* Use `readonly` for fields that are only assigned in constructors or initialization.

```csharp
private readonly NpcHeaderGenerator _headerGenerator;
private readonly NpcAppearanceGenerator _appearanceGenerator;
```

* Nullable reference types should be declared with `?`:

```csharp
public NpcBlueprint? SelectedNpc { get; set; }
private MainViewModel? ViewModel => DataContext as MainViewModel;
```

## MVVM Pattern

### Models

* Models represent data structures and should inherit from `ObservableObject` for property change notifications.
* Use the `SetProperty` helper method for property setters to automatically raise `PropertyChanged`.
* Decorate properties with `[JsonProperty("propertyName")]` for consistent JSON serialization.
* Initialize collections in property initializers, not constructors:

```csharp
public class NpcBlueprint : ObservableObject
{
    private string _npcId = string.Empty;

    [JsonProperty("npcId")]
    public string NpcId
    {
        get => _npcId;
        set => SetProperty(ref _npcId, value);
    }

    [JsonProperty("scheduleActions")]
    public ObservableCollection<NpcScheduleAction> ScheduleActions { get; } = new();
}
```

* Provide sensible defaults for all properties.
* Models should be purely data-focused with minimal business logic.

### ViewModels

* ViewModels manage application state and expose data/commands to Views.
* Implement `INotifyPropertyChanged` (via `ObservableObject` base class).
* Use `RelayCommand` or similar for command binding.
* ViewModels should never directly reference UI elements or WPF types (except for dialogs).

```csharp
public class MainViewModel : ObservableObject
{
    private NpcBlueprint? _selectedNpc;

    public NpcBlueprint? SelectedNpc
    {
        get => _selectedNpc;
        set => SetProperty(ref _selectedNpc, value);
    }

    public ICommand SaveCommand { get; }

    public MainViewModel()
    {
        SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
    }
}
```

### Views

* Views are XAML files with minimal code-behind.
* Code-behind should only contain:
  * Event handlers that cannot be easily implemented via commands
  * Visual tree manipulation
  * Dialog invocation
  * Direct UI state management
* Prefer data binding over imperative code.
* Use `x:Name` for elements that need to be accessed from code-behind.

```xml
<TextBox x:Name="ConnectionIdTextBox"
         Text="{Binding ConnectionText, UpdateSourceTrigger=PropertyChanged}"
         materialDesign:HintAssist.Hint="NPC ID" />
```

### Custom Controls

* Custom controls should be reusable and self-contained.
* Use Dependency Properties for all bindable properties.
* Follow WPF conventions for Dependency Property registration:

```csharp
public partial class Vector3Input : UserControl
{
    public static readonly DependencyProperty XProperty =
        DependencyProperty.Register(
            nameof(X),
            typeof(float),
            typeof(Vector3Input),
            new FrameworkPropertyMetadata(
                0f,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public float X
    {
        get => (float)GetValue(XProperty);
        set => SetValue(XProperty, value);
    }
}
```

* Include validation logic in property changed callbacks when appropriate.
* Document custom controls with XML comments explaining purpose and usage.

## Code Generation Services

### Architecture

* Code generators implement `ICodeGenerator<T>` interface.
* Use `ICodeBuilder` for all code output - never use string concatenation directly.
* Break complex generation into smaller, focused methods.
* Use composition - inject or instantiate specialized generators for different aspects.

```csharp
public class NpcCodeGenerator : ICodeGenerator<NpcBlueprint>
{
    private readonly NpcHeaderGenerator _headerGenerator;
    private readonly NpcAppearanceGenerator _appearanceGenerator;
    private readonly NpcScheduleGenerator _scheduleGenerator;

    public NpcCodeGenerator()
    {
        _headerGenerator = new NpcHeaderGenerator();
        _appearanceGenerator = new NpcAppearanceGenerator();
        _scheduleGenerator = new NpcScheduleGenerator();
    }

    public string GenerateCode(NpcBlueprint npc)
    {
        var builder = new CodeBuilder();

        _headerGenerator.Generate(builder, npc);
        _appearanceGenerator.Generate(builder, npc.Appearance);
        _scheduleGenerator.Generate(builder, npc);

        return builder.Build();
    }
}
```

### ICodeBuilder Usage

* Always use `OpenBlock(string)` and `CloseBlock()` for proper indentation management.
* Use `AppendLine(string)` for single lines of code.
* Use `AppendBlockComment(params string[])` for multi-line XML comments.
* Never manually manage indentation - let `ICodeBuilder` handle it.

```csharp
builder.OpenBlock("public class MyClass");
builder.AppendLine("public string Name { get; set; }");
builder.AppendLine();
builder.OpenBlock("public void DoSomething()");
builder.AppendLine("Console.WriteLine(Name);");
builder.CloseBlock(); // method
builder.CloseBlock(); // class
```

### Code Generation Best Practices

* Always sanitize user input before generating code:
  * Use `IdentifierSanitizer.MakeSafeIdentifier()` for class/variable names
  * Use `CodeFormatter.EscapeString()` for string literals
  * Use `NamespaceNormalizer.NormalizeForNpc()` for namespaces

```csharp
var className = IdentifierSanitizer.MakeSafeIdentifier(npc.ClassName, "GeneratedNpc");
builder.AppendLine($"builder.WithIdentity(\"{CodeFormatter.EscapeString(npc.NpcId)}\")");
```

* Generate code conditionally based on blueprint configuration:

```csharp
if (npc.EnableCustomer)
{
    builder.AppendLine(".EnsureCustomer()");
    GenerateCustomerDefaults(builder, npc.CustomerDefaults);
}
```

* Use fluent builder patterns that match the S1API style.
* Format generated code for readability with proper indentation and line breaks.

## XAML Standards

### General XAML Style

* Use Material Design in XAML (MaterialDesignThemes) for consistent UI.
* Organize XAML attributes in this order:
  1. `x:Name`
  2. Attached properties (Grid.Row, Grid.Column, DockPanel.Dock, etc.)
  3. Layout properties (Width, Height, Margin, Padding, HorizontalAlignment, VerticalAlignment)
  4. Content/Data properties (Text, Content, ItemsSource, SelectedItem)
  5. Binding expressions
  6. Style and visual properties
  7. Event handlers

```xml
<TextBox x:Name="FirstNameTextBox"
         Grid.Row="0"
         Grid.Column="1"
         Margin="8"
         Text="{Binding SelectedNpc.FirstName, UpdateSourceTrigger=PropertyChanged}"
         materialDesign:HintAssist.Hint="First Name"
         Style="{StaticResource MaterialDesignOutlinedTextBox}" />
```

* Use `UpdateSourceTrigger=PropertyChanged` for immediate binding updates.
* Always use two-way binding explicitly when needed: `Mode=TwoWay`.
* Use value converters for complex binding transformations.

### Resource Management

* Define reusable styles and templates in ResourceDictionaries.
* Use `StaticResource` for styles and templates (faster than `DynamicResource`).
* Use `DynamicResource` only for theme-dependent resources.
* Group related resources logically.

```xml
<UserControl.Resources>
    <BooleanToVisibilityConverter x:Key="BoolToVisibilityConverter"/>

    <Style x:Key="SectionHeaderStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="16"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Margin" Value="0,16,0,8"/>
    </Style>
</UserControl.Resources>
```

### Layout

* Prefer `Grid` for complex layouts, `StackPanel` for simple vertical/horizontal stacking.
* Use `ScrollViewer` for content that may overflow.
* Use `TabControl` with Material Design styles for multi-section interfaces.
* Maintain consistent spacing using Margin (typically multiples of 4 or 8).

```xml
<TabControl Style="{StaticResource MaterialDesignNavigationRailTabControl}">
    <TabItem Header="Identity &amp; Setup">
        <ScrollViewer VerticalScrollBarVisibility="Auto">
            <Grid Margin="16">
                <!-- Content -->
            </Grid>
        </ScrollViewer>
    </TabItem>
</TabControl>
```

### Binding Conventions

* Always specify `Path` explicitly in complex scenarios.
* Use `RelativeSource` when binding to ancestor elements.
* Use `ElementName` binding for cross-element references within the same view.

```xml
<!-- Binding to ViewModel -->
<TextBox Text="{Binding SelectedNpc.FirstName, UpdateSourceTrigger=PropertyChanged}"/>

<!-- ElementName binding -->
<TextBlock Text="{Binding Text, ElementName=FirstNameTextBox}"/>

<!-- Visibility binding -->
<TabItem Visibility="{Binding SelectedNpc.EnableCustomer,
                              Converter={StaticResource BoolToVisibilityConverter}}">
```

## Documentation

* All public classes, methods, and properties must have XML documentation summaries.
* Include `<param>` tags for all method parameters.
* Include `<returns>` tags for methods that return values.
* Include `<exception>` tags for exceptions that may be thrown.

```csharp
/// <summary>
/// Generates complete C# source code for an NPC blueprint.
/// </summary>
/// <param name="npc">The NPC blueprint to generate code from.</param>
/// <returns>Generated C# source code as a string.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="npc"/> is null.</exception>
public string GenerateCode(NpcBlueprint npc)
{
    if (npc == null)
        throw new ArgumentNullException(nameof(npc));

    // ...
}
```

* Use inline comments for complex logic that may not be immediately obvious.
* Avoid obvious comments that just restate what the code does.

```csharp
// Good: Explains WHY
// Validate minutes component doesn't exceed 59 (e.g., 1365 is invalid time)
if (minutes > 59)
    control.Time = (time / 100) * 100 + 59;

// Bad: Restates WHAT the code does
// Set the time property
control.Time = newTime;
```

## Error Handling

* Validate user input early and provide clear error messages.
* Use exceptions for exceptional circumstances, not control flow.
* Catch specific exceptions rather than general `Exception` where possible.
* Log errors appropriately (if logging infrastructure is added).

```csharp
public string GenerateCode(NpcBlueprint npc)
{
    if (npc == null)
        throw new ArgumentNullException(nameof(npc));

    if (string.IsNullOrWhiteSpace(npc.ClassName))
    {
        // Provide default instead of throwing
        npc.ClassName = "GeneratedNpc";
    }

    // ...
}
```

* For WPF applications, catch exceptions at appropriate boundaries (command handlers, event handlers).

```csharp
private void SaveProject_Click(object sender, RoutedEventArgs e)
{
    try
    {
        _projectService.SaveProject(ViewModel.CurrentProject);
        MessageBox.Show("Project saved successfully!", "Success",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
    catch (IOException ex)
    {
        MessageBox.Show($"Failed to save project: {ex.Message}", "Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

## Testing Considerations

* Write code that is testable - avoid tight coupling.
* Use dependency injection where appropriate.
* Keep business logic separate from UI logic.
* Consider creating interfaces for services to enable mocking.

## Code Organization

* Organize class members in this order:
  1. Static fields and constants
  2. Instance fields (private/internal)
  3. Constructors
  4. Properties (public, then protected, then private)
  5. Events
  6. Public methods
  7. Protected methods
  8. Private methods
  9. Event handlers (at the end)

```csharp
public class NpcCodeGenerator : ICodeGenerator<NpcBlueprint>
{
    // Static members
    private static readonly string DefaultNamespace = "Schedule1.Generated.NPCs";

    // Instance fields
    private readonly NpcHeaderGenerator _headerGenerator;
    private readonly NpcAppearanceGenerator _appearanceGenerator;

    // Constructor
    public NpcCodeGenerator()
    {
        _headerGenerator = new NpcHeaderGenerator();
        _appearanceGenerator = new NpcAppearanceGenerator();
    }

    // Public methods
    public string GenerateCode(NpcBlueprint npc)
    {
        // ...
    }

    // Private helper methods
    private void GenerateNpcClass(ICodeBuilder builder, NpcBlueprint npc)
    {
        // ...
    }
}
```

* Use regions sparingly - only for very long classes where logical grouping adds clarity.
* Prefer splitting large classes into smaller, focused classes over using regions.

## What NOT to Do

* Do not mix business logic with UI logic.
* Do not use magic strings - use constants or enums.
* Do not ignore exceptions silently.
* Do not create "God classes" that do everything - maintain single responsibility.
* Do not use `var` when the type is not obvious from the right side of the assignment.
* Do not create public properties for internal state that should remain encapsulated.
* Do not use `Dispatcher.Invoke` in ViewModels - keep ViewModels UI-agnostic.
* Do not generate malformed code - always validate and sanitize user input.

## Performance Considerations

* Use `ObservableCollection<T>` for collections that need UI binding.
* For large collections that don't change often, consider using `List<T>` and raising PropertyChanged explicitly.
* Avoid unnecessary object allocations in loops.
* Use `StringBuilder` for complex string concatenation (though prefer `ICodeBuilder` for code generation).
* Cache results of expensive operations when appropriate.

```csharp
// Cache the safe identifier instead of regenerating it repeatedly
private string? _safeClassName;
public string SafeClassName
{
    get
    {
        if (_safeClassName == null)
            _safeClassName = IdentifierSanitizer.MakeSafeIdentifier(ClassName, "GeneratedNpc");
        return _safeClassName;
    }
}
```

## Version Control

* Commit logical, atomic changes.
* Write clear, descriptive commit messages.
* Don't commit commented-out code - use version control instead.
* Don't commit user-specific files (`.user`, `bin/`, `obj/`, etc.) - ensure `.gitignore` is properly configured.

## Summary

These standards are designed to keep the codebase clean, consistent, and maintainable. When in doubt:

1. Look at existing code for patterns and conventions
2. Prefer clarity over cleverness
3. Follow MVVM principles strictly
4. Keep concerns separated
5. Write code that others (including future you) can easily understand
