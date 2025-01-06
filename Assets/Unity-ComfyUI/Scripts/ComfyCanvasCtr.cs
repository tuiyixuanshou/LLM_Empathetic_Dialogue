using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ComfyCanvasCtr : MonoBehaviour
{
    [Header("References")]
    [SerializeField] ComfyPromptCtr comfyPromptCtr;

    [Header("Strength")]
    [SerializeField] KeyCode strengthIncreaseKey = KeyCode.UpArrow;
    [SerializeField] KeyCode strengthDecreaseKey = KeyCode.DownArrow;
    [SerializeField] float strengthMin = 0.1f, strengthMax = 1;
    [SerializeField] float strengthDefaultValue = 0.35f;
    [SerializeField] float strengthChangeAmount = 0.1f;
    [SerializeField] Slider strengthSlider;
    [SerializeField] Text strengthText;



    [Header("Guidance Scale")]
    [SerializeField] KeyCode guidanceScaleIncreaseKey = KeyCode.RightArrow;
    [SerializeField] KeyCode GuidanceScaleDecreaseKey = KeyCode.LeftArrow;
    [SerializeField] float guidanceScaleMin = 0.1f, guidanceScaleMax = 2;
    [SerializeField] float guidanceScaleDefaultValue = 1f;
    [SerializeField] float guidanceScaleChangeAmount = 1f;
    [SerializeField] Slider guidanceScaleSlider;
    [SerializeField] TextMeshProUGUI guidanceScaleText;



    [Header("Seed")]
    [SerializeField] KeyCode seedIncreaseKey = KeyCode.KeypadPlus;
    [SerializeField] KeyCode seedDecreaseKey = KeyCode.KeypadMinus;
    [SerializeField] int seedMin = 1, seedMax = 100000;
    [SerializeField] int seedChangeAmount = 1;
    [SerializeField] Slider seedSlider;
    [SerializeField] TextMeshProUGUI seedText;



    [Header("Random Seed")]
    [SerializeField] KeyCode toggleRandomSeedKey;
    [SerializeField] Toggle toggleRandomSeedToggle;



    [Header("Steps")]
    [SerializeField] KeyCode stepsIncreaseKey = KeyCode.PageUp;
    [SerializeField] KeyCode stepsDecreaseKey = KeyCode.PageDown;
    [SerializeField] int stepsMin = 1, stepsMax = 20;
    [SerializeField] int stepsDefaultValue = 4;
    [SerializeField] int stepsChangeAmount = 1;
    [SerializeField] Slider stepsSlider;
    [SerializeField] Text stepsText;



    [Header("Auto API Call Rate")]
    [SerializeField] KeyCode apiCallRateIncreaseKey = KeyCode.Mouse1;
    [SerializeField] KeyCode apiCallRateDecreaseKey = KeyCode.Mouse0;
    [SerializeField] float apiCallRateMin = 0.005f, apiCallRateMax = 2;
    [SerializeField] float apiCallRateDefaultValue = 0.6f;
    [SerializeField] float apiCallRateChangeAmount = 0.05f;
    [SerializeField] Slider apiCallRateSlider;
    [SerializeField] TextMeshProUGUI apiCallRateText;



    [Header("Enable Safety Checks")]
    [SerializeField] KeyCode toggleEnableSafetyChecksKey;
    [SerializeField] Toggle toggleEnableSafetyChecksToggle;



    void OnEnable()
    {
        //falApiOutputOnlyViewerWebSockets.OnRandomSeedReceived += UpdateRandomSeedText;
    }

    void OnDisable()
    {
        //falApiOutputOnlyViewerWebSockets.OnRandomSeedReceived -= UpdateRandomSeedText;
    }



/*
    void Start()
    {
        // Strength Setups
        strengthSlider.minValue = strengthMin;
        strengthSlider.maxValue = strengthMax;
        strengthSlider.value = strengthDefaultValue;
        guidanceScaleText.text = strengthDefaultValue.ToString("F2");

        // Guidance Scale Setups
        guidanceScaleSlider.minValue = guidanceScaleMin;
        guidanceScaleSlider.maxValue = guidanceScaleMax;
        guidanceScaleSlider.value = guidanceScaleDefaultValue;
        guidanceScaleText.text = guidanceScaleDefaultValue.ToString("F2");

        // Seed Setups
        seedSlider.minValue = seedMin;
        seedSlider.maxValue = seedMax;
        seedSlider.value = seedMin;
        seedText.text = seedMin.ToString();

        // Steps Setups
        stepsSlider.minValue = stepsMin;
        stepsSlider.maxValue = stepsMax;
        stepsSlider.value = stepsDefaultValue;
        stepsText.text = stepsDefaultValue.ToString();

        // Auto API Call Rate Setups
        apiCallRateSlider.minValue = apiCallRateMin;
        apiCallRateSlider.maxValue = apiCallRateMax;
        apiCallRateSlider.value = apiCallRateDefaultValue;
        apiCallRateText.text = apiCallRateDefaultValue.ToString("F3");
    }
*/


    void Update()
    {
        // si el mouse esta fuera de la ventana, no hacer nada
        if (!Input.mousePresent) return;

        // check for key presses
        ControlStrength();
        //ControlGuidanceScale();
        //ControlSeed();
        //ToggleRandomSeed();
        ControlSteps();
        //ToggleEnableSafetyChecks();   
        //ControlApiCallRate();
    }







    private void ControlStrength() // For the mouse wheel
    {
        float mouseWheelChange = Input.GetAxis("Mouse ScrollWheel");

        if (mouseWheelChange != 0)
        {
            comfyPromptCtr.comfyInputs.denoise.strength += (float)Math.Round(mouseWheelChange * strengthChangeAmount, 2);
            comfyPromptCtr.comfyInputs.denoise.strength = Mathf.Clamp(comfyPromptCtr.comfyInputs.denoise.strength, strengthMin, strengthMax);
            strengthSlider.value = comfyPromptCtr.comfyInputs.denoise.strength;
            strengthText.text = comfyPromptCtr.comfyInputs.denoise.strength.ToString("F2");
        }
    }

    public void ControlStrength(float value) // For the slider
    {
        comfyPromptCtr.comfyInputs.denoise.strength = (float)Math.Round(value, 2);
        strengthText.text = comfyPromptCtr.comfyInputs.denoise.strength.ToString("F2");
    }





/*
    private void ControlGuidanceScale() // For the keyboard
    {
        if (Input.GetKeyDown(guidanceScaleIncreaseKey))
        {
            if (falApiOutputOnlyViewerWebSockets.GuidanceScale < guidanceScaleMax)
            {
                falApiOutputOnlyViewerWebSockets.GuidanceScale += guidanceScaleChangeAmount;
                guidanceScaleSlider.value = falApiOutputOnlyViewerWebSockets.GuidanceScale;
                guidanceScaleText.text = falApiOutputOnlyViewerWebSockets.GuidanceScale.ToString("F2");
            }            
        }
        else if (Input.GetKeyDown(GuidanceScaleDecreaseKey))
        {
            if (falApiOutputOnlyViewerWebSockets.GuidanceScale > guidanceScaleMin)
            {
                falApiOutputOnlyViewerWebSockets.GuidanceScale -= guidanceScaleChangeAmount;
                guidanceScaleSlider.value = falApiOutputOnlyViewerWebSockets.GuidanceScale;
                guidanceScaleText.text = falApiOutputOnlyViewerWebSockets.GuidanceScale.ToString("F2");
            }
        }
    }
    public void ControlGuidanceScale(float value) // For the slider
    {
        falApiOutputOnlyViewerWebSockets.GuidanceScale = (float)Math.Round(value, 2);
        guidanceScaleText.text = falApiOutputOnlyViewerWebSockets.GuidanceScale.ToString("F2");
    }

*/



/*

    private void ControlSeed() // For the keyboard
    {
        if(Input.GetKeyDown(seedIncreaseKey))
        {
            if(falApiOutputOnlyViewerWebSockets.Seed < seedMax && toggleRandomSeedToggle.isOn)
            {
                falApiOutputOnlyViewerWebSockets.Seed += seedChangeAmount;
                seedSlider.value = falApiOutputOnlyViewerWebSockets.Seed;
                seedText.text = falApiOutputOnlyViewerWebSockets.Seed.ToString();
            }
            
        }
        else if(Input.GetKeyDown(seedDecreaseKey))
        {
            if(falApiOutputOnlyViewerWebSockets.Seed > seedMin && toggleRandomSeedToggle.isOn)
            {
                falApiOutputOnlyViewerWebSockets.Seed -= seedChangeAmount;
                seedSlider.value = falApiOutputOnlyViewerWebSockets.Seed;
                seedText.text = falApiOutputOnlyViewerWebSockets.Seed.ToString();
            }
        }
    }
    public void ControlSeed(float value) // For the slider
    {
        falApiOutputOnlyViewerWebSockets.Seed = (int)value;
        seedText.text = falApiOutputOnlyViewerWebSockets.Seed.ToString();
    }

*/



/*

    private void ToggleRandomSeed() // For the keyboard
    {
        if (Input.GetKeyDown(toggleRandomSeedKey))
        {
            // Cambia el estado del toggle y luego aplica la lógica
            toggleRandomSeedToggle.isOn = !toggleRandomSeedToggle.isOn;
            ApplyRandomSeedLogic();
        }
    }
    private void ApplyRandomSeedLogic()
    {
        if (toggleRandomSeedToggle.isOn)
        {
            falApiOutputOnlyViewerWebSockets.previousSeed = falApiOutputOnlyViewerWebSockets.LcmRequestParameters.seed;
            falApiOutputOnlyViewerWebSockets.IsSeedRandom(true); // Set seed to 0 to get a random seed
            seedText.text = "Random";
            seedSlider.interactable = false;
        }
        else
        {
            falApiOutputOnlyViewerWebSockets.LcmRequestParameters.seed = falApiOutputOnlyViewerWebSockets.previousSeed;
            falApiOutputOnlyViewerWebSockets.IsSeedRandom(false); // Set seed to the previous value
            seedText.text = falApiOutputOnlyViewerWebSockets.LcmRequestParameters.seed.ToString();
            seedSlider.interactable = true;
        }
    }
    public void ToggleRandomSeedPublic() // To be activated from the UI
    {
        ApplyRandomSeedLogic();
    }

    private void UpdateRandomSeedText()
    {        
        seedText.text = falApiOutputOnlyViewerWebSockets.LcmRequestParameters.seed.ToString();
    }


*/





    private void ControlSteps() // For the keyboard
    {
        if(Input.GetKeyDown(stepsIncreaseKey))
        {
            if (comfyPromptCtr.comfyInputs.steps.stepsValue < stepsMax)
            {
                comfyPromptCtr.comfyInputs.steps.stepsValue += stepsChangeAmount;
                stepsSlider.value = comfyPromptCtr.comfyInputs.steps.stepsValue;
                stepsText.text = comfyPromptCtr.comfyInputs.steps.stepsValue.ToString();
            }            
        }
        else if(Input.GetKeyDown(stepsDecreaseKey))
        {
            if (comfyPromptCtr.comfyInputs.steps.stepsValue > stepsMin)
            {
                comfyPromptCtr.comfyInputs.steps.stepsValue -= stepsChangeAmount;
                stepsSlider.value = comfyPromptCtr.comfyInputs.steps.stepsValue;
                stepsText.text = comfyPromptCtr.comfyInputs.steps.stepsValue.ToString();
            }            
        }
    }
    public void ControlSteps(float value) // For the slider
    {
        comfyPromptCtr.comfyInputs.steps.stepsValue = (int)value;
        stepsText.text = comfyPromptCtr.comfyInputs.steps.stepsValue.ToString();
    }









/*
    private void ControlApiCallRate() // For the keyboard
    {
        if (Input.GetKeyDown(apiCallRateIncreaseKey))
        {
            if (falApiOutputOnlyViewerWebSockets.MinTimeBetweenCalls < apiCallRateMax)
            {
                falApiOutputOnlyViewerWebSockets.MinTimeBetweenCalls += apiCallRateChangeAmount;
                apiCallRateSlider.value = falApiOutputOnlyViewerWebSockets.MinTimeBetweenCalls;
                apiCallRateText.text = falApiOutputOnlyViewerWebSockets.MinTimeBetweenCalls.ToString("F3");
            }
        }
        else if (Input.GetKeyDown(apiCallRateDecreaseKey))
        {
            if (falApiOutputOnlyViewerWebSockets.MinTimeBetweenCalls > apiCallRateMin)
            {
                falApiOutputOnlyViewerWebSockets.MinTimeBetweenCalls -= apiCallRateChangeAmount;
                apiCallRateSlider.value = falApiOutputOnlyViewerWebSockets.MinTimeBetweenCalls;
                apiCallRateText.text = falApiOutputOnlyViewerWebSockets.MinTimeBetweenCalls.ToString("F3");
            }
        }
    }

    public void ControlApiCallRate(float value) // For the slider
    {
        falApiOutputOnlyViewerWebSockets.MinTimeBetweenCalls = (float)Math.Round(value, 3);
        apiCallRateText.text = falApiOutputOnlyViewerWebSockets.MinTimeBetweenCalls.ToString("F3");
    }
*/







/*
    private void ToggleEnableSafetyChecks() // For the keyboard
    {
        if (Input.GetKeyDown(toggleEnableSafetyChecksKey))
        {
            OnToggleEnableSafetyChecks();
        }
    }
    private void OnToggleEnableSafetyChecks()
    {
        falApiOutputOnlyViewerWebSockets.EnableSafetyChecks = !falApiOutputOnlyViewerWebSockets.EnableSafetyChecks;
        // Cambiar el estado del toggle solo si es necesario para evitar llamadas redundantes
        if (toggleEnableSafetyChecksToggle.isOn != falApiOutputOnlyViewerWebSockets.EnableSafetyChecks)
        {
            toggleEnableSafetyChecksToggle.isOn = falApiOutputOnlyViewerWebSockets.EnableSafetyChecks;
        }
    }
    // Un manejador de eventos separado para cambios del toggle de la interfaz de usuario
    public void ToggleEnableSafetyChecksPublic() // To be activated from the UI
    {
        if (toggleEnableSafetyChecksToggle.isOn != falApiOutputOnlyViewerWebSockets.EnableSafetyChecks)
        {
            OnToggleEnableSafetyChecks();
        }
    }
    */
}
