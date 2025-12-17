# Antigravity Copilot Chat 執行指令

## 在 Antigravity IDE 中執行此任務

### 步驟 1：開啟 Copilot Chat
按快捷鍵：`Ctrl + Shift + I`

### 步驟 2：複製並貼上以下指令

```
請幫我執行以下完整的牆體防火性能視覺化任務：

步驟 1: 取得當前視圖
const view = await get_active_view();
回報視圖名稱和 ID。

步驟 2: 查詢所有牆體
const walls = await query_elements({ category: "Walls", viewId: view.Id });
回報找到多少面牆。

步驟 3: 分析每面牆的防火防煙性能參數
建立一個統計物件來記錄分布情況。
對每面牆執行 get_element_info，查找以下參數名稱之一：
- "防火防煙性能"
- "防火時效"
- "Fire Rating"
- "FireRating"

步驟 4: 根據參數值應用顏色覆寫
使用以下顏色映射：
- 2小時防火 → RGB(0, 180, 0) 透明度 20%
- 1.5小時防火 → RGB(100, 220, 100) 透明度 30%
- 1小時防火 → RGB(255, 255, 0) 透明度 30%
- 0.5小時防火 → RGB(255, 165, 0) 透明度 30%
- 無防火 → RGB(100, 150, 255) 透明度 40%
- 未設定 → RGB(200, 0, 200) 透明度 50%

使用 override_element_graphics 工具應用顏色。

步驟 5: 產生報告
回報：
- 總牆體數量
- 各防火等級的數量和百分比
- 顏色映射表
- 清除覆寫的指令

請執行並顯示詳細過程。
```

### 預期結果
Copilot Chat 會執行這些步驟，並在 Revit 視圖中顯示不同顏色的牆體標記。

### 如果需要清除顏色
在 Copilot Chat 中輸入：
```
請清除剛才對所有牆體的顏色覆寫
```
