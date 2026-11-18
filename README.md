# Toolkit popup shape/shadow ownership reproduction

This sample uses public CommunityToolkit.Maui APIs to inspect the host Borders of
two nested popups. It demonstrates that default popup options share mutable shape
and shadow objects across hosts. No reflection, private Toolkit types, forced GC,
application services or external data are involved.

## Versions and prerequisites

- .NET SDK **11.0.100**, workload set **11.0.100.1** (pinned in `global.json`).
- Microsoft.Maui.Controls **11.0.10**.
- CommunityToolkit.Maui **17.0.0**.
- The .NET 11 MAUI/Windows App SDK runtime for Windows execution, Android API 24
  or later, or a supported Apple development environment for iOS/Mac Catalyst.

The template's target frameworks were updated to .NET 11 to match the versions
under investigation. The app displays the actual runtime and OS version, and
includes those values when copying the results.

## Run

Install the corresponding MAUI workload and native platform toolchain, then open
`MauiTest.slnx` in Visual Studio or run from this directory:

```powershell
dotnet workload restore
dotnet build MauiTest.csproj -t:Run -f net11.0-windows10.0.19041.0
```

For Android, select an emulator/device in Visual Studio or use:

```powershell
dotnet build MauiTest.csproj -t:Run -f net11.0-android
```

iOS and Mac Catalyst execution require a Mac and the matching Xcode/toolchain.

## Steps to reproduce

1. Keep **Separate new PopupOptions (default)** selected.
2. Select **Open popup A**. Both A parent checks initially read `True`.
3. In A, select **Open popup B**.
4. Inspect the ownership results displayed inside B. With the affected versions,
   the expected reproduction output is:

   ```text
   B opened while A remains open
   A shape parent is A: False
   A shadow parent is A: False
   Shapes shared: True
   Shadows shared: True
   B shape parent is B: True
   B shadow parent is B: True
   ```

5. Select **Close Popup B**. A displays its ownership checks again after B's
   close completes. A's parent checks remain `False` with these package versions.
6. You can reopen B while keeping A open. Close A to return to the main screen.
7. Repeat with **PopupOptions.Empty** and **Omitted options**. Both exercise the
   same default appearance sharing.
8. Repeat with **Fresh shape and shadow (workaround)**. A and B now retain their
   respective parents (`True`), and both sharing checks read `False`.
9. After closing A, select **Copy results** to copy the runtime/OS, selected mode,
   and observations before, during and after B. Debug builds also write the
   observations to the debugger output.

Expected behavior: creating or removing another popup preserves each existing
host Border's shape/shadow ownership. Creating a separate `PopupOptions` alone
should not cause another popup's appearance Elements to be reparented.

## Implementation

- `MainPage.xaml.cs` presents real popups through `Page.ShowPopupAsync` and reads
  `Popup.Parent` after `Opened`. That public property exposes the Toolkit host
  Border. Checks use `ReferenceEquals` and the appearance Elements' `Parent`.
- `OwnershipPopup.cs` supplies only the inspection text and open/close buttons.
- `PopupAppearance.cs` switches between the three default paths and fresh
  shape/shadow objects with the same default appearance.

The native popup lifecycle runs normally. The sample deliberately retains A
while B is open so the ownership change is directly observable.

## Scope and validation

This reproduces incorrect parent ownership. It does not promise to reproduce a
`RealParent ... has been Garbage Collected` warning, crash, memory leak or a
particular native GC schedule. The relationship to previously observed iOS
modal-removal warnings remains unproven.

Validation on 18 November 2026, from Windows build 26300:

- All four configured Debug targets build with zero warnings/errors using
  `dotnet build MauiTest.slnx`, with an explicit installed Android SDK path and
  `AndroidKeyStore=false` supplied on the validation machine.
- A separate managed check source-linked this sample's `PopupAppearance.cs` and
  constructed actual MAUI Borders. Separate default options and `PopupOptions.Empty`
  shared both objects and changed A's parent ownership; fresh appearance objects
  preserved both owners. All checks passed. This check exercises options/Border
  ownership, not native popup navigation.
- Changed C# files pass `dotnet format style --verify-no-changes`; whitespace
  checks pass.

Native interactive verification is still pending because local app automation
was unavailable. Windows compilation and managed iOS/Mac Catalyst outputs do not
establish native popup behavior, Apple signing or device GC timing.

## Upstream references

- [Shared defaults](https://github.com/CommunityToolkit/Maui/blob/17.0.0/src/CommunityToolkit.Maui/Primitives/Defaults/DefaultPopupOptionsSettings.shared.cs)
- [PopupOptions initialization](https://github.com/CommunityToolkit/Maui/blob/17.0.0/src/CommunityToolkit.Maui/Views/Popup/PopupOptions.shared.cs)
- [Popup host bindings](https://github.com/CommunityToolkit/Maui/blob/17.0.0/src/CommunityToolkit.Maui/Views/Popup/PopupPage.shared.cs#L717)
