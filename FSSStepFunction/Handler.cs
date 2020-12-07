using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace AwsDotnetCsharp
{
    public class Handler
    {
        public string FssFunctions(Request request)
        {
            return $"Hello {request.name}! I know you like {request.likes}";
        }
    }

    public class Request
    {
        public string name { get; set; }
        public string likes { get; set; }
    }
}