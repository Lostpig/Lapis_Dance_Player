### Lapis_Dance_Player
### Lapis Re:LiGHTs 模型查看器

---
感谢[@bromine0x23](https://github.com/bromine0x23)的[Lapis Re:LiGHTs 资源挖掘备忘](https://bromine0x23.github.io/lapis-datamine-notes/lapis-datamine-notes.html)  
运行所需要使用的manifest.json和AssetBundles都来自[@bromine0x23](https://github.com/bromine0x23)的资源站[https://bromine0x23.github.io/](https://bromine0x23.github.io/)

### 注意点
必须用OpenGL模式运行，否则着色器会出错，导致渲染错误  

### 所需资源
依赖游戏的 AssetBundles, manifest.json, SoundBank  
  
manifest.json: 需要将游戏的manifest.xml解密，并转为json  

AssetBundles: 游戏的部分AssetBundle是加密的,需要提前解密;AssetBundles的目录结构不可修改  
> 在unity editor里可以直接用游戏的AssetBundle运行，editor可无视平台限制读取AB包  
> 但是build为windows平台可执行程序后，由于原AB包的target platform是Android  
> 所以需要使用[AssetTool.Net](https://github.com/nesrak1/AssetsTools.NET)修改原AssetBundle文件后才能读取  

SoundBank: 已放弃集成wwise,需要提取出音频文件,提取后的目录结构为`原bnk文件名/(wwise number).ogg`,如`MUSIC_0001/413274000.ogg`  
> 如果提取为其他格式，可以在`SoundBankLoader.LoadAudioClip`里修改相关代码  
  
---

### 配置项  
#### Config/config.json 基本配置文件
```js
{
  "manifest": "C:/lapis_relights/manifest.json",      // manifest.json 文件路径;使用"."或".."开头则认为是相对路径(相对config.json文件)
  "assetbundles": "C:/lapis_relights/AssetBundles",   // AssetBundles 资源所在目录;使用"."或".."开头则认为是相对路径(相对config.json文件)
  "soundbanks": "C:/lapis_relights/SoundBanks",       // SoundBank 资源所在目录;使用"."或".."开头则认为是相对路径(相对config.json文件)
  "soundExt": "wav",                                  // 使用的音频文件的格式,与SoundBank下的文件格式需匹配,支持ogg/mp3/wav;默认使用wav
  "physicalType": 0,                                  // 使用的物理模块版本 0:UnityChanSpringBone 1:DynamicBone,默认为0
  "qualityLevel": 5,                                  // 画质设置(0-5),就是unity默认的6档
  "fullscreen": true                                  // 是否全屏
}
```

#### Config/character.json 角色模型配置文件
> 一个完整的角色模型由 头部，身体 + 各个配件组成  
> 其中一个角色的头部是固定的，在该角色的human.bdl中  
> 身体分为两种：  
> + 通用模型,需要根据human.bdl的ScaleSettings信息调整比例;且非为_r(巨)和_l(贫),依据角色的isLoli字段判断使用哪种
> + 角色专用模型,读取后可直接使用  
```js
{
  "characters": [                           // 各个角色专用服装模型
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
  ],
  "common_actors": [                        // 通用模型
    {
      "name": "all_002",
      "body": "actors/avatars/all/ava_all_002/ava_all_suit_002/ava_all_suit_002",
      "eqiups": [
        "actors/avatars/all/ava_all_002/ava_all_hat_002/ava_{0}_hat_002"    // 一个文件内按不同角色有多个AvatarSetting的，用通配符
                                                                            // 部分按不同角色使用不同模型的，没有适配这一类，要使用需单独配置
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
      "id": "MUSIC_0002",                     // 该舞蹈的ID,为固定值
      "music": "1017980350",                  // 手工填写从soundbank中解包出的音乐文件名，或者自己拿对应的音乐文件放上也可以
      "stage": "BG2004",                      // 舞蹈对应的舞台,该项仅在魔法舞台Live中生效
      "characters": 5                         // 舞蹈对应的人数,该项仅在魔法舞台Live中生效
    }
  ]
}
```

#### Config/stage.json 场景配置文件
```js
{
  "stages": [
    {
      "id": "BG301",                              // id,唯一值
      "name": "supernova 舞台",                   // 名称,随意设置
      "prefab": "SceneAssets/Scene_stage/BG301/Prefab/BG301", // 场景资源路径
      "height": 0.01                              // 地面修正高度
    }
  ]
}
```

#### Config/mainscene.json 主界面配置
```js
{
  "stage": "BG108",             // 主界面场景ID,需要与stage.json中的场景id匹配
  "p1": "Alpha/r001",           // P1位置角色actor标识，格式为"{character.name}/{actor.name}"，需要与character.json中的匹配
  "p2": "Alpha/r001",           // P2位置角色，同上
  "p3": "Alpha/r001",           // P3位置角色，同上
  "bgms": []                    // 主界面可用BGM列表
}
```


---

### RoadMap
> ~~只是列一下，不一定能完成~~
> 搞不动了，就这样吧...
+ [✓] 模型查看
+ [✓] 口型控制模块
+ [✓] 眨眼控制模块
+ [✓] 表情控制模块
+ [✓] 动作控制模块
+ [✓] 舞蹈播放器
+ [✓] 魔法舞台基础版本
+ 物理碰撞系统
  > 基于 DynamicBone 实现了一个基础版本，但穿模问题严重  
  > 添加了用 UnityChanSpringBone 实现的另一个版本，一样有问题  
+ [x]~~wwise集成~~
+ [✓] 一个简单的UI












