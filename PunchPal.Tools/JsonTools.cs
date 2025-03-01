using System.Collections.Generic;
using System.Text;

namespace PunchPal.Tools
{
    public class JsonTools
    {
        public static List<object> ParsePath(string path)
        {
            List<object> result = new List<object>();
            StringBuilder currentPart = new StringBuilder();
            bool inIndex = false;    // 是否在索引中（[...]）
            bool inString = false;   // 是否在字符串中（"..."）
            char quoteChar = '\0';   // 当前引号类型（"或'）

            foreach (char c in path)
            {
                if (inIndex)
                {
                    if (inString)
                    {
                        if (c == quoteChar) // 结束字符串索引
                        {
                            inString = false;
                            result.Add(currentPart.ToString());
                            currentPart.Clear();
                        }
                        else // 字符串内容
                        {
                            currentPart.Append(c);
                        }
                    }
                    else
                    {
                        if (c == ']') // 结束索引
                        {
                            inIndex = false;
                            string content = currentPart.ToString().Trim();
                            if (int.TryParse(content, out int num))
                            {
                                result.Add(num);
                            }
                            else
                            {
                                result.Add(content);
                            }
                            currentPart.Clear();
                        }
                        else if (c == '"' || c == '\'') // 开始字符串索引
                        {
                            inString = true;
                            quoteChar = c;
                            currentPart.Clear();
                        }
                        else // 非字符串索引内容（如数字）
                        {
                            currentPart.Append(c);
                        }
                    }
                }
                else
                {
                    if (c == '[') // 进入索引
                    {
                        if (currentPart.Length > 0)
                        {
                            result.Add(currentPart.ToString());
                            currentPart.Clear();
                        }
                        inIndex = true;
                    }
                    else if (c == '.') // 分隔属性
                    {
                        if (currentPart.Length > 0)
                        {
                            result.Add(currentPart.ToString());
                            currentPart.Clear();
                        }
                    }
                    else // 普通字符
                    {
                        currentPart.Append(c);
                    }
                }
            }

            // 处理最后未提交的部分
            if (currentPart.Length > 0)
            {
                result.Add(currentPart.ToString());
            }

            return result;
        }
    }
}
