/*using API.Models;
using System.Reflection;
using System.Text;

namespace API.Controllers;

public class OwnerRepository
{

    private static void ApplySort(ref IQueryable<Owner> owners, string orderByQueryString)
    {
        if (!owners.Any())
            return;

        if (string.IsNullOrWhiteSpace(orderByQueryString))
        {
            owners = owners.OrderBy(x => x.Name);
            return;
        }

        var orderParams = orderByQueryString.Trim().Split(',');
        var propertyInfos = typeof(Owner).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var orderQueryBuilder = new StringBuilder();

        foreach (var param in orderParams)
        {
            if (string.IsNullOrWhiteSpace(param))
                continue;

            var propertyFromQueryName = param.Split(" ")[0];
            var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

            if (objectProperty == null)
                continue;

            var sortingOrder = param.EndsWith(" desc") ? "descending" : "ascending";

            orderQueryBuilder.Append($"{objectProperty.Name.ToString()} {sortingOrder}, ");
        }

        var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');

        if (string.IsNullOrWhiteSpace(orderQuery))
        {
            owners = owners.OrderBy(x => x.Name);
            return;
        }

        owners = owners.OrderBy(orderQuery);
    }

}*/