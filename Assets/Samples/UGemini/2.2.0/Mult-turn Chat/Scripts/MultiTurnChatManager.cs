using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Uralstech.UGemini.Models;
using Uralstech.UGemini.Models.Content;
using Uralstech.UGemini.Models.Generation.Chat;

namespace Uralstech.UGemini.Samples
{
    public class MultiTurnChatManager : MonoBehaviour
    {
        [SerializeField] private bool _useBeta = true;
        [SerializeField] private Dropdown _fileType;
        [SerializeField] private InputField _chatInput;
        [SerializeField] private Transform _chatMessages;
        [SerializeField] private UIChatMessage _chatMessagePrefab;
    
        private readonly List<GeminiContent> _chatHistory = new();
        private readonly List<GeminiContentPart> _uploadedData = new();
        private GeminiContent _systemPrompt = null;
    
        private GeminiRole _senderRole = GeminiRole.User;
        private bool _settingSystemPrompt = false;


        [SerializeField] private RawImage _outputImage; // UI ��ʾͼ��
        public async void OnGenerateImageFromTextAndImage()
        {
            string prompt = _chatInput.text.Trim();
            if (string.IsNullOrEmpty(prompt))
            {
                Debug.LogError("��������ʾ��");
                return;
            }

            var imagePart = _uploadedData.FirstOrDefault(p => p.InlineData != null)?.InlineData;
            if (imagePart == null)
            {
                Debug.LogError("�����ϴ�һ��ͼ��");
                return;
            }

            byte[] imageBytes = Convert.FromBase64String(imagePart.Data);

            var request = new GeminiImageGenerationRequest(prompt, imageBytes);
            var response = await GeminiManager.Instance.Request<GeminiChatResponse>(request);

            string base64Image = response.Candidates[0].Content.Parts
                .FirstOrDefault(p => p.InlineData != null)?.InlineData?.Data;

            if (!string.IsNullOrEmpty(base64Image))
            {
                byte[] imageData = Convert.FromBase64String(base64Image);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);
                _outputImage.texture = texture;
                Debug.Log("ͼ�����ɳɹ���");
            }
            else
            {
                Debug.LogWarning("δ�յ�ͼ������");
            }
        }

        public void SetRole(int role)
        {
            if (role > (int)GeminiRole.ToolResponse)
            {
                _settingSystemPrompt |= true;
                return;
            }
    
            _senderRole = (GeminiRole)role;
        }
    
        public async void OnAddFile(string filePath)
        {
            byte[] data;
            try
            {
                data = await File.ReadAllBytesAsync(filePath);
            }
            catch (SystemException exception)
            {
                Debug.LogError($"Failed to load file: {exception.Message}");
                return;
            }
    
            _uploadedData.Add(new GeminiContentPart()
            {
                InlineData = new GeminiContentBlob()
                {
                    MimeType = (GeminiContentType)_fileType.value,
                    Data = Convert.ToBase64String(data),
                }
            });
    
            Debug.Log("Added file!");
        }
    
        public async void OnChat()
        {
            string text = _chatInput.text;
            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.LogError("Chat text should not be null or whitespace!");
                return;
            }
    
            _chatInput.text = string.Empty;
            GeminiContent addedContent;
    
            if (_settingSystemPrompt)
            {
                if (!_useBeta)
                {
                    Debug.LogError("System prompts are not yet supported in the non-beta API!");
                    return;
                }
    
                addedContent = _systemPrompt = GeminiContent.GetContent(text);
            }
            else
            {
                addedContent = GeminiContent.GetContent(text, _senderRole);
                if (_uploadedData.Count > 0)
                {
                    addedContent.Parts = addedContent.Parts.Concat(_uploadedData).ToArray();
                    _uploadedData.Clear();
                }
                
                _chatHistory.Add(addedContent);
            }
           
            AddMessage(addedContent, _settingSystemPrompt);
    
            _settingSystemPrompt = false;
            if (_chatHistory.Count == 0)
                return;
    
            GeminiChatResponse response = await GeminiManager.Instance.Request<GeminiChatResponse>(new GeminiChatRequest(GeminiModel.Gemini1_5Flash, _useBeta)
            {
                Contents = _chatHistory.ToArray(),
                SystemInstruction = _systemPrompt,
            });
            
            _chatHistory.Add(response.Candidates[0].Content);
            AddMessage(response.Candidates[0].Content);
        }
    
        private void AddMessage(GeminiContent content, bool isSystemPrompt = false)
        {
            UIChatMessage message = Instantiate(_chatMessagePrefab, _chatMessages);
            message.Setup(content, isSystemPrompt);
        }
    }
}
