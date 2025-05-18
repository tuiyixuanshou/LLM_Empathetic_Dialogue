using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace MyUtilities
{
    public class ListString : MonoBehaviour
    {
       public static string ListToString<T>( //�������Ͳ���
       List<T> list,
       string separator = ", ",
       System.Func<T, string> formatFunc = null // ��ѡ���Զ���Ԫ�ظ�ʽ������.����һ������Ϊ T �Ĳ�����������һ�� string ���͵Ľ����
        )
       {
            if (list == null) return "null";
            if (list.Count == 0) return "[]";

            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            for (int i = 0; i < list.Count; i++)
            {
                T item = list[i];

                // ����ʹ���Զ����ʽ������
                if (formatFunc != null)
                {
                    sb.Append(formatFunc(item));
                }
                else
                {
                    // Ĭ����Ϊ�������ȡ�ֶκ�����ֵ
                    sb.Append(ConvertObjectToString(item));
                }
                if (i < list.Count - 1) sb.Append(separator);
            }
            sb.Append("]");
            return sb.ToString();
        }

        // �ݹ�ת����������Ϊ��ϸ�ַ���
        private static string ConvertObjectToString(object obj, int indentLevel = 0)
        {
            if (obj == null) return "null";

            // ����ǻ�������ֱ�ӷ���
            if (obj.GetType().IsPrimitive || obj is string)
            {
                return $"{obj}";
            }

            StringBuilder sb = new StringBuilder();
            string indent = new string(' ', indentLevel * 2); // ��������

            sb.AppendLine("{");

            // �����ȡ���й����ֶκ�����
            FieldInfo[] fields = obj.GetType().GetFields();
            PropertyInfo[] properties = obj.GetType().GetProperties();

            // �ϲ��ֶκ�����
            foreach (var field in fields)
            {
                object value = field.GetValue(obj);
                sb.AppendLine($"{indent}  {field.Name} = {ConvertValueToString(value, indentLevel + 1)},");
            }

            foreach (var prop in properties)
            {
                if (prop.CanRead)
                {
                    object value = prop.GetValue(obj);
                    sb.AppendLine($"{indent}  {prop.Name} = {ConvertValueToString(value, indentLevel + 1)},");
                }
            }

            sb.Append($"{indent}}}");
            return sb.ToString();
        }

        // ����ֵ�ĵݹ�ת��
        private static string ConvertValueToString(object value, int indentLevel)
        {
            if (value == null) return "null";

            // ����Ǽ������ͣ���List�����飩
            if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[");

                bool first = true;
                foreach (var item in enumerable)
                {
                    if (!first) sb.Append(", ");
                    sb.Append(ConvertObjectToString(item, indentLevel));
                    first = false;
                }

                sb.Append("]");
                return sb.ToString();
            }

            // �ݹ鴦��Ƕ�׶���
            return ConvertObjectToString(value, indentLevel);
        }

    }
    public class PostWeb : MonoBehaviour
    {
        public static bool isAIRun; //�Ƿ���Ҫһ��һ��
        public static IEnumerator postRequest(string url, string api, string json, Action<string> callback)
        {
            PostWeb.isAIRun = true;
            using (var uwr = new UnityWebRequest(url, "POST"))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
                uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                uwr.SetRequestHeader("Content-Type", "application/json");
                uwr.SetRequestHeader("Authorization", "Bearer " + api);

                //Send the request then wait here until it returns
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Error While Sending: " + uwr.error);
                    Debug.Log("Full respond:" + uwr.downloadHandler.text);
                }
                else
                {
                    Debug.Log("Received: " + uwr.downloadHandler.text);
                    string response = uwr.downloadHandler.text;
                    ApiSilicion apiResponse = JsonUtility.FromJson<ApiSilicion>(response);
                    string responseJson = apiResponse.choices[0].message.content;
                    Debug.Log(responseJson);
                    callback(responseJson);
                }
            }
            PostWeb.isAIRun = false;
        }

        public static string JsonPatch(string rawText)
        {
            try
            {
                // ����ƽ������ȡ����� [...]
                string pattern = @"\[(?:[^[\]]+|(?<open>\[)|(?<-open>\]))+(?(open)(?!))\]";
                Match match = Regex.Match(rawText, pattern, RegexOptions.Singleline);
                if (!match.Success) return null;
                Debug.Log("�ᴿ��" + match.Value);
                // �ٽ�����ȡ��� JSON
                return match.Value;
            }
            catch (Exception ex)
            {
                Debug.LogError($"����ʧ�ܣ�{ex.Message}");
                return null;
            }
        }
        #region �ظ�����ʵ��
        [System.Serializable]
        public class ApiSilicion
        {
            public List<Choice> choices;
        }
        [System.Serializable]
        public class Choice
        {
            public Message message;
        }
        [System.Serializable]
        public class Message
        {
            public string role;
            public string content;
        }
        #endregion
    }

    public class tools : MonoBehaviour
    {
        public static string ReadFile(string FilePath,string FileName)
        {
            string fullPath = Path.Combine(FilePath, FileName);
            if (File.Exists(fullPath))
            {
                return File.ReadAllText(fullPath);
            }
            else
            {
                Debug.LogError("�ļ������ڣ�" + fullPath);
                return string.Empty;
            }
        }

        
    }
}

