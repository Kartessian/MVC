MVC
===

The guts and insides of the MVC project that supports Kartessian.

### Server Side Request Cache

Kartessian implements a custom server side cache system. The advantages are visible when you have multiple request to the same action/content that needs to be processed each time (i.e. reading a table in the database and returning a json string). If the table data doesn't change often, then is where this cache system takes into action.

How it works:

Kartessian overrides the default Controller class to implement this Cache feature, creating a custom [BaseController](https://github.com/Kartessian/MVC/blob/master/MVCproject/MVCproject/Controllers/BaseController.cs) class.

And then as simple as:

```csharp
  
  // original
  public class HomeController : Controller
  
  // replace with:
  public class HomeController : BaseController
  
```

Next step is use custom [Attributes](https://github.com/Kartessian/MVC/blob/master/MVCproject/MVCproject/Classes/Attributes.cs) created to define when a Request will be cacheable or not.

```csharp
  
  [Cache()] // Custom Attribute to indicate we want to cache the result of this Request
  public ContentResult getUsers()
  {
      // ...
  }
  
```

The [BaseController](https://github.com/Kartessian/MVC/blob/master/MVCproject/MVCproject/Controllers/BaseController.cs) will now handle when the Attribute is present and create a cache file with the Response.

Next time the same Request is requested, the Controller will check if there is a cache file with the Response and if it is it will return the content of the file instead of process the entire Request.

In the web.config you can find the parameters needed to enable/disable the cache (cache\_enabled) and to define the path you want to use to store the cache files (cache\_folder).

### Database Access

This projects uses the SQLClient Class to connect to a SQL Server Database. (You can replace it with any other like MySQLClient to connect to the database of your choice. The code is pretty standar, just need to change the object names.)

The logic is in the [Database](https://github.com/Kartessian/MVC/blob/master/MVCproject/MVCproject/Database/Database.cs) file, where you will find some basic functionallity: ExecuteSQL, ExecuteScalar, GetDataTable, BeginTransaction, etc...


The ussage is simple:

```csharp

  Database db = new Database("connectionString:here");
  
  DataTable myTable = db.GetDataTable("select * from myTable");
  
  int iRowsDeleted = db.ExecuteSQL("delete from myTable where name = @firstName", 
      new KeyValuePair<string,object>("@firstName", "john"));
  
  db.Dispose();

```

If you don't want to use SQL code you can take advantage of the [ITable](https://github.com/Kartessian/MVC/blob/master/MVCproject/MVCproject/Database/ITable.cs) interface.
You then can create a class that matches the column names in the table with the properties you create into it, like:

```csharp

public class User : ITable
    {
        [RelatedField] // The RelatedField attribute indicates this "field" does not exist in the table
        public string TableName
        {
            get { return "users"; }
        }

        [PrimaryKeyDefinition(IsPrimaryKey: true, AutoNumeric: true)]
        public int id { get; set; }

        public string firstName { get; set; }
        public string lastName { get; set; }

    }

```

And then just simply:

```csharp

  Database db = new Database("connectionString:here");
  
  List<User> lUsers = db.GetRecords<User>();
  
  User johnSmith = db.GetRecors<User>(
      new KeyValuePair<string,object>("firstName", "john"),
      new KeyValuePair<string,object>("lastName", "smith")
    ).FirstOrDefault();


  db.Dispose();

```

You can always use EntityFramework if you wish; this is just a very lightweight approach to it.

