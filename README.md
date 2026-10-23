# lxShopping
用 **ASP.NET MVC** 写的购物网站，前台后台都有。前台：逛商品、加购物车、下单、注册登录、管收货地址；后台：管商品、订单、用户。数据存 SQL Server，统计图用的 ECharts。

```
浏览器
 ├─ 前台：Home（商品）/ Cart（购物车）/ Order（下单）/ Member（登录注册）
 └─ 后台：Admin（商品 / 订单 / 用户）
        │
        ▼
  Controllers + Entity Framework ──► SQL Server
```

## 快速开始

### 1. 建数据库
执行 `LXShopping/Database/LXShopping.sql`；或者包管理器控制台里跑 `Update-Database`（项目带了 EF Migrations，二选一）。

### 2. 改连接字符串
`Web.config` 里改成你的数据库。

### 3. 跑起来
VS 打开 `LXShopping.sln`，F5 起 IIS Express。后台入口是 `/Admin/Login`，后台账号数据库里没有现成的，自己加一条。

> IIS Express 的端口每台机器不一样，以 VS 弹出来那个地址为准。

## 目录

```
LXShopping/
├─ Controllers/     # Home / Cart / Order / Member / Admin…
├─ Models/          # 实体类 + DbContext
├─ Views/           # 前台一套 Layout，后台一套
├─ Content/         # 样式、图标、echarts
├─ Scripts/         # jQuery 这些
├─ Database/        # LXShopping.sql
├─ Migrations/      # EF 迁移
└─ App_Data/        # 注册邮件的模板
```

## 常见问题

- **起不来，报数据库错**：先看连接字符串，再看脚本跑没跑。
- **后台进不去**：数据库里得有 Admin 账号，注册功能是给前台会员用的。
- **首页统计图不显示**：ECharts 在 `Content/` 里，发布的时候别漏静态文件。
