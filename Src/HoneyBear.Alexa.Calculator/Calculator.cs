using System;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Slight.Alexa.Framework.Models.Requests;
using Slight.Alexa.Framework.Models.Requests.RequestTypes;
using Slight.Alexa.Framework.Models.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace HoneyBear.Alexa.Calculator
{
    public class Function
    {
        public SkillResponse Handle(SkillRequest input, ILambdaContext context)
        {
            var log = context.Logger;
            var speech = "Failed to interpret the request.";
            var shouldEndSession = true;

            if (input.GetRequestType() == typeof(ILaunchRequest))
            {
                log.LogLine("Default LaunchRequest made");

                shouldEndSession = false;
                speech = "Welcome to HoneyBear Calculator.  You can ask us to add numbers!";
            }
            else if (input.GetRequestType() == typeof(IIntentRequest))
            {
                log.LogLine($"Intent Requested {input.Request.Intent.Name}");

                var n1 = Convert.ToDouble(input.Request.Intent.Slots["firstnum"].Value);
                var n2 = Convert.ToDouble(input.Request.Intent.Slots["secondnum"].Value);
                speech = $"The result is {n1 + n2}.";
            }

            log.LogLine($"Response: {speech}");

            return new SkillResponse
            {
                Response = new Response
                {
                    ShouldEndSession = shouldEndSession,
                    OutputSpeech = new PlainTextOutputSpeech
                    {
                        Text = speech
                    }
                },
                Version = "1.0"
            };
        }
    }
}
