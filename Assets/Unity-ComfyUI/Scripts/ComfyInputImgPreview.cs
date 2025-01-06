using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ComfyInputImgPreview : MonoBehaviour
{
    public ComfyPromptCtr comfyPromptCtr;
    public RawImage rawImage;



    /*

        private void OnEnable()
        {
            if (!comfyPromptCtr)
                return;

            comfyPromptCtr.OnGeneratePrompt += UpdateImagePreview;
        }

        private void OnDisable()
        {
            if (!comfyPromptCtr)
                return;

            comfyPromptCtr.OnGeneratePrompt -= UpdateImagePreview;
        }

    */

    private void Start()
    {
        UpdateImagePreview();
    }

    public void UpdateImagePreview()
    {
        if (!comfyPromptCtr || !comfyPromptCtr.comfyInputs.loadImage.image || !rawImage)
            return;

        DisplayImage(comfyPromptCtr.comfyInputs.loadImage.image);
    }


    public void DisplayImage(Texture2D preview)
    {
        rawImage.texture = preview;
    }


    [MenuItem("Tools/Load Image")]
    public void LoadImage()
    {
        if (!comfyPromptCtr || !comfyPromptCtr.comfyInputs.loadImage.image || !rawImage)
            return;

        //EditorUtility.OpenFilePanel显示一个文件选择对话框
        //"Load Image"：文件选择面板的标题。
        //""：初始打开路径（为空表示默认打开项目路径）。
        //"png,jpg,jpeg"：允许选择的文件类型（扩展名）。
        string path = EditorUtility.OpenFilePanel("Load Image", "", "png,jpg,jpeg");

        //确保用户确实选择了一个文件
        if (path.Length != 0)
        {
            //读取选中的文件内容，并将其转换为字节数组
            var fileContent = System.IO.File.ReadAllBytes(path);

            if (comfyPromptCtr.comfyInputs.loadImage.image == null)
            {
                comfyPromptCtr.comfyInputs.loadImage.image = new Texture2D(2, 2);
            }

            comfyPromptCtr.comfyInputs.loadImage.image.LoadImage(fileContent);

            DisplayImage(comfyPromptCtr.comfyInputs.loadImage.image);
        }
    }


}
