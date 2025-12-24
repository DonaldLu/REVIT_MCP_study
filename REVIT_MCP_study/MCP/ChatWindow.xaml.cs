using Autodesk.Revit.UI;
using Newtonsoft.Json;
using RevitMCP.Core;
using RevitMCP.Models;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;

namespace RevitMCP
{
    public partial class ChatWindow : Window
    {
        private readonly GeminiChatService _chatService;
        private readonly UIApplication _uiApp;
        private readonly ObservableCollection<string> _messages;

        public ChatWindow(GeminiChatService chatService, UIApplication uiApp)
        {
            InitializeComponent();
            _chatService = chatService;
            _uiApp = uiApp;
            _messages = new ObservableCollection<string>();
            ChatHistory.ItemsSource = _messages;

            _messages.Add("🤖 AI 助手已就緒。請輸入您的問題來控制 Revit。");
            _messages.Add("💡 例如：請建立一道長度5米的牆、高度5米, 起點(0,0)");
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userInput = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(userInput)) return;

            // 顯示用戶訊息
            _messages.Add($"👤 您: {userInput}");
            InputBox.Clear();

            // 獲取 AI 回應
            SendButton.IsEnabled = false;
            SendButton.Content = "處理中...";

            try
            {
                string context = @"您是 Revit BIM 專家助手。
                                    請務必僅以 JSON 格式回覆，不要包含額外的文字描述。

                                    輸出的 JSON 必須嚴格遵守以下格式：
                                    {
                                      ""CommandName"": ""指令名稱"",
                                      ""Parameters"": {
                                        ""startX"": 0,
                                        ""startY"": 0,
                                        ""endX"": 0,
                                        ""endY"": 0,
                                        ""height"": 0
                                      },
                                      ""RequestId"": ""自動生成的隨機ID""
                                    }

                                    可用指令：
                                    - create_wall: 必須提供 startX, startY, endX, endY, height (單位皆為 mm)。
                                    - get_project_info: 無需參數。

                                    請注意：欄位首字母必須大寫（CommandName, Parameters, RequestId）。";

                string response = await _chatService.ChatAsync(userInput, context);
                _messages.Add($"🤖 AI: {response}");

                // 如果 AI 建議執行操作，可以在這裡添加自動執行邏輯, 自動執行邏輯：從回覆中提取 JSON 指令                
                ExecuteRevitCommandIfFound(response);
            }
            catch (Exception ex)
            {
                _messages.Add($"❌ 錯誤: {ex.Message}");
            }
            finally
            {
                SendButton.IsEnabled = true;
                SendButton.Content = "傳送";
            }
        }
        /// <summary>
        /// 解析 AI 回傳的文字，尋找並執行 Revit 命令
        /// </summary>
        private void ExecuteRevitCommandIfFound(string response)
        {
            try
            {
                // 💡 使用正則表達式提取 ```json ... ``` 之間的內容
                var match = Regex.Match(response, @"```json\s*(\{.*?\})\s*```", RegexOptions.Singleline);

                if (match.Success)
                {
                    string jsonContent = match.Groups[1].Value;
                    var request = JsonConvert.DeserializeObject<RevitCommandRequest>(jsonContent);

                    if (request != null)
                    {
                        // 透過已有的 ExternalEventManager 執行命令 
                        ExternalEventManager.Instance.ExecuteCommand((uiApp) =>
                        {
                            var executor = new CommandExecutor(uiApp);
                            var result = executor.ExecuteCommand(request);

                            // 回到 UI 執行緒更新訊息
                            Dispatcher.Invoke(() => {
                                if (result.Success)
                                {
                                    _messages.Add($"✅ 成功執行指令: {request.CommandName}");
                                }
                                else
                                {
                                    // 修正處：使用 result.Error 而非 result.Message 
                                    _messages.Add($"⚠️ 執行失敗: {result.Error}");
                                }
                            });
                        });
                    }
                }
                else if (response.Contains("create_") || response.Contains("Command"))
                {
                    // 提示使用者 AI 格式不正確
                    _messages.Add("💡 偵測到指令意圖但格式不正確，請嘗試要求 AI：'請用 JSON 格式回覆'");
                }
            }
            catch (Exception ex)
            {
                _messages.Add($"🔍 解析指令時發生錯誤: {ex.Message}");
            }
        }
    }
}