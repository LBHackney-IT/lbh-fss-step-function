using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace AwsDotnetCsharp
{
    public class Handler
    {
        public Response SendEmail1(Request request)
        {
            return new Response
            {
                name = request.name,
                likes = request.likes
            };
        }

        public Response Wait1stMonth(Request request)
        {
            return new Response
            {
                name = request.name,
                likes = request.likes
            };
        }


    }

    public class Request
    {
        public string name { get; set; }
        public string likes { get; set; }
    }

    public class Response
    {
        public string name { get; set; }
        public string likes { get; set; }
    }
}