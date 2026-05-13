# 分区模块API接口定义文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | CategoryService（分区服务） |
| 服务地址 | http://category-service:5008 |
| API版本 | v1.0 |
| 创建时间 | 2026-05-12 |

---

## 2. 模块概述

### 2.1 服务职责

- 视频分区管理（10个主分区）
- 分区层级管理（主分区 + 子分区）
- 分区视频查询

---

## 3. API接口列表

| 序号 | 接口名称 | HTTP方法 | 路径 | 权限 |
|------|---------|---------|------|------|
| 1 | 获取分区列表 | GET | /api/category/categories | 公开 |
| 2 | 获取分区视频 | GET | /api/category/categories/{id}/videos | 公开 |

**总计：2个API接口**

---

## 4. 分区列表接口

### 4.1 获取分区列表

#### 基本信息

```
GET /api/category/categories?parentId={uuid}&level={int}
权限：公开
```

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| parentId | UUID | ❌ | 父分区ID（获取子分区） |
| level | int | ❌ | 分区层级（1=主分区，2=子分区） |

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "...",
      "name": "animation",
      "displayName": "动画",
      "iconUrl": "icons/animation.png",
      "description": "动画分区",
      "parentId": null,
      "level": 1,
      "sortOrder": 1,
      "videoCount": 50000,
      "isActive": true,
      "children": [
        {
          "id": "...",
          "name": "anime",
          "displayName": "动漫",
          "videoCount": 30000,
          "level": 2
        }
      ]
    },
    {
      "id": "...",
      "name": "game",
      "displayName": "游戏",
      "level": 1,
      "sortOrder": 2
    }
    // ... 10个主分区
  ],
  "totalCount": 10
}
```

---

## 5. 分区视频接口

### 5.1 获取分区视频

#### 基本信息

```
GET /api/category/categories/{id}/videos?maxResultCount={int}&skipCount={int}&sortOrder={string}
权限：公开
```

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| maxResultCount | int | ❌ | 返回数量（默认20） |
| skipCount | int | ❌ | 跳过数量（默认0） |
| sortOrder | string | ❌ | 排序（hot/new，默认hot） |

#### 成功响应（200 OK）

```json
{
  "categoryId": "...",
  "categoryName": "游戏",
  "items": [
    {
      "id": "...",
      "title": "视频标题",
      "coverImageUrl": "...",
      "totalViews": 10000,
      "publishTime": "..."
    }
  ],
  "totalCount": 500
}
```

---

## 6. 10个主分区数据

| 分区代码 | 分区名称 | 排序 |
|---------|---------|------|
| animation | 动画 | 1 |
| game | 游戏 | 2 |
| music | 音乐 | 3 |
| dance | 舞蹈 | 4 |
| knowledge | 知识 | 5 |
| tech | 科技 | 6 |
| life | 生活 | 7 |
| food | 美食 | 8 |
| fashion | 时尚 | 9 |
| entertainment | 娱乐 | 10 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 分区模块API文档完成