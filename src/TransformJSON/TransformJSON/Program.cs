using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;


var inputJSON = "{\"InvoiceNo\":536365,\"StockCode\":\"85123A\",\"Description\":\"WHITEHANGINGHEARTT-LIGHTHOLDER\",\"Quantity\":6,\"InvoiceDate\":\"12/1/20108:26\",\"UnitPrice\":2.55,\"CustomerID\":17850,\"Country\":\"UnitedKingdom\",\"Order\":{\"OrderId\":123123,\"OrderNumber\":10,\"Price\":5000,\"Products\":[{\"Product\":1,\"Name\":\"Kindle\",\"Price\":1000,\"Stock\":{\"StockCode\":\"85123A\",\"Address\":\"Rua01\"}},{\"Product\":2,\"Name\":\"AppleWatch\",\"Price\":4000,\"Stock\":{\"StockCode\":\"85123A\",\"Address\":\"Rua02\"}}]}}";

var schemaJSON = "{\"$schema\":\"http://json-schema.org/draft-04/schema#\",\"type\":\"object\",\"properties\":{\"InvoiceNo\":{\"type\":\"number\"},\"StockCode\":{\"type\":\"string\"},\"Description\":{\"type\":\"string\"},\"Quantity\":{\"type\":\"number\"},\"Order\":{\"type\":\"object\",\"properties\":{\"OrderId\":{\"type\":\"number\"},\"OrderNumber\":{\"type\":\"number\"},\"Products\":{\"type\":\"object\",\"properties\":{\"Product\":{\"type\":\"number\"},\"Name\":{\"type\":\"string\"},\"Stock\":{\"type\":\"object\",\"properties\":{\"Address\":{\"type\":\"string\"}}}}}}}}}";

JSchema schema = JSchema.Parse(schemaJSON);

JToken inputObject = JsonConvert.DeserializeObject<JToken>(inputJSON);

var outputObject = TransformJson(inputObject, schema.Properties);

Console.WriteLine(outputObject);

JToken TransformJson(JToken token, IDictionary<string, JSchema> properties)
{
    var outputObject = JsonConvert.DeserializeObject<JToken?>("{\"Root\":0}");

    foreach (JToken element in token.Children())
    {
        var property = element as JProperty;
        if (property != null && properties.ContainsKey(property.Name))
        {
            var objectschema = property?.Value as JObject;
            var arraryschema = property?.Value as JArray;
            if(objectschema != null)
            {
                var result = TransformJson(objectschema, properties.Single(x => x.Key == property.Name).Value.Properties);

                JProperty propertyObject = new JProperty(property.Name, result);
                outputObject.Last.AddAfterSelf(propertyObject);
            }
            else if (arraryschema != null)
            {
                var objects = new List<JObject>();
                foreach (var item in arraryschema)
                {
                    var result = TransformJson(item, properties.Single(x => x.Key == property.Name).Value.Properties);
                    objects.Add(result as JObject);
                }

                var newArray = new JArray(objects);

                JProperty propertyObject = new JProperty(property.Name, newArray);
                outputObject.Last.AddAfterSelf(propertyObject);
            }
            else
            {
                outputObject.Last.AddAfterSelf(property);
            }
        }
    }

    outputObject.First.Remove();
    return outputObject;
}