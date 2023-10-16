using CosmosDb_Demo;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class Program
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();
    // Replace <documentEndpoint> with the information created earlier
    private static readonly string EndpointUri = Configuration["AppSettings:EndpointUri"] ?? "";

    // Set variable to the Primary Key from earlier.
    private static readonly string PrimaryKey = Configuration["AppSettings:PrimaryKey"] ?? "";

    // The Cosmos client instance
    private CosmosClient? CosmosClient { get; set; }

    //The database we will create
    private Database? Database { get; set; }

    // The container we will create.
    private Container? Container { get; set; }

    // The names of the database and container we will create
    private readonly string DatabaseId = Configuration["AppSettings:DatabaseId"] ?? "";
    private readonly string ContainerId = Configuration["AppSettings:ContainerId"] ?? "";
    private readonly string Partitionkey = Configuration["AppSettings:Partitionkey"] ?? "";

    public static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Beginning operations...\n");
            Program p = new Program();
            await p.CosmosAsync();

        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
        }
        finally
        {
            Console.WriteLine("End of program, press any key to exit.");
            Console.ReadKey();
        }
    }

    public async Task CosmosAsync()
    {
        // Create a new instance of the Cosmos Client
        CosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

        // Runs the CreateDatabaseAsync method
        await CreateDatabaseAsync();

        // Run the CreateContainerAsync method
        await CreateContainerAsync();
        try
        {
            Console.WriteLine("Enter Your Choice 1.Create Item and 2.Get Item : ");
            var choice = Convert.ToInt32(Console.ReadLine());

            switch(choice)
            {
                case 1: 
                    //Run the CreateItemAsync
                    await CreateItemAsync();
                    break;
                case 2:
                    //Run ReadItemAsync
                    await ReadItemAsync();
                    break;
                default: Console.WriteLine("Enter Valid Choice");
                    break;
            }
        }
        catch
        {
            Console.WriteLine("Enter Valid Choice");
        }
    }

    private async Task CreateDatabaseAsync()
    {
        // Create a new database using the cosmosClient
        Database = await CosmosClient!.CreateDatabaseIfNotExistsAsync(DatabaseId);
        Console.WriteLine("Created Database: {0}\n", Database.Id);
    }

    private async Task CreateContainerAsync()
    {
        // Create a new container
        Container = await Database!.CreateContainerIfNotExistsAsync(ContainerId, Partitionkey);
        Console.WriteLine("Created Container: {0}\n", Container.Id);
    }

    private async Task CreateItemAsync()
    {
        Console.WriteLine("======Create Item======");

        try
        {
            Console.WriteLine("Enter FirstName : ");
            var firstName = Console.ReadLine();
            Console.WriteLine("Enter Last Name : ");
            var lastName = Console.ReadLine();
            Console.WriteLine("Enter Designation : ");
            var designation = Console.ReadLine();
            UserModel user = new UserModel()
            {
                id = Guid.NewGuid().ToString(),
                FirstName = firstName,
                LastName = lastName,
                Designation = designation,
                IsActive = true,
            };

            await Container!.CreateItemAsync(user, new PartitionKey(user.LastName));

            Console.WriteLine("Item Created Successfully");
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message.ToString());
        }
    }

    private async Task ReadItemAsync()
    {
        Console.WriteLine("======Read Item======");
        Console.WriteLine("Enter Id : ");
        var yourItemId = Console.ReadLine();
        Console.WriteLine("Enter Partition By : ");
        var partitionBy = Console.ReadLine();

        try
        {
            ItemResponse<UserModel> response = await Container!.ReadItemAsync<UserModel>(yourItemId, new PartitionKey(partitionBy));

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                UserModel item = response.Resource;
                Console.WriteLine(JsonConvert.SerializeObject(item));

                // Process or use the retrieved item here
            }
            else
            {
                Console.WriteLine("Item Not Found");
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Console.WriteLine("Item Not Found");
            // Handle the case where the item was not found
        }

    }
}
