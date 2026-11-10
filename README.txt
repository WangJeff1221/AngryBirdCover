angry bird space cover — 独立 Unity2D 课堂项目

Unity 版本：6000.3.7f1。
打开这个独立项目文件夹，不要打开上一层混合了其他作业的项目。

Unity Hub → Add project from disk → 选择 AngryBirdCover 文件夹。
打开 Assets/Cover1/Scenes/01_PigBang_1-1.unity 后按 Play。
场景里的星球、猪、木板、冰架、砖块在编辑态就存在，可以拖动、旋转和调整。

四个场景依次参考 Pig Bang 1-1、1-3、1-10、1-14。
完整来源、实现差异、数学解释、diagram 脚本对应：
Documentation/课堂讲解与原版对照.txt

左键拖鸟后松开发射；飞行时再点左键触发分裂/爆炸。
R 重试；1–4 切关；V 全景/跟随；N 提前装下一只；Enter 胜利后继续。
没有文字 HUD，弹弓旁的备用鸟显示未装填的鸟。

Assets/Cover1/Scripts 按 Aim、Flight、Props、Camera、Game、Effects 分类。
本项目的使用方法与参考来源见 README.md。
Library、Temp、Logs、Builds 是生成文件，不需要随源码压缩包分发。
Cover 菜单可以打开首关、构建 Windows 和 WebGL。

这是独立项目，不包含其他课程游戏的 Resources。

角色和特效 Prefab：Assets/Cover1/Prefabs。
运行时对象清单与课堂对应：Documentation/Prefab与运行时对象.txt。
