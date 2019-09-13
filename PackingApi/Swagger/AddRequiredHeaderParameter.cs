using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackingApi.Swagger
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(Swashbuckle.AspNetCore.Swagger.Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<IParameter>();

            operation.Parameters.Add(new HeaderParameter()
            {
                Name = "api-key",
                In = "header",
                Type = "string",
                Required = true
            });
        }

        class HeaderParameter : NonBodyParameter
        {
        }
    }
}