# 牆體防火性能視覺化 - 執行指引

## 方式 A：使用 Claude Desktop 或 Gemini CLI（推薦）

### 步驟 1：確認 Revit MCP 服務已啟動
1. 開啟 Revit 2024
2. 點擊「MCP Tools」→「MCP 服務 (開/關)」
3. 確認看到「WebSocket 伺服器已啟動，監聽: localhost:8765」

### 步驟 2：在 AI 工具中執行指令

**直接對話指令：**
```
請執行以下操作：
1. 取得當前視圖
2. 查詢所有牆體
3. 根據牆體的「防火防煙性能」參數值，用不同顏色標記：
   - 2小時：深綠色
   - 1小時：黃色
   - 無防火：藍色
   - 未設定：紫色
4. 產生統計報告
```

**或使用準備好的腳本：**
```
請執行專案中的腳本：
c:\Users\User\Desktop\REVIT MCP\MCP-Server\classify_walls_by_fire_rating.js
```

---

## 方式 B：手動逐步執行（適合學習）

### 步驟 1：取得當前視圖
```javascript
const view = await get_active_view();
```

### 步驟 2：查詢所有牆體
```javascript
const walls = await query_elements({
    category: "Walls",
    viewId: view.Id
});
```

### 步驟 3：分析第一面牆的參數
```javascript
const wallInfo = await get_element_info({ 
    elementId: walls.Elements[0].ElementId 
});

// 查看所有參數
console.log(wallInfo.Parameters);

// 找到防火相關參數
const fireParam = wallInfo.Parameters.find(p => 
    p.Name.includes("防火") || p.Name.includes("Fire")
);
console.log(fireParam);
```

### 步驟 4：應用顏色覆寫（測試單個元素）
```javascript
// 假設防火時效為 "2小時"，標記為綠色
await override_element_graphics({
    elementId: walls.Elements[0].ElementId,
    viewId: view.Id,
    surfaceFillColor: { r: 0, g: 180, b: 0 },
    transparency: 20
});
```

### 步驟 5：批次處理所有牆體
```javascript
for (const wall of walls.Elements) {
    const info = await get_element_info({ elementId: wall.ElementId });
    
    // 找到防火參數
    const fireParam = info.Parameters.find(p => 
        p.Name === "防火防煙性能" || 
        p.Name === "防火時效" ||
        p.Name === "Fire Rating"
    );
    
    // 決定顏色
    let color;
    if (fireParam?.Value === "2小時") {
        color = { r: 0, g: 180, b: 0 };  // 綠色
    } else if (fireParam?.Value === "1小時") {
        color = { r: 255, g: 255, b: 0 };  // 黃色
    } else if (fireParam?.Value === "無防火") {
        color = { r: 100, g: 150, b: 255 };  // 藍色
    } else {
        color = { r: 200, g: 0, b: 200 };  // 紫色（未設定）
    }
    
    // 應用覆寫
    await override_element_graphics({
        elementId: wall.ElementId,
        viewId: view.Id,
        surfaceFillColor: color,
        transparency: 30
    });
}
```

---

## 清除顏色標記

執行完成後，如果要清除所有顏色覆寫：

```javascript
// 取得所有牆體 ID
const wallIds = walls.Elements.map(w => w.ElementId);

// 清除覆寫
await clear_element_override({
    elementIds: wallIds,
    viewId: view.Id
});
```

---

## 預期結果

在 Revit 視圖中，您應該會看到：
- 🟢 綠色牆體：2小時防火
- 🟡 黃色牆體：1小時防火
- 🔵 藍色牆體：無防火
- 🟣 紫色牆體：未設定參數

---

## 故障排除

### 問題 1：找不到參數
**症狀**：所有牆都顯示為紫色（未設定）

**解決方法**：
1. 先檢查一面牆的所有參數：
```javascript
const info = await get_element_info({ elementId: <某面牆的ID> });
console.log(info.Parameters.map(p => p.Name));
```
2. 找到正確的參數名稱
3. 修改腳本中的 `PARAMETER_NAMES` 陣列

### 問題 2：顏色沒有顯示
**可能原因**：
- Revit MCP 服務未啟動
- 視圖的視覺樣式設定不支援顏色覆寫

**解決方法**：
- 確認 MCP 服務運行中
- 在 Revit 中切換視圖樣式為「著色」或「一致色彩」

### 問題 3：部分牆體沒有標記
**可能原因**：
- 牆體不在當前視圖範圍內
- 牆體被裁切參考面遮蔽

**解決方法**：
- 檢查視圖範圍設定
- 嘗試在其他視圖執行

---

## 注意事項

⚠️ **視圖專用性**：此操作只影響當前視圖，不影響其他視圖
⚠️ **可逆操作**：隨時可以使用 `clear_element_override` 清除
⚠️ **不改變屬性**：只是視覺化標記，不改變牆體本身的屬性
