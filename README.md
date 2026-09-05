# 🍔 大胃袋历险记 (Great Stomach Adventure)

> **大二《游戏设计与开发概论》课程期末大作业** 
> 🏆 3D 美食主题大富翁游戏

## 🎮 游戏简介
《大胃袋历险记》是一款以“美食与经营”为主题的 3D 休闲策略大富翁游戏。
玩家需要通过投掷骰子在地图上移动，消耗金币和体力来购买、建造和升级各种特色美食店铺。在多达 40 个回合的较量中，合理规划资源，利用房主特权与随机事件，击败其他 AI 竞争者，成为最终的美食大亨！

### ✨ 核心机制 (Features)
- **双资源管理**：金币（购买/升级/缴租）与体力（移动/行动限制）的平衡。
- **丰富的地块交互**：包含空地购买、店铺升级、地主特权折扣、路过消费等经典大富翁机制。
- **AI 自动对战**：支持玩家与多名具有不同独特技能的 AI 角色同台竞技。

---

## 📷 游戏截图

<p align="center">
  <img src="./Images/screenshot_1.png" width="48%" />
  <img src="./Images/screenshot_2.png" width="48%" />
</p>
<p align="center">
  <img src="./Images/screenshot_3.png" width="48%" />
  <img src="./Images/screenshot_4.png" width="48%" />
</p>

---

## 🧑‍💻 个人职责与技术实现 (My Contributions)

本项目由 **4人团队（1策划，1程序，1美术，1音乐）** 合作完成。
在本项目中，我担任**唯一的程序员**，负责了游戏底层的回合制框架设计、地图数据管理、AI 逻辑以及所有 UI 的交互开发。

### 🛠️ 技术栈
- **游戏引擎：** Unity 2020.3.30f1c1
- **编程语言：** C#
- **版本控制：** Git / GitHub (多人协作仓库)

### 🌟 核心开发工作

1. **回合制游戏流程控制 (Turn-Based Game Loop)**
   - 设计并实现了一套基于**状态机 (State Machine)** 的回合控制系统，精准管理“回合开始 -> 投掷骰子 -> 角色移动 -> 触发地块事件 -> 等待玩家操作 -> 回合结束”的完整生命周期。
   - 解决了多角色（玩家与多个AI）交替行动时的异步等待和状态切换问题，确保 40 回合流程顺畅无 Bug。

2. **3D 棋盘地图与地块数据系统 (Board & Tile System)**
   - 将地图抽象为网格数据结构，实现角色的沿路跳跃式移动。
   - 编写了高度封装的地块基类，衍生出不同类型的地块（如空地、个人资产、奖励点）。
   - 实现了复杂的资产逻辑计算，包括：所有权判定、建造/升级消耗、路过租金扣除以及房主半价折扣等。

3. **AI 对手逻辑 (AI Opponent Behavior)**
   - 为 AI 角色编写了自动化的决策系统。
   - AI 能够根据当前自身的体力、金钱状态以及踩中的地块类型，自动执行掷骰子、决定是否购买/升级店铺等操作，无需人工干预。

4. **UI 数据绑定与事件交互 (UI System)**
   - 搭建了复杂的信息展示面板，实时监听并更新玩家与 AI 的金钱、体力、状态变化。
   - 开发了动态弹窗系统，根据玩家踩中的地块类型动态生成不同的操作按钮（如“建造”、“升级”、“消费”等），并处理相应的资源扣除逻辑。

---

## 👥 开发团队 (Credits)
*本项目由以下成员共同完成（见仓库 Collaborators）：*
- **程序 (Programmer)：** [Dulst] (GitHub: [@DulstAminos](https://github.com/DulstAminos))
- **策划 (Game Designer)：** [Yang Yi](GitHub: [@MasterYi114514](https://github.com/MasterYi114514))
- **美术 (3D Artist)：** [Promisun-abc](GitHub: [@Promisun-abc](https://github.com/Promisun-abc))
- **音频 (Audio Designer)：** [wangyuze764](GitHub: [@wangyuze764](https://github.com/wangyuze764))