using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitMCP.Core;
using System;

namespace RevitMCP.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class ChatCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // 從設定中讀取 API Key
                var apiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY");

                if (string.IsNullOrEmpty(apiKey))
                {
                    TaskDialog.Show("設定錯誤",
                        "請設定環境變數 GEMINI_API_KEY\n\n" +
                        "在 Windows 中：\n" +
                        "1. 按 Win + Pause\n" +
                        "2. 進階系統設定\n" +
                        "3. 環境變數\n" +
                        "4. 新增：GEMINI_API_KEY = AIzaSyCJniuuHoAYlQAusVWuKCzZyJdWWoXgemE");
                    return Result.Failed;
                }

                // 建立聊天服務
                var chatService = new GeminiChatService(apiKey);

                // 開啟對話視窗
                var chatWindow = new ChatWindow(chatService, commandData.Application);
                chatWindow.Show();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("錯誤", $"開啟 AI Chat 失敗: {ex.Message}");
                return Result.Failed;
            }
        }
    }
}