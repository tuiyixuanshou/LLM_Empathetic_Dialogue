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

        //EditorUtility.OpenFilePanel��ʾһ���ļ�ѡ��Ի���
        //"Load Image"���ļ�ѡ�����ı��⡣
        //""����ʼ��·����Ϊ�ձ�ʾĬ�ϴ���Ŀ·������
        //"png,jpg,jpeg"������ѡ����ļ����ͣ���չ������
        string path = EditorUtility.OpenFilePanel("Load Image", "", "png,jpg,jpeg");

        //ȷ���û�ȷʵѡ����һ���ļ�
        if (path.Length != 0)
        {
            //��ȡѡ�е��ļ����ݣ�������ת��Ϊ�ֽ�����
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
