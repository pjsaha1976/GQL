using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using System.Text.Json;
using GQL.Models;
using Microsoft.Extensions.DependencyInjection;
using GQL.Data;

namespace GQL.GraphQL;

public class ProductViewTypeModule : ITypeModule, IDisposable
{
    private ProductViewConfig _config;
    private readonly FileSystemWatcher? _watcher;
    private readonly string _configFilePath;
    private readonly IServiceProvider _serviceProvider;

    public ProductViewTypeModule(ProductViewConfig config, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        _config = config;
        _configFilePath = System.IO.Path.Combine(env.ContentRootPath, "GraphQL", "productview-config.json");
        _serviceProvider = serviceProvider;

        // Watch for changes to the config file
        var configPath = System.IO.Path.Combine(env.ContentRootPath, "GraphQL");
        if (System.IO.Directory.Exists(configPath))
        {
            _watcher = new FileSystemWatcher(configPath, "productview-config.json");
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += OnConfigFileChanged;
            _watcher.EnableRaisingEvents = true;
        }
    }

    public event EventHandler<EventArgs>? TypesChanged;

    private async void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        // Reload the config file
        try
        {
            System.Threading.Thread.Sleep(100); // Small delay to ensure file is written
            var configJson = System.IO.File.ReadAllText(_configFilePath);
            _config = JsonSerializer.Deserialize<ProductViewConfig>(configJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            }) ?? new ProductViewConfig();
            
            Console.WriteLine($"Config file changed, recreating view with {_config.DynamicFields.Count} fields...");
            
            // Recreate the database view
            using (var scope = _serviceProvider.CreateScope())
            {
                var viewManager = scope.ServiceProvider.GetRequiredService<ViewManager>();
                await viewManager.RecreateProductsViewAsync();
            }
            
            // Trigger schema regeneration
            TypesChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reloading config: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }

    public async ValueTask<IReadOnlyCollection<ITypeSystemMember>> CreateTypesAsync(
        IDescriptorContext context,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"CreateTypesAsync called with {_config.DynamicFields.Count} dynamic fields");
        
        var types = new List<ITypeSystemMember>();

        var productViewType = new ObjectType<ProductView>(descriptor =>
        {
            descriptor.Name("ProductView");
            
            // Explicitly add all fields including dynamic ones
            descriptor.Field(f => f.Id).Type<NonNullType<IntType>>();
            // Use JsonType to properly serialize JsonElement as JSON object
            descriptor.Field(f => f.Data).Type<JsonType>();

            // Add dynamic fields from config
            Console.WriteLine("Adding dynamic fields:");
            foreach (var field in _config.DynamicFields)
            {
                Console.WriteLine($"  - {field.Name} ({field.Type})");
                ConfigureDynamicField(descriptor, field);
            }
        });

        types.Add(productViewType);

        return await ValueTask.FromResult<IReadOnlyCollection<ITypeSystemMember>>(types);
    }

    private void ConfigureDynamicField(IObjectTypeDescriptor<ProductView> descriptor, DynamicField field)
    {
        switch (field.Type)
        {
            case "String":
                descriptor.Field(field.Name)
                    .Type<StringType>()
                    .Resolve(ctx =>
                    {
                        var parent = ctx.Parent<ProductView>();
                        return ExtractJsonValue<string>(parent.Data, field.JsonPath);
                    });
                break;

            case "Int":
                descriptor.Field(field.Name)
                    .Type<IntType>()
                    .Resolve(ctx =>
                    {
                        var parent = ctx.Parent<ProductView>();
                        return ExtractJsonValue<int?>(parent.Data, field.JsonPath);
                    });
                break;

            case "Boolean":
                descriptor.Field(field.Name)
                    .Type<BooleanType>()
                    .Resolve(ctx =>
                    {
                        var parent = ctx.Parent<ProductView>();
                        return ExtractJsonValue<bool?>(parent.Data, field.JsonPath);
                    });
                break;

            case "Float":
            case "Decimal":
                descriptor.Field(field.Name)
                    .Type<DecimalType>()
                    .Resolve(ctx =>
                    {
                        var parent = ctx.Parent<ProductView>();
                        return ExtractJsonValue<decimal?>(parent.Data, field.JsonPath);
                    });
                break;

            case "ListString":
                descriptor.Field(field.Name)
                    .Type<ListType<StringType>>()
                    .Resolve(ctx =>
                    {
                        var parent = ctx.Parent<ProductView>();
                        return ExtractJsonValue<List<string>>(parent.Data, field.JsonPath) ?? new List<string>();
                    });
                break;

            default:
                descriptor.Field(field.Name)
                    .Type<StringType>()
                    .Resolve(ctx =>
                    {
                        var parent = ctx.Parent<ProductView>();
                        return ExtractJsonValue<string>(parent.Data, field.JsonPath);
                    });
                break;
        }
    }

    private T? ExtractJsonValue<T>(JsonElement? data, string jsonPath)
    {
        if (!data.HasValue) return default;

        try
        {
            var propertyName = jsonPath.Replace("$.", "");

            if (data.Value.TryGetProperty(propertyName, out var element))
            {
                return JsonSerializer.Deserialize<T>(element.GetRawText());
            }
        }
        catch
        {
            return default;
        }

        return default;
    }
}
