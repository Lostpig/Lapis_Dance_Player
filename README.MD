### Lapis_Dance_Player
### Lapis Re:LiGHTs 模型查看器

---
### 注意点
必须用OpenGL模式运行，否则着色器会出错，导致渲染错误  

### 所需资源
依赖游戏的 AssetBundles, manifest.json, SoundBank  
  
manifest.json: 需要将游戏的manifest.xml解密,并转为json  
AssetBundles: 游戏的部分AssetBundle是加密的,需要提前解密;AssetBundles的目录结构不可修改  
SoundBank: 目前不支持wwise,需要提取出wav文件,提取后的目录结构为`原bnk文件名/(wwise number).wav`,如`MUSIC_0001/413274000.wav`  
> 如果提取为其他格式，可以在`SoundBankLoader.LoadAudioClip`里修改相关代码  
  
解包/提取方面的更多详情请参考[@bromine0x23](https://github.com/bromine0x23)的[Lapis Re:LiGHTs 资源挖掘备忘](https://bromine0x23.github.io/lapis-datamine-notes/lapis-datamine-notes.html)  
  
---

### 配置项  
#### Config/config.json 基本配置文件
```js
{
  "manifest": "C:/lapis_relights/manifest.json",      // manifest.json 文件路径
  "assetbundles": "C:/lapis_relights/AssetBundles",   // AssetBundles 资源所在目录
  "soundbanks": "C:/lapis_relights/SoundBanks"        // SoundBank 资源所在目录
}
```

#### Config/character.json 角色模型配置文件
> 一个完整的角色模型由 头部，身体 + 各个配件组成  
> 其中一个角色的头部是固定的，在该角色的human.bdl中  
> 身体分为两种，一种是通用模型，读取后要根据human.bdl的ScaleSettings信息调整比例再与头部组合  
> 一种是专用模型，读取后可直接与头部组合  
> 配件目前只完成了Skinning类型的，Attach类型的不会跟着角色动
```js
{
  "characters": [
    {
      "name": "tiara",                      // 角色全名,与资源读取相关,25个角色固定
      "shortName": "tia",                   // 角色名缩写,与资源读取相关,25个角色固定
      "label": "ティアラ / 缇亚拉",         // 显示名称,随意设置 
      "actors": [                           // 该角色配置下的可用模型组合
        {
          "name": "Tiara UR002",            // 名称,随意设置,但同一个角色下不可重复
          "body": "Actors/Avatars/TIA/Ava_Tia_Ur001/Ava_Tia_Suit_Ur001/Ava_Tia_Suit_Ur001", // 身体的asset
          "eqiups": [ // 配件assets
            "Actors/Avatars/TIA/Ava_Tia_Ur001/Ava_Tia_Hat_Ur001/Ava_Tia_Hat_Ur001"
          ]
        }
      ]
    }
  ]
}
```

#### Config/dance.json 舞蹈动作/时间轴资源配置文件
> 基于AR版舞蹈的舞蹈查看器资源配置  
```js
{
  "dances": [
    {
      "name": "LiGHTs - Your Lights",         // 显示舞蹈名称,随意设置
      "id": "MUSIC_0002"                      // 该舞蹈的ID,为固定值
      "music": "1017980350"                   // 由于当前还不支持wwise,需要人工填写从soundbank中解包出的音乐文件名
    }
  ]
}
```

---

### RoadMap
> 只是列一下，不一定能完成
+ [✓] 模型查看
+ [✓] 口型控制模块
+ [✓] 眨眼控制模块
+ 表情控制模块
+ 动作控制模块
+ [✓] 舞蹈播放器
+ 物理碰撞系统
  > 基于 DynamicBone 实现了一个基础版本，但穿模问题严重
+ wwise集成
+ 一个简单的UI

