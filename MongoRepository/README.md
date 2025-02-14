# ⭐ Introduction 
This is mongo repository.
It has the following types of repository:  
```
                ReadOnlyRepository
                      ↙    ↘
CachedReadOnlyRepository   ReadWriteRepository
                                    ↘
                               ReadWriteWithAuditRepository
```  
# 📃 Key features
    * Multi-tenant with prefix or suffix
    * Lazy loading for database/collection/index
    * Azure Application Insight Telemetry
    * Support Read/Write Separation Replica sets
    * Support pagination results
    * Support auditing
    * Support cache
    * Support Queryable
    * Auto index lazy check/creation
    * Support Single Field Index
    * Support Compound Index
    * Support Geospatial Index
    * Support Time-to-live Index (TTL)
    * Support Nested Index
    * Support Expression Index 
    * Support Partial Index 
    * Support write your Own customized Index (Eager index builder)
# ▶️ Getting Started
Install this nuget package to use it.
1. create options class to extend MongoDbOptions for setting up MongoDB connection string
```
// Your Options class
public class SampleOptions : MongoDbOptions {}
```

``` 
// Add to your appsettings.json
 "SampleOptions": {
   "ReadWriteConnection": "mongodb://localhost:27017",  // ReadWrite endpoint.
   "ReadOnlyConnection": "mongodb://localhost:27017",   // Read replica endpoint
   "DefaultDatabase": "Sample"  //if not using attribute define database.
 }
```
2. in your entity class, implement `IEntity<KeyType>` eg. `IEntity<ObjectId>`, `IEntity<string>` etc..
then add class level attribute
```
[EntityDatabase("SampleDatabase")]
[EntityCollection("SampleCollection")]
public class SampleEntity : IEntity<ObjectId>
{
    public ObjectId Id { get; set; }

    public string Name { get; set; }

    public string ToDescription()
    {
        return $"This is a sample with {Id}";
    }
}
```
3. Define your Repository. 
```
public interface ISampleReadWriteRepository : IReadWriteRepository<SampleEntity, ObjectId> {}

public class SampleRepository : ReadWriteRepository<SampleEntity, ObjectId>, ISampleReadWriteRepository
{
    public SampleRepository(IOptions<SampleOptions> mongoOptions, IMongoClientFactory factory) 
    : base(mongoOptions, factory) {}
}
```
4. Set start up.
```
builder.Services.Configure<SampleOptions>(builder.Configuration.GetSection("SampleOptions"));
builder.Services.AddMongoClientFactory();
builder.Services.AddScoped<ISampleReadWriteRepository, SampleRepository>();

```

# 🐞 Debug in your own solution
1. Remove the nuget package of `YSMongoRepository` from your project.
2. In your project, `add project reference`, add `YSMongoRepository` into your project.
3. Happy debugging 🤞

# 🔑 Key feature introduce
### ➡️ 1. Multi-tenant set up 
Database level multi-tenant is supported by providing `ITenantIdentificationService`.
In this `ITenantIdentificationService`, you need to implement your own logic to provide suffix or prefix.
This repository will automaticaly create that database, collection and index for you with lazy-loading.

Recommended way is to have an interface inherit from the `ITenantIdentificationService`, then implement your logic and pass into repository. (see example of `MultiTenantIdentificationService.cs` in Sample project) 

### ➡️ 2. Lazy loading for database/collection/index
This repository will only Create/check the resource that you going to invoke.
What are the benefits? 
1. You don't need know the whole DB structure.
1. Your service will not touch the DB you don't use.
1. It will automatically create `Database`, `Collection` and `index` for you if resource is not there, especially in multi-tenant scenario.

### ➡️ 3. Azure Application Insight Telemetry
In some scenarios below, you may be interested in what query you are using.
* Would like to know what query is using to fetch data.
* Would like to know how the query hit the index.
* Would like to optimize the query. 
Then you can do the following step:
1. -> Go into your targeting azure application insight
1. -> Go into Transaction search  ( you can also search in Logs )
1. -> Find the dependency record for your mongo db transaction
1. -> View the detail in the `command` session.
1. -> Grab the `filter` part to your `compass` that you can use `Explain` to explore it.

### ➡️ Support Read/Write Separation Replica sets
To use Read/Write Separation, simplely just update your connection string:
```
"SampleOptions": {
   "ReadWriteConnection": "mongodb://localhost:27017",  // ReadWrite endpoint.
   "ReadOnlyConnection": "mongodb://localhost:27017",   // Read replica endpoint
   "DefaultDatabase": "Sample"  //if not using attribute define database.
 }
```

### ➡️ Support pagination results
This repository has built-in Pagination Result. Check out `PaginatedResult.cs`.

### ➡️ Support auditing
To use auditing feature, 

