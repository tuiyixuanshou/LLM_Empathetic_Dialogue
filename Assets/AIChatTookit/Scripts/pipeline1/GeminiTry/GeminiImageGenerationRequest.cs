using UnityEngine;
using System.Threading.Tasks;
using Uralstech.UGemini;
using Uralstech.UGemini.Models.Generation;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using Uralstech.UGemini.Models.Content;

public class GeminiImageGenerationRequest : IGeminiPostRequest
{
    private readonly string prompt;
    private readonly byte[] imageBytes;

    public GeminiImageGenerationRequest(string prompt, byte[] imageBytes)
    {
        this.prompt = prompt;
        this.imageBytes = imageBytes;
    }

    public string ContentType => "application/json";
    public GeminiAuthMethod AuthMethod => GeminiAuthMethod.APIKey;
    public string OAuthAccessToken => null;

    public string GetEndpointUri(GeminiRequestMetadata metadata) =>
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-preview-image-generation:generateContent";

    public string GetUtf8EncodedData()
    {
        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = prompt },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = "image/png",
                                data = Convert.ToBase64String(imageBytes)
                            }
                        }
                    }
                }
            }
        };

        return JsonConvert.SerializeObject(payload);
    }
}
