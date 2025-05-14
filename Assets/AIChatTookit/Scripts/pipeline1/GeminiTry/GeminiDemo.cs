using UnityEngine;
using System.Threading.Tasks;
using Uralstech.UGemini;
using Uralstech.UGemini.Models.Generation;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using Uralstech.UGemini.Models.Content;
using System.Linq;
using Uralstech.UGemini.Models.Generation.Chat;

public class ImageGenerationExample : MonoBehaviour
{
    private readonly List<GeminiContentPart> _uploadedData = new();
    
    //public string model = "models/gemini-2.0-flash-preview-image-generation";

    //public List<GeminiContent> contents = new();

    //public string ContentType => "application/json";

    //public GeminiAuthMethod AuthMethod => GeminiAuthMethod.APIKey;

    //public string OAuthAccessToken => null;

    //public string GetEndpointUri(GeminiRequestMetadata metadata)
    //{
    //    return $"{GeminiManager.ProductionApiUri}/{model}:generateContent";
    //}

    //public string GetUtf8EncodedData()
    //{
    //    return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
    //    {
    //        contents = contents,
    //        generationConfig = new
    //        {
    //            temperature = 0.7
    //        },
    //        safetySettings = new[]
    //        {
    //            new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_LOW_AND_ABOVE" },
    //            new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
    //        },
    //        responseMimeType = "image/png"
    //    })));
    //}
}
