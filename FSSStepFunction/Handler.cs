using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace AwsDotnetCsharp
{
    public class Handler
    {
        public string SendEmail1(Request request)
        {
            return $"Send Email {request.name}! I know you like {request.likes}";
        }

        public string Wait1stMonth(Request request)
        {
            return $"Wait 1st Month {request.name}! Step1 {request.likes}";
        }


    }

    public class Request
    {
        public string name { get; set; }
        public string likes { get; set; }
    }
}