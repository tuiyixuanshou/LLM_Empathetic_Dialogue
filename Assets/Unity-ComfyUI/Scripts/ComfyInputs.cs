using System;
using UnityEngine;


[Serializable]
public class ComfyInputs
{
    public SDPositivePrompt positivePrompt;
    public SDNegativePrompt negativePrompt;
    public SDSeed seed;
    public SDLoadImage loadImage;
    public SDDenoise denoise;
    public SDSteps steps;
}


[Serializable]
public class SDPositivePrompt
{
    public string inputID = "PromptValue";
    public string prompt = "cat";

    public void SetPrompt(string prompt)
    {
        this.prompt = prompt;
    }
}

[Serializable]
public class SDNegativePrompt
{
    public string inputID = "Nprompt";
    public string prompt = "watermark";

    public void SetPrompt(string prompt)
    {
        this.prompt = prompt;
    }
}

[Serializable]
public class SDSeed
{
    public string inputID = "SeedValue";
    public int seedValue = 0;
    public SeedType seedType = SeedType.Randomized;
    public enum SeedType
    {
        Randomized,
        Fixed,
        Incremental,
        Decremental
    }

    public int GetSeed()
    {
        switch (seedType)
        {
            case SeedType.Randomized:
                return seedValue = UnityEngine.Random.Range(0, 1000000);
            case SeedType.Fixed:
                return seedValue;
            case SeedType.Incremental:
                return seedValue++;
            case SeedType.Decremental:
                return seedValue--;
            default:
                return seedValue = UnityEngine.Random.Range(0, 1000000);
        }
    }
}

[Serializable]
public class SDLoadImage
{
    public string inputID = "LoadImgBase64";
    public Texture2D image;

    [NonSerialized]
    public string imageBase64;

    public string ConvertToBase64(Texture2D image)
    {
        byte[] bytes = image.EncodeToPNG();
        imageBase64 = Convert.ToBase64String(bytes);
        return imageBase64;
    }
}

[Serializable]
public class SDDenoise
{
    public string inputID = "DenoiseStrength";

    [Range(0.1f, 1f)]
    public float strength = 0.5f;
}

[Serializable]
public class SDSteps
{
    public string inputID = "StepsValue";
    public int stepsValue = 4;
}