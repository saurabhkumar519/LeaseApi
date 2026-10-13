using LeaseBackend.Model.DTOs.UserDTO;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class EntryTypeAsStringSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(EntryType))
        {
            schema.Type = "string";
            schema.Format = null;
            schema.Enum.Clear();

            foreach (var name in Enum.GetNames(typeof(EntryType)))
            {
                schema.Enum.Add(new OpenApiString(name));
            }
        }
        if (context.Type == typeof(UserTrackingEnum))
        {
            schema.Type = "string";
            schema.Format = null;
            schema.Enum.Clear();
            foreach (var name in Enum.GetNames(typeof(UserTrackingEnum)))
            {
                schema.Enum.Add(new OpenApiString(name));
            }
        }
    }
}
