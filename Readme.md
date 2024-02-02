# Setting up a solution for MAUI hybrid and Blazor web with shared UI

This repo demonstrates a starter solution that contains a MAUI hybrid (native, cross-platform) app, a Blazor web app and a Razor class library that contains all the shared UI that is used by both native and web apps.

![Apps running on Windows, Android, and Web with shared UI](MyApp.png)

To manually set this up yourself in Visual Studio, follow these steps:

1.  Create an empty Solution and name it `MyApp`

2.  Add new project MAUI Blazor Hybrid app and name it `MyApp.MAUI`

3.  Add new project Blazor Web App and name it `MyApp.Web`. Select the following options:    

    a.  Authentication type = none

    b.  Configure for HTTPS is checked

    c.  Interactive render mode = Server

    d.  Interactivity location = **Global** <-- _This setting is important because hybrid apps always run interactive and will throw errors on pages or components that explicitly specify a render mode. See [#51235](https://github.com/dotnet/aspnetcore/issues/51235)_
    
    e.  Uncheck Include sample pages

4.  Add new project Razor Class Library (RCL) and name it `MyApp.Shared`

    a.  don\'t select \"support pages and views\" (default)

5.  Now add project references to `MyApp.Shared` from both `MyApp.MAUI` & `MyApp.Web` project

6.  Move the `Components` folder and all of its contents from `MyApp.MAUI` to `MyApp.Shared` (Ctrl+X, Ctrl+V)

7.  Move `wwwroot/css` folder and all of its contents from from `MyApp.MAUI` to `MyApp.Shared` (Ctrl+X, Ctrl+V)

8.  Move `Routes.razor` from `MyApp.MAUI` to `MyApp.Shared` (Ctrl+X, Ctrl+V). 

9.  Open the `Routes.razor` file and change `MauiProgram` to `Routes`:
```
<Router AppAssembly="@typeof(Routes).Assembly">
...
```
10. Open the `MainPage.xaml` in the `MyApp.MAUI` project and add a `xmlns:shared` reference to the
    `MyApp.Shared` RCL and update the `BlazorWebView` `RootComponent` `ComponentType` from `local` to `shared`:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:MyApp.MAUI"
             xmlns:shared="clr-namespace:MyApp.Shared;assembly=MyApp.Shared" 
             x:Class="MyApp.MAUI.MainPage"
             BackgroundColor="{DynamicResource PageBackgroundColor}">

    <BlazorWebView x:Name="blazorWebView" HostPage="wwwroot/index.html">
        <BlazorWebView.RootComponents>
            <RootComponent Selector="#app" ComponentType="{x:Type shared:Routes}" />
        </BlazorWebView.RootComponents>
    </BlazorWebView>
</ContentPage>
```

11. In the `MyApp.MAUI` project open `wwwroot/index.html` and change stylesheets to point to
    `_content/MyApp.Shared/`:

```xml
<link rel="stylesheet" href="_content/MyApp.Shared/css/bootstrap/bootstrap.min.css" />
<link rel="stylesheet" href="_content/MyApp.Shared/css/app.css" />
```

12. Open `App.razor` from `MyApp.Web` project and **add** the stylesheet references to
    the `MyApp.Shared` there too:

```xml
<link rel="stylesheet" href="_content/MyApp.Shared/css/bootstrap/bootstrap.min.css" />
<link rel="stylesheet" href="_content/MyApp.Shared/css/app.css" />   
```

13. Move `_Imports.razor` from `MyApp.MAUI` to `MyApp.Shared` (overwrite the one that is there)
    and rename the last two `@using`s to `MyApp.Shared`

```code
...
@using MyApp.Shared
@using MyApp.Shared.Components
```

14. In the `MyApp.Web` project, open `_Imports.razor` and add to end `@using MyApp.Shared`

15.  In the `MyApp.Web` project, delete files `Routes.razor`, `Layouts` folder & all its contents, and `Pages\Home.razor` (leave the `Error.razor` page)

16. Open `MyApp.Web` project `Program.cs` file and `AddAddionalAssemblies` to `MapRazorComponents`:

```code
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(MyApp.Shared._Imports).Assembly);
```
**You should now be all set! F5 and party on.**


# Using Interfaces to support different device implementations

This sample also shows how to use interfaces on the UI to call into different implementations across the web app and the native (MAUI Hybrid) app. We will make a component that displays the device form factor. We can use the MAUI abstraction layer for all the native apps but we will need to provide our own implementation for the web app.

1.  In the `MyApp.Shared` project, create an `Interfaces` folder and add file called `IFormFacto.cs` with the following code:

```csharp
namespace MyApp.Shared.Interfaces
{
    public interface IFormFactor
    {
        public string GetFormFactor();
        public string GetPlatform();
    }
}
```

2.  Move `Component1.razor` in the `MyApp.Shared` project to `Components` folder and
    rename it to `FormFactorComponent.razor` and add the following code:

```razor
@using MyApp.Shared.Interfaces
@inject IFormFactor FormFactor

<div class="my-component">
    <h3>@formFactor</h3>

    <em>This component is defined in the <strong>MyApp.Shared</strong> library.</em>
</div>

@code {
    private string formFactor => FormFactor.GetFormFactor();
}
```
3. Now that we have the interface defined we need to provide implementations in the web and native apps. In the `MyApp.Web` project, add a folder called `Services` and add a file called `FormFactor.cs`. Add the following code:

```csharp
using MyApp.Shared.Interfaces;

namespace MyApp.Web.Services
{
    public class FormFactor : IFormFactor
    {
        public string GetFormFactor()
        {
            return "Web";
        }

        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
``` 
4. Now in the `MyApp.MAUI` project, add a folder called `Services` and add a file called `FormFactor.cs`. We can use the MAUI abstractions layer to write code that will work on all the native device platforms. Add the following code:
 
```csharp
using MyApp.Shared.Interfaces;

namespace MyApp.MAUI.Services
{
    public class FormFactor : IFormFactor
    {
        public string GetFormFactor()
        {
            return DeviceInfo.Idiom.ToString();
        }

        public string GetPlatform()
        {
            return DeviceInfo.Platform.ToString() + " - " + DeviceInfo.VersionString;
        }
    }
}
```
5. Use dependency injection to get the implementations of these services into the right place. In the `MyApp.MAUI` project open `MauiProgram.cs` and add the `using`s at the top:
```csharp
using Microsoft.Extensions.Logging;
using MyApp.MAUI.Services;
using MyApp.Shared.Interfaces;
```
6. And right before the call to `builder.Build();` add the following code:

```csharp
...
// Add device specific services used by Razor Class Library (MyApp.Shared)
builder.Services.AddSingleton<IFormFactor, FormFactor>();

return builder.Build();
```
6. Similarly, in the `MyApp.Web` project, open the `Program.cs` and right before the call to `builder.Build();` add the `using`s at the top:
```csharp
using MyApp.Web.Components;
using MyApp.Shared.Interfaces;
using MyApp.Web.Services;  
```
6. And right before the call to `builder.Build();` add the following code:

```csharp
...
// Add device specific services used by Razor Class Library (MyApp.Shared)
builder.Services.AddScoped<IFormFactor, FormFactor>();

var app = builder.Build();
```
