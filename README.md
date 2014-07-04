MVC
===

The guts and insides of the MVC project that supports Kartessian.

### Server Side Request Cache

Kartessian implements a custom server side cache system. The advantages are visible when you have multiple request to the same action/content that needs to be processed each time (i.e. reading a table in the database and returning a json string). If the table data doesn't change often, then is where this cache system takes into action.

How it works:

Kartessian overrides the default Controller class to implement this Cache feature, creating a custom [https://github.com/Kartessian/MVC/blob/master/MVCproject/MVCproject/Controllers/BaseController.cs](BaseController) class.

And then as simple as:

```csharp
  
  // original
  public class HomeController : Controller
  
  // replace with:
  public class HomeController : BaseController
  
```

Next step is use custom [https://github.com/Kartessian/MVC/blob/master/MVCproject/MVCproject/Classes/Attributes.cs](Attributes) created to define when a Request will be cacheable or not.

```csharp
  
  [Cache()] // Custom Attribute to indicate we want to cache the result of this Request
  public ContentResult getUsers()
  {
      // ...
  }
  
```

The [https://github.com/Kartessian/MVC/blob/master/MVCproject/MVCproject/Controllers/BaseController.cs](BaseController) will now handle when the Attribute is present and create a cache file with the Response.

Next time the same Request is requested, the Controller will check if there is a cache file with the Response and if it is it will return the content of the file instead of process the entire Request.

In the web.config you can find the parameters needed to enable/disable the cache (cache\_enabled) and to define the path you want to use to store the cache files (cache\_folder).
