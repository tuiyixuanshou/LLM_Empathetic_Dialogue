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
       public static string ListToString<T>( //泛型类型参数
       List<T> list,
       string separator = ", ",
       System.Func<T, string> formatFunc = null // 可选：自定义元素格式化方法.接受一个类型为 T 的参数，并返回一个 string 类型的结果。
        )
       {
            if (list == null) return "null";
            if (list.Count == 0) return "[]";

            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            for (int i = 0; i < list.Count; i++)
            {
                T item = list[i];

                // 优先使用自定义格式化方法
                if (formatFunc != null)
                {
                    sb.Append(formatFunc(item));
                }
                else
                {
                    // 默认行为：反射获取字段和属性值
                    sb.Append(ConvertObjectToString(item));
                }
                if (i < list.Count - 1) sb.Append(separator);
            }
            sb.Append("]");
            return sb.ToString();
        }

        // 递归转换单个对象为详细字符串
        private static string ConvertObjectToString(object obj, int indentLevel = 0)
        {
            if (obj == null) return "null";

            // 如果是基本类型直接返回
            if (obj.GetType().IsPrimitive || obj is string)
            {
                return $"{obj}";
            }

            StringBuilder sb = new StringBuilder();
            string indent = new string(' ', indentLevel * 2); // 缩进控制

            sb.AppendLine("{");

            // 反射获取所有公共字段和属性
            FieldInfo[] fields = obj.GetType().GetFields();
            PropertyInfo[] properties = obj.GetType().GetProperties();

            // 合并字段和属性
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

        // 处理值的递归转换
        private static string ConvertValueToString(object value, int indentLevel)
        {
            if (value == null) return "null";

            // 如果是集合类型（如List、数组）
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

            // 递归处理嵌套对象
            return ConvertObjectToString(value, indentLevel);
        }

    }
    public class PostWeb : MonoBehaviour
    {
        public static bool isAIRun; //是否需要一问一答
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
                // 先用平衡组提取最外层 [...]
                string pattern = @"\[(?:[^[\]]+|(?<open>\[)|(?<-open>\]))+(?(open)(?!))\]";
                Match match = Regex.Match(rawText, pattern, RegexOptions.Singleline);
                if (!match.Success) return null;
                Debug.Log("提纯：" + match.Value);
                // 再解析提取后的 JSON
                return match.Value;
            }
            catch (Exception ex)
            {
                Debug.LogError($"解析失败：{ex.Message}");
                return null;
            }
        }
        #region 回复解析实例
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
                Debug.LogError("文件不存在：" + fullPath);
                return string.Empty;
            }
        }

        
    }
}

